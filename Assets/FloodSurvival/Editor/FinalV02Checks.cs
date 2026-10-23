using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace FloodSurvival.Editor
{
    public static class FinalV02Checks
    {
        public static string Status="Not run";
        static readonly List<string> lines=new List<string>();
        static void Check(bool condition,string message)
        {
            if(!condition)throw new Exception(message);
            lines.Add("- PASS — "+message);Write();
        }
        static void Write(){Directory.CreateDirectory("Verification/v0.2");File.WriteAllText("Verification/v0.2/FinalPresentation.md","# Final UI and equipment regression\n\nFocused setup advances story timing directly. Button callbacks are invoked programmatically; this is not a native mouse-click test.\n\n"+string.Join("\n",lines));}
        static T Read<T>(SurvivalUI ui,string name)=>(T)typeof(SurvivalUI).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ui);
        static Button Use(GameManager g,int kind)=>Read<Button[]>(g.ui,"itemButtons")[kind];
        public static async void Begin()
        {
            if(!EditorApplication.isPlaying || GameManager.Instance.State!=GameState.Menu)throw new Exception("Run at the Play mode main menu");
            int seen=PlayerPrefs.GetInt("OpeningSeenV02",0);lines.Clear();Status="Running";
            try
            {
                var g=GameManager.Instance;g.Begin();g.story.Tick(12.1f);g.story.Tick(2);g.story.StandUp();
                foreach(var kind in new[]{ItemKind.Backpack,ItemKind.Food,ItemKind.Water,ItemKind.Medicine,ItemKind.Flashlight,ItemKind.Batteries,ItemKind.Radio})g.player.inventory.Add(AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/FloodSurvival/Items/"+kind+".asset"));
                g.ToggleInventory();await Task.Delay(300);
                Check(g.InventoryOpen && !Read<GameObject>(g.ui,"hud").activeSelf,"Inventory hides the HUD");
                var captions=Read<GameObject>(g.ui,"subtitlePanel");Check(captions.activeSelf,"Reporter captions remain visible with the bag open");
                var bag=Read<RectTransform>(g.ui,"bagCard");var a=new Vector3[4];var b=new Vector3[4];bag.GetWorldCorners(a);captions.GetComponent<RectTransform>().GetWorldCorners(b);
                Check(a[0].y>b[1].y+10,"Caption panel and inventory have separate screen space");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Verification/v0.2/07-final-captioned-inventory.png"));
                float remaining=g.Remaining;Use(g,3).onClick.Invoke();await Task.Delay(2200);
                Check(g.player.LampOn && g.player.BatteryCharge<99 && g.Remaining<remaining-1,"Flashlight charge and countdown continue while the bag is open");
                Use(g,4).onClick.Invoke();Check(g.player.BatteryCharge>99.9f && g.player.inventory.Count(ItemKind.Batteries)==0,"Inventory LOAD button consumes batteries and refills charge");
                Use(g,3).onClick.Invoke();Check(!g.player.LampOn,"Inventory LIGHT button toggles the flashlight");
                foreach(int i in new[]{0,1,2})
                {
                    g.player.WaterTick(-100,3);g.player.WaterTick(100,4.5f);g.player.WaterTick(100,.1f);
                    Use(g,i).onClick.Invoke();Check(g.player.Health==100 && g.player.inventory.Count((ItemKind)i)==0,"Inventory USE button heals and consumes "+(ItemKind)i);
                }
                Use(g,6).onClick.Invoke();Check(g.AnnouncementTitle=="EMERGENCY CHANNEL 04","Inventory TUNE button plays the radio advisory");
                Check(!Use(g,5).interactable,"Worn backpack cannot be consumed");
                g.ToggleInventory();g.Escape();remaining=g.Remaining;float charge=g.player.BatteryCharge;await Task.Delay(350);
                Check(g.State==GameState.Paused && g.Remaining==remaining && g.player.BatteryCharge==charge && AudioListener.pause,"Pause freezes timer, equipment charge and audio");
                var sensitivity=Read<Slider>(g.ui,"sensitivity");var volume=Read<Slider>(g.ui,"volume");float oldSensitivity=sensitivity.value,oldVolume=volume.value;
                sensitivity.value=.08f;volume.value=.5f;Check(Mathf.Approximately(g.player.sensitivity,.08f) && Mathf.Approximately(AudioListener.volume,.5f),"Sensitivity and master-volume sliders apply their settings");sensitivity.value=oldSensitivity;volume.value=oldVolume;
                g.Resume();g.Restart();await Task.Delay(1500);g=GameManager.Instance;
                Check(g.State==GameState.Menu && g.player.inventory.Used==0 && g.player.Health==100 && !AudioListener.pause,"Final restart restores a clean main menu");
                Status="COMPLETE";lines.Add("- COMPLETE — final presentation/equipment regression passed");Write();
            }
            catch(Exception error){Status="FAIL: "+error.Message;lines.Add("- FAIL — "+error);Write();Debug.LogException(error);}
            finally{PlayerPrefs.SetInt("OpeningSeenV02",seen);if(GameManager.Instance)GameManager.Instance.ui.RefreshState();}
        }
    }
}
