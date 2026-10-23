using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace FloodSurvival.Editor
{
    // Runs real Input System events in Play mode. No QA code is shipped in the player.
    public static class PrototypeVerification
    {
        public static string Status="Not run";
        static readonly List<string> lines=new List<string>();
        static Keyboard keyboard;
        static Mouse mouse;
        static bool running;
        static bool accelerated;
        static InputSettings.BackgroundBehavior previousBackground;
        static InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
        static string ReportPath => Path.GetFullPath(accelerated?"Verification/ColdReloadPlaytest.md":"Verification/Playtest.md");
        [MenuItem("Flood Survival/Run Full Play Mode Verification")]
        public static void Begin()
        {
            if(running) return;
            if(!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first.");
            accelerated=false; running=true; Run();
        }
        public static void BeginFast()
        {
            if(running)return;
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play mode first.");
            accelerated=true; running=true; Run();
        }
        static void Record(string result)
        {
            Status=result; lines.Add(result); Directory.CreateDirectory("Verification");
            File.WriteAllText(ReportPath,"# Prototype v0.1 — automated Editor playtest\n\nUses real Input System events; teleports only arrange pickup/hazard scenarios.\n"+(accelerated?"Regression after unloading/reloading item assets from disk. Countdown/flood accelerated at timeScale 15; movement and interaction use normal time.":"The first countdown and flood run at normal time.")+"\n\n"+string.Join("\n",lines.ConvertAll(x=>"- "+x))+"\n");
        }
        static void Check(bool condition,string message) { if(!condition) throw new Exception(message); Record("PASS — "+message); }
        static Task Wait(float seconds) => Task.Delay(Mathf.CeilToInt(seconds*1000));
        static async Task WaitGame(float seconds) { float target=Time.time+seconds; while(Time.time<target) { await Task.Delay(20); if(!EditorApplication.isPlaying)throw new Exception("Play mode interrupted"); } }
        static void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys)); }
        static void IsolateTestInput(FirstPersonController player,bool enabled)
        {
            if(!player)return;
            foreach(string name in new[]{"move","look","sprint","jump","interact","bag","pause","lamp","medicine"})
            {
                var action=(InputAction)typeof(FirstPersonController).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(player);
                action.RemoveAllBindingOverrides();
                if(!enabled)continue;
                for(int i=0;i<action.bindings.Count;i++)
                {
                    string path=action.bindings[i].path;
                    if(path.StartsWith("<Keyboard>"))action.ApplyBindingOverride(i,path.Replace("<Keyboard>","<Keyboard>{FloodVerification}"));
                    if(path.StartsWith("<Mouse>"))action.ApplyBindingOverride(i,path.Replace("<Mouse>","<Mouse>{FloodVerification}"));
                }
            }
        }
        static async Task Tap(Key key) { Keys(key); await Wait(.12f); Keys(); await Wait(.14f); }
        static void Capture(string name) { Directory.CreateDirectory("Verification"); ScreenCapture.CaptureScreenshot(Path.GetFullPath("Verification/"+(accelerated?"cold-":"")+name+".png")); }
        static async Task Collect(ItemKind kind)
        {
            var game=GameManager.Instance; PickupItem chosen=null;
            foreach(var item in UnityEngine.Object.FindObjectsByType<PickupItem>())if(item.definition.kind==kind){chosen=item;break;}
            if(!chosen)throw new Exception("Missing pickup "+kind);
            game.player.Teleport(new Vector3(chosen.transform.position.x,.25f,chosen.transform.position.z-2.4f));
            Vector3 direction=chosen.transform.position-game.player.view.transform.position;
            game.player.transform.rotation=Quaternion.Euler(0,Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg,0);
            typeof(FirstPersonController).GetField("pitch",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(game.player,-Mathf.Atan2(direction.y,new Vector2(direction.x,direction.z).magnitude)*Mathf.Rad2Deg);
            await Wait(.3f); Check(game.player.FocusedItem==chosen,"Raycast identifies "+kind+" within reach");
            int before=game.player.inventory.Count(kind); await Tap(Key.E);
            Check(game.player.inventory.Count(kind)==before+1 && chosen.Collected,"E collects "+kind+" exactly once");
            Check(!chosen.Collect(game.player.inventory),"Duplicate collection rejected for "+kind);
        }
        static async void Run()
        {
            lines.Clear();
            try
            {
                previousBackground=InputSystem.settings.backgroundBehavior; previousEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard=InputSystem.AddDevice<Keyboard>("FloodVerificationKeyboard"); mouse=InputSystem.AddDevice<Mouse>("FloodVerificationMouse");
                InputSystem.SetDeviceUsage(keyboard,"FloodVerification"); InputSystem.SetDeviceUsage(mouse,"FloodVerification");
                var game=GameManager.Instance; Check(game && game.State==GameState.Menu,"Main menu loads with wired systems");
                IsolateTestInput(game.player,true);
                int frame=Time.frameCount; await Wait(.3f); Check(Time.frameCount>frame+2,"Player loop advances while Editor is unfocused");
                GameObject.Find("Start").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                float started=Time.time; Check(game.State==GameState.Preparation && Mathf.Abs(game.Remaining-150)<.1f,"Begin button starts a 150-second emergency");
                Check(game.audioSystem.sirens.Length>0 && game.audioSystem.sirens[0].isPlaying,"Spatial emergency sirens play");
                Check(!string.IsNullOrEmpty(game.AnnouncementTitle),"Emergency broadcast appears"); Capture("02-emergency");
                game.player.transform.rotation=Quaternion.identity;
                var start=game.player.transform.position; Keys(Key.W); await WaitGame(1); Keys(); await Wait(.1f);
                Check(Vector3.Distance(start,game.player.transform.position)>4,"WASD moves CharacterController");
                float yaw=game.player.transform.eulerAngles.y; InputSystem.QueueDeltaStateEvent(mouse.delta,new Vector2(100,-20)); await Wait(.2f);
                Check(Mathf.Abs(Mathf.DeltaAngle(yaw,game.player.transform.eulerAngles.y))>2,"Mouse delta rotates first-person view");
                float ground=game.player.transform.position.y; Keys(Key.Space); await WaitGame(.28f); Keys(); Check(game.player.transform.position.y>ground+.5f,"Space produces a grounded jump"); await WaitGame(.9f);
                start=game.player.transform.position; Keys(Key.W,Key.LeftShift); await WaitGame(1); Keys(); await Wait(.15f); Check(Vector3.Distance(start,game.player.transform.position)>7,"Shift sprint increases movement speed");
                await Tap(Key.Tab); Check(game.InventoryOpen && GameObject.Find("Inventory"),"Tab opens inventory UI"); await Tap(Key.Tab); Check(!game.InventoryOpen,"Tab closes inventory UI");
                await Tap(Key.Escape); float paused=game.Remaining; await Wait(.6f); Check(game.State==GameState.Paused && Mathf.Abs(paused-game.Remaining)<.001f,"Escape pauses countdown and gameplay"); await Tap(Key.Escape); Check(game.IsPlaying && Time.timeScale==1,"Escape safely resumes gameplay");
                game.player.SetSensitivity(.14f); Check(Mathf.Approximately(game.player.sensitivity,.14f),"Mouse sensitivity is adjustable"); game.player.SetSensitivity(.12f);
                await Collect(ItemKind.Food); await Collect(ItemKind.Water); await Collect(ItemKind.Medicine); await Collect(ItemKind.Flashlight);
                Check(game.player.inventory.Prepared,"All four supply types appear in inventory"); Check(game.objectives.ReturnedHome,"Returning to the house completes home objective");
                await Tap(Key.F); Check(game.player.LampOn,"F equips and activates collected flashlight"); await Tap(Key.F); Check(!game.player.LampOn,"F toggles flashlight off");
                await Tap(Key.Tab); Capture("03-inventory"); await Tap(Key.Tab);
                // Test a solid building wall, then walk up the entire evacuation slope.
                game.player.Teleport(new Vector3(21,.25f,23)); game.player.transform.rotation=Quaternion.identity; Keys(Key.W); await WaitGame(1.2f); Keys(); Check(game.player.transform.position.z<24.5f,"CharacterController collides with market back wall");
                game.player.Teleport(new Vector3(35,.5f,-46)); game.player.transform.rotation=Quaternion.Euler(0,90,0); Keys(Key.W,Key.LeftShift); await WaitGame(7.4f); Keys(); await Wait(.3f);
                Check(game.player.transform.position.x>84 && game.player.transform.position.y>14,"Player can sprint continuously from street to shelter terrace");
                game.player.Teleport(game.objectives.highGround.position+Vector3.up*.2f);
                foreach(var npc in game.npcs.citizens)
                {
                    var agent=npc.GetComponent<UnityEngine.AI.NavMeshAgent>(); var path=new UnityEngine.AI.NavMeshPath();
                    Check(npc.Panicking && agent.isOnNavMesh && agent.CalculatePath(npc.destination,path) && path.status==UnityEngine.AI.NavMeshPathStatus.PathComplete,"Evacuee has complete shelter path: "+npc.name);
                }
                if(accelerated)Time.timeScale=15;
                Record(accelerated?"RUNNING — accelerated countdown/flood regression":"RUNNING — waiting for the natural 150-second countdown at timeScale 1");
                while(game.State==GameState.Preparation) { await Wait(.25f); if(!EditorApplication.isPlaying)throw new Exception("Play mode interrupted"); }
                Check(game.State==GameState.Flood && Time.time-started>=149.9f,accelerated?"150 gameplay seconds trigger flood after disk reload":"Countdown expires naturally and triggers flood");
                Check(!game.flood.damCenter.activeSelf && game.flood.water.gameObject.activeSelf,"Dam breach removes central section and activates water");
                Check(game.objectives.Target==game.objectives.highGround,"Objective changes to high ground");
                // Looking back down the valley provides a visual record of the rising surface.
                game.player.transform.rotation=Quaternion.Euler(0,-90,0); typeof(FirstPersonController).GetField("pitch",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(game.player,18f);
                float previous=game.flood.Level; await WaitGame(12); Check(game.flood.Level>previous+1,accelerated?"Water level rises progressively after disk reload":"Water level rises progressively at normal time"); Capture("04-rising-water");
                await WaitGame(25); Capture("05-flooded-town");
                Record("RUNNING — waiting for surge stabilization and victory");
                while(game.State==GameState.Flood) { await Wait(.25f); if(!EditorApplication.isPlaying)throw new Exception("Play mode interrupted"); }
                Check(game.State==GameState.Victory && Mathf.Abs(game.flood.Level-9)<.01f,"High ground survives the full 75-second rising surge"); Capture("06-victory");
                game.Restart(); await Wait(1.6f); game=GameManager.Instance;
                IsolateTestInput(game.player,true);
                Check(game.State==GameState.Menu && game.Remaining==150 && game.player.Health==100 && game.player.inventory.Count(ItemKind.Food)==0 && !game.flood.IsFlooding,"Restart restores menu, timer, health, empty bag, water and references");
                int pickups=UnityEngine.Object.FindObjectsByType<PickupItem>().Length; Check(pickups==8,"Restart restores all eight pickups");
                game.Begin(); game.Breach(); game.flood.Tick(75); game.player.Teleport(new Vector3(0,.2f,-30)); await WaitGame(5.2f);
                Check(game.player.Oxygen==0 && game.player.Health<100,"Submersion depletes air and applies drowning damage");
                float beforeHeal=game.player.Health; PickupItem med=null;foreach(var p in UnityEngine.Object.FindObjectsByType<PickupItem>())if(p.definition.kind==ItemKind.Medicine){med=p;break;}
                med.Collect(game.player.inventory); game.player.UseMedicine(); Check(game.player.Health>beforeHeal && game.player.inventory.Count(ItemKind.Medicine)==0,"Medicine restores health and consumes one item");
                // Poll state because defeat pauses Time.time.
                float deadline=Time.realtimeSinceStartup+30; while(game.State==GameState.Flood && Time.realtimeSinceStartup<deadline)await Wait(.15f); Check(game.State==GameState.Defeat,"Remaining submerged produces game-over screen"); Capture("07-game-over");
                game.Restart(); await Wait(1.4f); game=GameManager.Instance; Check(game.State==GameState.Menu && Time.timeScale==1 && game.player.Oxygen==100,"Restart also recovers from drowning defeat");
                Record("COMPLETE — all automated gameplay checks passed. Native audio is synthesized; subjective listening and hardware performance remain manual checks.");
            }
            catch(Exception error) { Record("FAIL — "+error); Debug.LogException(error); }
            finally
            {
                if(GameManager.Instance)IsolateTestInput(GameManager.Instance.player,false);
                if(keyboard!=null)InputSystem.RemoveDevice(keyboard); if(mouse!=null)InputSystem.RemoveDevice(mouse);
                InputSystem.settings.backgroundBehavior=previousBackground; InputSystem.settings.editorInputBehaviorInPlayMode=previousEditorInput;
                running=false;
            }
        }
    }
}

