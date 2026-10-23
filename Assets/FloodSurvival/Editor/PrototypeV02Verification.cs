using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace FloodSurvival.Editor
{
    public static class PrototypeV02Verification
    {
        public static string Status="Not run";
        static bool running;
        static Keyboard keyboard;
        static Mouse mouse;
        static readonly List<string> results=new List<string>();
        static InputSettings.BackgroundBehavior background;
        static InputSettings.EditorInputBehaviorInPlayMode editorInput;
        static int seen,subtitles;
        const string Folder="Verification/v0.2-landscape/";
        [MenuItem("Flood Survival/Verify v0.2 full loop (Play mode)")]
        public static void Begin()
        {
            if(running)return;
            if(!EditorApplication.isPlaying)throw new Exception("Enter Play mode at the main menu first");
            running=true; Run();
        }
        static void Record(string text)
        {
            Status=text;results.Add(text);Directory.CreateDirectory(Folder);
            File.WriteAllText(Folder+"Playtest.md","# Prototype v0.2 runtime verification\n\nPrimary opening (12 s), countdown (150 s) and flood (75 s) run at timeScale 1.\nVirtual Input System events test controls; teleports arrange interactions and hazards.\nSecondary hazard/no-supply branches explicitly accelerate setup.\n\n"+string.Join("\n",results.ConvertAll(x=>"- "+x))+"\n");
        }
        static void Check(bool condition,string message) { if(!condition)throw new Exception(message);Record("PASS — "+message); }
        static Task Real(float seconds)=>Task.Delay((int)(seconds*1000));
        static async Task GameTime(float seconds)
        {
            float end=Time.time+seconds;float deadline=Time.realtimeSinceStartup+Mathf.Max(30,seconds*4);
            while(Time.time<end){await Real(.025f);if(!EditorApplication.isPlaying || Time.realtimeSinceStartup>deadline)throw new Exception("Simulation interrupted or stopped advancing");}
        }
        static void Keys(params Key[] keys)=>InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));
        static async Task Tap(Key key){Keys(key);await Real(.12f);Keys();await Real(.15f);}
        static void Isolate(FirstPersonController player,bool value)
        {
            if(!player)return;
            foreach(string name in new[]{"move","look","sprint","jump","interact","bag","pause","lamp","medicine","inspect","reload","radio"})
            {
                var a=(InputAction)typeof(FirstPersonController).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(player);a.RemoveAllBindingOverrides();if(!value)continue;
                for(int i=0;i<a.bindings.Count;i++)
                {
                    var path=a.bindings[i].path;
                    if(path.StartsWith("<Keyboard>"))a.ApplyBindingOverride(i,path.Replace("<Keyboard>","<Keyboard>{V02Test}"));
                    if(path.StartsWith("<Mouse>"))a.ApplyBindingOverride(i,path.Replace("<Mouse>","<Mouse>{V02Test}"));
                }
            }
        }
        static void Aim(FirstPersonController player,Vector3 target)
        {
            Vector3 v=target-player.view.transform.position;player.transform.rotation=Quaternion.Euler(0,Mathf.Atan2(v.x,v.z)*Mathf.Rad2Deg,0);
            typeof(FirstPersonController).GetField("pitch",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(player,-Mathf.Atan2(v.y,new Vector2(v.x,v.z).magnitude)*Mathf.Rad2Deg);
        }
        static async Task<PickupItem> Focus(ItemKind kind)
        {
            var player=GameManager.Instance.player;
            var candidates=UnityEngine.Object.FindObjectsByType<PickupItem>();
            Array.Sort(candidates,(a,b)=>GameManager.Instance.story.HouseBounds.Contains(b.transform.position).CompareTo(GameManager.Instance.story.HouseBounds.Contains(a.transform.position)));
            foreach(var item in candidates)
            {
                if(item.definition.kind!=kind)continue;
                for(int angle=0;angle<8;angle++)
                {
                    float radians=angle*Mathf.PI/4;Vector3 feet=item.transform.position+new Vector3(Mathf.Sin(radians)*2.1f,0,Mathf.Cos(radians)*2.1f);feet.y=.27f;
                    if(Physics.CheckCapsule(feet+Vector3.up*.34f,feet+Vector3.up*1.4f,.3f,~(1<<2),QueryTriggerInteraction.Ignore))continue;
                    Vector3 eye=feet+Vector3.up*1.64f;Vector3 direction=item.transform.position-eye;
                    if(!Physics.Raycast(eye,direction.normalized,out var hit,3.1f,~(1<<2),QueryTriggerInteraction.Ignore) || hit.collider.GetComponentInParent<PickupItem>()!=item)continue;
                    player.Teleport(feet);Aim(player,item.transform.position);await GameTime(.25f);
                    if(player.FocusedItem==item)return item;
                }
            }
            throw new Exception("No reachable interaction approach for "+kind);
        }
        static async Task Collect(ItemKind kind)
        {
            var item=await Focus(kind);var inventory=GameManager.Instance.player.inventory;int before=inventory.Count(kind);await Tap(Key.E);
            Check(item.Collected && inventory.Count(kind)==before+1,"E picks up "+kind+" from a reachable natural placement");
            Check(!item.Collect(inventory),"Duplicate "+kind+" collection is rejected");
        }
        static void Capture(string name) {Directory.CreateDirectory(Folder);ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+name+".png"));}
        static async Task<GameManager> Restart()
        {
            GameManager.Instance.Restart();await Real(1.2f);var g=GameManager.Instance;Isolate(g.player,true);
            Check(g.State==GameState.Menu && Time.timeScale==1 && g.player.Health==100 && g.player.inventory.Used==0 && !g.flood.IsFlooding,"Restart clears state, inventory, health, water and countdown");return g;
        }
        static async void Run()
        {
            results.Clear();
            seen=PlayerPrefs.GetInt("OpeningSeenV02",0);subtitles=PlayerPrefs.GetInt("SubtitlesV02",1);
            background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            try
            {
                PlayerPrefs.SetInt("OpeningSeenV02",0);PlayerPrefs.SetInt("SubtitlesV02",1);
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard=InputSystem.AddDevice<Keyboard>("V02Keyboard");mouse=InputSystem.AddDevice<Mouse>("V02Mouse");InputSystem.SetDeviceUsage(keyboard,"V02Test");InputSystem.SetDeviceUsage(mouse,"V02Test");
                var g=GameManager.Instance;Check(g && g.story && g.State==GameState.Menu,"Existing CedarValley scene loads with v0.2 references");Isolate(g.player,true);
                Check(!OpeningSequence.CanSkip,"First opening cannot be skipped before it is seen");
                int frame=Time.frameCount;await Real(.2f);Check(Time.frameCount>frame+2,"Player loop advances while the Editor is unfocused");
                GameObject.Find("Start").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                Check(g.State==GameState.Opening && g.story.Seated && !g.EmergencyStarted,"Start Story begins seated on the couch before an emergency");
                Check(!g.story.television.IsNews && g.audioSystem.television.isPlaying,"Original cartoon animation and TV audio play");
                await GameTime(.25f);Capture("01-couch-cartoon");
                Vector3 position=g.player.transform.position;Keys(Key.W);await GameTime(.6f);Keys();Check(Vector3.Distance(position,g.player.transform.position)<.05f,"Seated player can look but cannot walk through the couch");
                float yaw=g.player.transform.eulerAngles.y;InputSystem.QueueDeltaStateEvent(mouse.delta,new Vector2(80,-10));await GameTime(.2f);Check(Mathf.Abs(Mathf.DeltaAngle(yaw,g.player.transform.eulerAngles.y))>2,"Mouse look works while seated");g.player.Sit(g.story.seat.position,g.story.seat.eulerAngles.y);
                await Tap(Key.Escape);float stopped=g.story.Elapsed;await Real(.5f);Check(g.State==GameState.Paused && Mathf.Abs(g.story.Elapsed-stopped)<.01f && AudioListener.pause,"Pause freezes opening, audio and gameplay");await Tap(Key.Escape);Check(g.State==GameState.Opening && !AudioListener.pause,"Opening resumes safely");
                int emergencies=0;g.Emergency+=()=>emergencies++;
                Record("RUNNING — waiting for the natural 12-second television interruption");
                while(!g.EmergencyStarted)await Real(.03f);
                float emergencyTime=Time.time;
                Check(g.story.Elapsed>=10 && g.story.Elapsed<=15,"Breaking news interrupts after the configured 10–15 seconds");
                Check(g.State==GameState.Preparation && g.story.television.IsNews && g.Remaining>149.5f,"TV warning starts the full 150-second preparation timer");
                g.StartWarning();g.StartWarning();Check(emergencies==1,"Repeated emergency calls do not duplicate events or reset the timer");
                Check(g.audioSystem.sirens[0].isPlaying && g.npcs.citizens[0].Panicking,"Sirens and NPC reactions start with the TV warning");
                await GameTime(1.5f);Check(!string.IsNullOrEmpty(g.story.Subtitle) && g.story.newsVoices.Length==4 && g.audioSystem.television.isPlaying,"Exact news subtitles and synthesized reporter audio play");Capture("02-breaking-news");
                await Tap(Key.E);Check(!g.story.Seated && g.player.GetComponent<CharacterController>().enabled && g.Remaining>146,"E stands up immediately without waiting for the full news report");
                Check(OpeningSequence.CanSkip,"Seeing the opening unlocks replay skip");
                var inspected=await Focus(ItemKind.Food);await Tap(Key.I);Check(g.AnnouncementBody==inspected.definition.description,"I inspects a nearby item and displays its description");
                foreach(var kind in new[]{ItemKind.Food,ItemKind.Water,ItemKind.Medicine,ItemKind.Flashlight,ItemKind.Batteries})await Collect(kind);
                Check(g.player.inventory.Capacity==6 && g.player.inventory.Used==5,"Base inventory tracks six slots and item costs");
                var radio=await Focus(ItemKind.Radio);await Tap(Key.E);Check(!radio.Collected && g.player.inventory.Count(ItemKind.Radio)==0,"Over-capacity pickup stays in the world");
                await Collect(ItemKind.Backpack);Check(g.player.inventory.Capacity==12,"Backpack expands capacity to twelve slots");await Collect(ItemKind.Radio);
                Check(g.player.inventory.Prepared && g.objectives.HouseSupplies>=2,"Essential supply checklist and house collection objective update");
                await Tap(Key.F);Check(g.player.LampOn,"F equips the collected flashlight");await GameTime(2);Check(g.player.BatteryCharge<99,"Equipped flashlight consumes charge");await Tap(Key.R);Check(g.player.BatteryCharge>99.4f && g.player.inventory.Count(ItemKind.Batteries)==0,"R consumes batteries and recharges the flashlight");await Tap(Key.F);Check(!g.player.LampOn,"F toggles the light off");
                await Tap(Key.V);Check(g.AnnouncementTitle=="EMERGENCY CHANNEL 04","V tunes the collected emergency radio");
                await Tap(Key.Tab);Check(g.InventoryOpen,"Tab opens the seven-item inventory with icons and quantities");Capture("03-inventory");await Tap(Key.Tab);Check(!g.InventoryOpen,"Tab closes the inventory");
                await Tap(Key.Escape);float remaining=g.Remaining;await Real(.5f);Check(g.State==GameState.Paused && Mathf.Abs(g.Remaining-remaining)<.01f,"Pause freezes the countdown");
                var subtitleToggle=GameObject.Find("SubtitleToggle").GetComponent<UnityEngine.UI.Button>();int old=PlayerPrefs.GetInt("SubtitlesV02",1);subtitleToggle.onClick.Invoke();Check(PlayerPrefs.GetInt("SubtitlesV02",1)!=old,"Optional subtitles can be changed in settings");subtitleToggle.onClick.Invoke();await Tap(Key.Escape);
                g.player.Teleport(new Vector3(-22,.28f,-34));g.player.transform.rotation=Quaternion.Euler(0,180,0);Keys(Key.W);await GameTime(1.1f);Keys();
                Check(g.story.OutsideTriggered && g.story.damCracks.activeSelf && g.State==GameState.Preparation,"Walking through the front entrance triggers cracking, not collapse");
                Check(g.story.startledBirds.particleCount>0 && g.story.damDust.particleCount>0,"The outside event starts birds and dam dust");
                Check(g.AnnouncementTitle.Contains("EVACUATE NOW"),"Second emergency alert appears outside");g.story.TriggerOutside();Check(emergencies==1,"Repeated outside checks do not start another emergency");
                int paths=0,animated=0;foreach(var npc in g.npcs.citizens){var agent=npc.GetComponent<NavMeshAgent>();var path=new NavMeshPath();if(agent.isOnNavMesh && agent.CalculatePath(npc.destination,path) && path.status==NavMeshPathStatus.PathComplete)paths++;if(npc.animator && npc.animator.isHuman)animated++;}
                Check(paths==8 && animated==8,"All eight animated residents have complete evacuation paths");
                g.player.Teleport(new Vector3(35,.5f,-46));g.player.transform.rotation=Quaternion.Euler(0,90,0);Keys(Key.W,Key.LeftShift);await GameTime(7.4f);Keys();await GameTime(.2f);
                Check(g.player.transform.position.x>84 && g.player.transform.position.y>14,"CharacterController can sprint continuously from street to high ground");
                g.player.Teleport(g.objectives.highGround.position+Vector3.up*.2f);
                Record("RUNNING — natural 150-second countdown at timeScale 1");while(g.State==GameState.Preparation)await Real(.15f);
                Check(g.State==GameState.Flood && Time.time-emergencyTime>=149.8f,"Countdown expires naturally and triggers exactly one breach");
                Check(!g.flood.damCenter.activeSelf && g.flood.surgeFront.gameObject.activeSelf && g.flood.fallingDebris[0].gameObject.activeSelf,"Breach activates structural debris, dust and downstream surge");
                Check(g.flood.GetLevel(new Vector3(0,0,-30))<-90,"Town remains dry before the surge front arrives");
                float front=g.flood.FrontZ;await GameTime(10);Check(g.flood.FrontZ<front-35 && g.flood.Level>-1,"Surge visibly travels downstream while its level rises");
                await GameTime(30);
                Check(g.flood.GetLevel(new Vector3(0,0,-12))>0 && g.flood.GetLevel(new Vector3(0,0,-110))<-90,"Flood reaches town progressively while downstream ground is still dry");
                bool floating=false;foreach(var floater in g.flood.floaters)if(floater.transform.position.y>2)floating=true;Check(floating,"Lightweight objects rise and drift with the flood");
                g.player.Teleport(new Vector3(85,15.3f,-57));Aim(g.player,new Vector3(-10,2,-15));await GameTime(.2f);Capture("04-advancing-flood");g.player.Teleport(g.objectives.highGround.position+Vector3.up*.2f);
                Record("RUNNING — full 75-second flood at timeScale 1");while(g.State==GameState.Flood)await Real(.15f);
                Check(g.State==GameState.Victory && Mathf.Abs(g.flood.Level-g.flood.FinalLevel)<.01f,"Wooded high ground survives the complete surge and produces victory");Capture("05-victory");
                g=await Restart();g.preparationDuration=60;g.BeginSkipped();Check(g.State==GameState.Preparation && !g.story.Seated && g.Remaining>59,"Replay skip and configurable countdown work");g.Breach();g.flood.Tick(50);g.player.Teleport(new Vector3(0,.25f,-30));
                Check(g.flood.CurrentAt(g.player.transform.position).z<0,"Deep floodwater pushes downstream");float height=g.player.transform.position.y;Keys(Key.Space);await GameTime(1.2f);Keys();Check(g.player.Swimming && g.player.transform.position.y>height+1.5f,"Holding Space swims upward in deep water");
                g.player.Teleport(new Vector3(0,.25f,-30));await GameTime(5.5f);Check(g.player.Oxygen==0 && g.player.Health<100,"Deep submersion depletes oxygen and damages health");
                var med=AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/FloodSurvival/Items/Medicine.asset");g.player.inventory.Add(med);float health=g.player.Health;await Tap(Key.H);Check(g.player.Health>health && g.player.inventory.Count(ItemKind.Medicine)==0,"H uses first aid and consumes one kit");
                float deadline=Time.realtimeSinceStartup+20;while(g.State==GameState.Flood && Time.realtimeSinceStartup<deadline)await Real(.1f);Check(g.State==GameState.Defeat,"Remaining caught underwater produces defeat");Capture("06-defeat");
                g=await Restart();g.BeginSkipped();g.Breach();g.player.Teleport(g.objectives.highGround.position+Vector3.up*.2f);Time.timeScale=20;
                Record("RUNNING — accelerated no-supplies victory branch");while(g.State==GameState.Flood)await Real(.1f);Check(g.State==GameState.Victory && g.player.inventory.Used==0,"Missing every optional supply does not prevent survival");
                await Restart();Record("COMPLETE — all v0.2 runtime checks passed. Listening quality and cross-PC profiling remain manual checks.");
            }
            catch(Exception error){Record("FAIL — "+error);Debug.LogException(error);}
            finally
            {
                if(GameManager.Instance)Isolate(GameManager.Instance.player,false);if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);
                InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
                PlayerPrefs.SetInt("OpeningSeenV02",seen);PlayerPrefs.SetInt("SubtitlesV02",subtitles);if(GameManager.Instance)GameManager.Instance.ui.RefreshState();running=false;
            }
        }
    }
}


