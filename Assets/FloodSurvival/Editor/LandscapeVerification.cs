using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
namespace FloodSurvival.Editor
{
    public static class LandscapeVerification
    {
        const string Folder="Verification/v0.2-landscape/";
        public static string Status="Not run";
        static List<string> results=new List<string>();
        static void Check(bool condition,string text){if(!condition)throw new Exception(text);results.Add("- PASS — "+text);Write();}
        static void Write(){Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"RevisionChecks.md","# Landscape / TV focused checks\n\nRuntime story/flood setup is accelerated explicitly. Primary normal-speed gameplay evidence is in Playtest.md.\n\n"+string.Join("\n",results));}
        public static string Saved()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play mode first");
            results.Clear();AssetPersistenceVerification.Verify();
            var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var terrain=UnityEngine.Object.FindFirstObjectByType<Terrain>();
            int old=0;foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>())if(t.name.StartsWith("Shelter ") || t.name=="Canopy pillar" || t.name=="Mountain ridge" || t.name=="Evacuation plateau")old++;
            Check(old==0,"Shelter structures, rectangular plateau and rounded mountain placeholders are absent");
            foreach(var text in UnityEngine.Object.FindObjectsByType<TMP_Text>())if(text.text.IndexOf("shelter",StringComparison.OrdinalIgnoreCase)>=0)throw new Exception("Stale shelter text: "+text.name);
            Check(true,"Visible in-game text no longer directs players to a shelter");
            float highest=0;int houses=0;foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>())if(t.name.StartsWith("Resident house ") || t.name=="Roof"){foreach(var r in t.GetComponentsInChildren<Renderer>())highest=Mathf.Max(highest,r.bounds.max.y);houses++;}
            Check(houses==11 && g.flood.FinalLevel>=highest && Mathf.Abs(g.flood.FinalLevel-10.6f)<.01f,"Final 10.6 m water reaches all eleven house roofs (highest "+highest.ToString("0.00")+" m)");
            Check(terrain && terrain.terrainData.heightmapResolution==513 && terrain.terrainData.terrainLayers.Length==3 && AssetDatabase.Contains(terrain.terrainData),"Continuous terrain and three blended ground/rock layers survive disk reload");
            string manifest=File.ReadAllText("Assets/FloodSurvival/Landscape/ForestManifest.json");int trees=int.Parse(System.Text.RegularExpressions.Regex.Match(manifest,"\"trees\":([0-9]+)").Groups[1].Value);
            Check(trees>=1400,"Dense forest contains "+trees+" authored pines combined into spatial patches");
            var path=new NavMeshPath();Check(NavMesh.CalculatePath(new Vector3(35,.3f,-46),g.objectives.highGround.position,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,"Town-to-wooded-hill navigation remains complete");
            int routes=0;foreach(var n in g.npcs.citizens)if(NavMesh.SamplePosition(n.transform.position,out var hit,2,NavMesh.AllAreas) && NavMesh.CalculatePath(hit.position,n.destination,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete)routes++;
            Check(routes==8,"All eight residents can reach their new open high-ground destinations");
            var tv=g.story.television;var rt=tv.screenCamera.targetTexture;var canvas=tv.cartoon.GetComponentInParent<Canvas>();
            Check(rt && rt.width==1280 && rt.height==720 && AssetDatabase.Contains(rt),"TV uses a persisted 1280 x 720 live screen");
            Check(canvas.transform.parent==tv.screenCamera.transform && Vector3.Distance(canvas.transform.position,tv.screenCamera.transform.TransformPoint(new Vector3(0,0,8)))<.01f,"TV programme framing survives scene reload");
            Check((g.player.view.cullingMask&(1<<30))==0 && tv.screenCamera.cullingMask==1<<30,"TV programme and game cameras render separate layers");
            Check(tv.anchorMouth && tv.anchorEyes && tv.tail && tv.leftWing && tv.rightWing && tv.tickerPeriod>500 && tv.ticker.text.Length>200,"Presenter/cartoon animation and repeated scrolling ticker references are connected");
            Status="Saved checks passed";return string.Join("\n",results);
        }
        static void Aim(FirstPersonController p,Vector3 target)
        {
            var v=target-p.view.transform.position;p.transform.rotation=Quaternion.Euler(0,Mathf.Atan2(v.x,v.z)*Mathf.Rad2Deg,0);typeof(FirstPersonController).GetField("pitch",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(p,-Mathf.Atan2(v.y,new Vector2(v.x,v.z).magnitude)*Mathf.Rad2Deg);
        }
        static bool PictureHasColour(RenderTexture target)
        {
            var before=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(target.width,target.height);image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();int colourful=0;for(int y=80;y<640;y+=70)for(int x=80;x<1200;x+=80){Color c=image.GetPixel(x,y);if(c.maxColorComponent>.15f && c.maxColorComponent-Mathf.Min(c.r,Mathf.Min(c.g,c.b))>.07f)colourful++;}UnityEngine.Object.Destroy(image);RenderTexture.active=before;return colourful>10;
        }
        public static async void Runtime()
        {
            if(!EditorApplication.isPlaying || GameManager.Instance.State!=GameState.Menu)throw new Exception("Start at the Play mode main menu");
            int seen=PlayerPrefs.GetInt("OpeningSeenV02",0);Status="Running";
            try
            {
                var g=GameManager.Instance;int frames=Time.frameCount;g.Begin();await Task.Delay(700);
                Check(Time.frameCount>frames+4,"Final scene runs in Play mode while unfocused");
                var tv=g.story.television;Check(PictureHasColour(tv.screenCamera.targetTexture),"The fitted television actually renders a coloured cartoon frame");
                var fox=tv.fox.anchoredPosition;await Task.Delay(300);Check(Vector2.Distance(fox,tv.fox.anchoredPosition)>.1f,"Cartoon characters animate on the live screen");ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"TV-cartoon-final.png"));await Task.Delay(180);
                g.story.Tick(12.1f);g.story.Tick(2);await Task.Delay(650);Check(tv.IsNews && PictureHasColour(tv.screenCamera.targetTexture),"Breaking news switches the live display to the illustrated presenter and dam map");
                var ticker=tv.tickerTrack.anchoredPosition;await Task.Delay(250);Check(Vector2.Distance(ticker,tv.tickerTrack.anchoredPosition)>5,"Evacuation ticker scrolls continuously");ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"TV-news-final.png"));await Task.Delay(180);
                g.Escape();fox=tv.fox.anchoredPosition;float remain=g.Remaining;await Task.Delay(300);Check(g.State==GameState.Paused && g.Remaining==remain && tv.fox.anchoredPosition==fox && AudioListener.pause,"Pause freezes the warning, programme and audio");g.Resume();g.story.StandUp();
                var terrain=UnityEngine.Object.FindFirstObjectByType<Terrain>();g.player.Teleport(g.objectives.highGround.position+Vector3.up*.3f);Aim(g.player,new Vector3(-10,7,18));g.Breach();g.flood.Tick(g.flood.RiseDuration);await Task.Delay(400);
                Check(Mathf.Abs(g.flood.Level-10.6f)<.01f && g.player.Health==100,"Roof-level flood stabilizes at 10.6 m while the wooded hill stays dry");
                Check(g.AnnouncementTitle=="THE DAM HAS COLLAPSED","Leaving the house after a breach cannot replay the pre-collapse cracking alert");
                Check(!tv.screenCamera.enabled,"TV rendering stops when the player is away from the house");ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"Roof-level-flood.png"));await Task.Delay(180);
                Vector3 remote=Vector3.zero;bool found=false;for(int x=-235;x<-115 && !found;x+=5)for(int z=-130;z<100;z+=5)
                {
                    var p=new Vector3(x,0,z);float h=terrain.SampleHeight(p)+terrain.transform.position.y;float slope=terrain.terrainData.GetSteepness((x+360)/720f,(z+340)/720f);if(h>20 && slope<16){remote=new Vector3(x,h+.35f,z);found=true;break;}
                }
                Check(found && Vector3.Distance(remote,g.objectives.highGround.position)>100,"A separate dry mountain location is available far from the old shelter");
                g.player.Teleport(remote);g.flood.RiseDuration=2;await Task.Delay(2700);Check(g.State==GameState.Victory,"Standing on other dry high ground wins without any shelter proximity check");
                g.Restart();await Task.Delay(1400);g=GameManager.Instance;Check(g.State==GameState.Menu && g.player.inventory.Used==0 && g.flood.FinalLevel==10.6f && !g.flood.IsFlooding && g.story.television.screenCamera,"Restart restores final terrain/TV references and clears flood state");
                Status="COMPLETE";results.Add("- COMPLETE — final landscape and TV checks passed");Write();
            }
            catch(Exception error){Status="FAIL: "+error.Message;results.Add("- FAIL — "+error);Write();Debug.LogException(error);}
            finally{PlayerPrefs.SetInt("OpeningSeenV02",seen);if(GameManager.Instance)GameManager.Instance.ui.RefreshState();}
        }
    }
}
