using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem.UI;
using TMPro;
namespace FloodSurvival.Editor
{
    public static class PrototypeBuilder
    {
        const string Root="Assets/FloodSurvival";
        static Transform world;
        static TMP_FontAsset font;
        static Material grass,asphalt,concrete,cream,roof,wood,glass,amber,white,metal,red,atlas,rock;
        static readonly List<AudioSource> sirens=new List<AudioSource>();
        [MenuItem("Flood Survival/Build Prototype Scene")]
        public static void Build()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before building.");
            foreach(string folder in new[]{"Scenes","Materials","Prefabs","Items","Settings"}) Directory.CreateDirectory(Root+"/"+folder);
            AssetDatabase.Refresh();
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if(!font) throw new InvalidOperationException("Import TMP essentials using TMP_PackageResourceImporter.ImportResources(true,false,false), then run Build again.");
            // Creates a dedicated scene; the user's template scene is retained on disk.
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            sirens.Clear(); UnityEngine.Random.InitState(50415);
            grass=Mat("Valley grass",new Color(.25f,.32f,.22f)); asphalt=Mat("Weathered asphalt",new Color(.16f,.19f,.2f)); concrete=Mat("Concrete",new Color(.48f,.49f,.45f));
            cream=Mat("Warm plaster",new Color(.75f,.69f,.52f)); roof=Mat("Slate roof",new Color(.13f,.2f,.23f)); wood=Mat("Cedar",new Color(.39f,.25f,.15f));
            glass=Mat("Windows",new Color(.24f,.43f,.47f),.7f); amber=Mat("Emergency amber",new Color(.96f,.52f,.12f)); white=Mat("Paint",new Color(.86f,.86f,.76f)); metal=Mat("Dark steel",new Color(.14f,.2f,.21f)); red=Mat("Red",new Color(.65f,.14f,.1f)); rock=Mat("Valley stone",new Color(.25f,.29f,.28f));
            atlas=Mat("Kenney atlas",Color.white); atlas.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/KenneySuburban/Textures/colormap.png");
            world=new GameObject("Cedar Valley • Environment").transform;
            Lighting(); TerrainAndRoads(); Neighborhood(); Home(); Market(); Ridge(); Dam(); TreesAndDetails();
            var nav=new GameObject("Town Navigation"); nav.transform.SetParent(world); var surface=world.gameObject.AddComponent<NavMeshSurface>(); surface.collectObjects=CollectObjects.Children; surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders; surface.layerMask=~(1<<2); surface.overrideTileSize=true; surface.tileSize=128; surface.BuildNavMesh();
            if(surface.navMeshData)
            {
                string navPath=Root+"/Settings/CedarValleyNavMesh.asset";
                if(AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath)) AssetDatabase.DeleteAsset(navPath);
                AssetDatabase.CreateAsset(surface.navMeshData,navPath);
            }
            var systems=new GameObject("Survival Systems"); var game=systems.AddComponent<GameManager>();
            var inventory=systems.AddComponent<InventorySystem>(); game.objectives=systems.AddComponent<ObjectiveSystem>();
            game.objectives.home=Point("Home objective",new Vector3(-22,.3f,-30)); game.objectives.shelter=Point("Ridge Shelter objective",new Vector3(88,15.1f,-46));
            Player(game,inventory); Water(game); Citizens(game); Audio(game); Supplies(); Interface(game);
            PlayerSettings.productName="FLOOD SURVIVAL: 5 DAYS"; PlayerSettings.companyName="Cedar Valley Studio"; PlayerSettings.defaultScreenWidth=1600; PlayerSettings.defaultScreenHeight=900; PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow; PlayerSettings.runInBackground=true;
            PlayerSettings.bundleVersion="0.1.0";
            EditorSceneManager.SaveScene(scene,Root+"/Scenes/CedarValley.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"/Scenes/CedarValley.unity",true)};
            AssetDatabase.SaveAssets();
            Selection.activeGameObject=game.player.gameObject;
            if(SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.LookAt(new Vector3(0,5,10),Quaternion.Euler(35,-30,0),100);
            Debug.Log("FLOOD SURVIVAL prototype scene built and saved. Press Play, then BEGIN EMERGENCY.");
        }
        static Material Mat(string name,Color color,float smoothness=.05f)
        {
            string path=Root+"/Materials/"+name+".mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat) { var shader=Shader.Find("Universal Render Pipeline/Lit"); if(!shader)throw new Exception("URP Lit shader missing"); mat=new Material(shader); AssetDatabase.CreateAsset(mat,path); }
            mat.color=color; mat.SetFloat("_Smoothness",smoothness); return mat;
        }
        static GameObject Box(string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool collider=true)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(parent?parent:world); go.transform.position=pos; go.transform.localScale=size; go.GetComponent<Renderer>().sharedMaterial=mat;
            go.isStatic=true; if(!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        static GameObject Shape(string name,PrimitiveType type,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool collider=false)
        {
            var go=GameObject.CreatePrimitive(type); go.name=name; go.transform.SetParent(parent?parent:world); go.transform.position=pos; go.transform.localScale=size; go.GetComponent<Renderer>().sharedMaterial=mat; go.isStatic=true; if(!collider)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        static Transform Point(string name,Vector3 pos) { var go=new GameObject(name); go.transform.position=pos; return go.transform; }
        static void Sign(string text,Vector3 pos,float yaw,float size=1.4f,Color? color=null,Transform parent=null)
        {
            var go=new GameObject("Sign • "+text); go.transform.SetParent(parent?parent:world); go.transform.position=pos; go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var t=go.AddComponent<TextMeshPro>(); t.font=font; t.text=text; t.fontSize=size; t.color=color??new Color(.92f,.91f,.8f); t.alignment=TextAlignmentOptions.Center; t.rectTransform.sizeDelta=new Vector2(14,3); t.enableAutoSizing=false;
        }
        static void Lighting()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight; RenderSettings.ambientSkyColor=new Color(.45f,.55f,.6f); RenderSettings.ambientEquatorColor=new Color(.39f,.43f,.42f); RenderSettings.ambientGroundColor=new Color(.23f,.27f,.25f);
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.ExponentialSquared; RenderSettings.fogColor=new Color(.33f,.43f,.47f); RenderSettings.fogDensity=.0037f;
            var sky=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Storm sky.mat"); if(!sky){sky=new Material(Shader.Find("Skybox/Procedural")); AssetDatabase.CreateAsset(sky,Root+"/Materials/Storm sky.mat");}
            sky.SetFloat("_SunSize",.025f); sky.SetFloat("_AtmosphereThickness",1.35f); sky.SetColor("_SkyTint",new Color(.41f,.49f,.55f)); sky.SetColor("_GroundColor",new Color(.22f,.27f,.25f)); sky.SetFloat("_Exposure",.85f); RenderSettings.skybox=sky;
            var sun=new GameObject("Late afternoon sun").AddComponent<Light>(); sun.type=LightType.Directional; sun.transform.rotation=Quaternion.Euler(31,-39,0); sun.color=new Color(1,.83f,.62f); sun.intensity=2; sun.shadows=LightShadows.Soft; RenderSettings.sun=sun;
            var fill=new GameObject("Overcast sky fill").AddComponent<Light>(); fill.type=LightType.Directional; fill.transform.rotation=Quaternion.Euler(65,140,0); fill.color=new Color(.62f,.77f,.9f); fill.intensity=.85f; fill.shadows=LightShadows.None;
            QualitySettings.shadowDistance=115; QualitySettings.vSyncCount=0;
            var urp=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset; if(!urp) throw new Exception("Project must use URP"); urp.supportsHDR=true;
            var volume=new GameObject("Storm atmosphere").AddComponent<Volume>(); volume.isGlobal=true;
            string path=Root+"/Settings/StormProfile.asset"; var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path); if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile,path);}
            foreach(var c in profile.components) if(c)UnityEngine.Object.DestroyImmediate(c,true); profile.components.Clear();
            var tone=profile.Add<Tonemapping>(true); tone.mode.value=TonemappingMode.ACES;
            var bloom=profile.Add<Bloom>(true); bloom.intensity.value=.25f; bloom.threshold.value=1;
            var vignette=profile.Add<Vignette>(true); vignette.intensity.value=.22f; vignette.smoothness.value=.45f;
            var grading=profile.Add<ColorAdjustments>(true); grading.saturation.value=-12; grading.contrast.value=8;
            foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile); volume.sharedProfile=profile;
        }
        static void TerrainAndRoads()
        {
            Box("Valley floor",new Vector3(0,-.7f,0),new Vector3(240,1.4f,250),grass);
            Box("Main Street",new Vector3(0,.025f,10),new Vector3(10,.05f,172),asphalt);
            Box("Cedar Avenue",new Vector3(0,.035f,-12),new Vector3(135,.06f,9),asphalt);
            Box("Ridge approach",new Vector3(24,.04f,-46),new Vector3(50,.07f,7),asphalt);
            for(int side=-1;side<=1;side+=2) Box("Main sidewalk",new Vector3(side*6.5f,.11f,10),new Vector3(2.8f,.22f,170),concrete);
            for(int side=-1;side<=1;side+=2)Box("Avenue sidewalk",new Vector3(0,.12f,-12+side*6.2f),new Vector3(132,.24f,2.6f),concrete);
            for(int z=-65;z<93;z+=10) if(Mathf.Abs(z+12)>8 && Mathf.Abs(z+46)>7) Box("Centerline",new Vector3(0,.065f,z),new Vector3(.14f,.015f,4.8f),white,null,false);
            for(int i=0;i<7;i++)Box("Crosswalk",new Vector3(-4+i*1.3f,.075f,-21),new Vector3(.75f,.02f,2.8f),white,null,false);
            Box("Town perimeter west",new Vector3(-117,8,0),new Vector3(2,16,248),rock);
            Box("Town perimeter east",new Vector3(117,8,0),new Vector3(2,16,248),rock);
            Box("Town perimeter south",new Vector3(0,8,-123),new Vector3(235,16,2),rock);
            for(int i=0;i<22;i++)
            {
                float angle=i*Mathf.PI*2/22; var pos=new Vector3(Mathf.Cos(angle)*143,7,Mathf.Sin(angle)*148);
                var hill=Shape("Mountain ridge",PrimitiveType.Sphere,pos,new Vector3(75,55+UnityEngine.Random.value*65,80),rock); hill.transform.rotation=UnityEngine.Random.rotation;
            }
        }
        static GameObject Model(string file,Vector3 position,float width,float yaw=0)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Art/KenneySuburban/"+file+".fbx"); if(!asset)throw new Exception("Model missing: "+file);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(asset); go.transform.SetParent(world); go.transform.position=Vector3.zero;
            Bounds bounds=new Bounds(); bool first=true; foreach(var r in go.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds); r.sharedMaterial=atlas;}
            float scale=width/Mathf.Max(bounds.size.x,.01f); go.transform.localScale=Vector3.one*scale; go.transform.rotation=Quaternion.Euler(0,yaw,0); go.transform.position=position+Vector3.up*(-bounds.min.y*scale);
            foreach(var child in go.GetComponentsInChildren<Transform>())child.gameObject.isStatic=true;
            return go;
        }
        static void Neighborhood()
        {
            var layouts=new[]{new Vector3(-25,0,20),new Vector3(-24,0,52),new Vector3(25,0,51),new Vector3(-51,0,-37),new Vector3(-51,0,17),new Vector3(49,0,16),new Vector3(47,0,51),new Vector3(-48,0,55),new Vector3(23,0,-70),new Vector3(-23,0,-70)};
            for(int i=0;i<layouts.Length;i++)
            {
                var p=layouts[i]; Box("Garden plot",p+new Vector3(0,.05f,0),new Vector3(20,.1f,23),grass);
                var model=Model("building-type-"+(char)('a'+i),p,12,i%2==0?0:180); model.name="Resident house "+(i+1);
                var bounds=new Bounds(model.transform.position,Vector3.zero); foreach(var renderer in model.GetComponentsInChildren<Renderer>())bounds.Encapsulate(renderer.bounds);
                var collider=model.AddComponent<BoxCollider>(); collider.center=model.transform.InverseTransformPoint(bounds.center); collider.size=new Vector3(bounds.size.x/model.transform.lossyScale.x,bounds.size.y/model.transform.lossyScale.y,bounds.size.z/model.transform.lossyScale.z);
                Box("Driveway",p+new Vector3(3,.045f,-10),new Vector3(4,.08f,8),concrete);
                for(int k=0;k<5;k++) Box("Garden fence",p+new Vector3(-9+k*4,1,-12),new Vector3(3.7f,1.2f,.12f),white);
            }
        }
        static void Home()
        {
            var p=new Vector3(-22,0,-30); var root=new GameObject("Your home • accessible interior").transform; root.SetParent(world);
            Box("Floor",p+new Vector3(0,.1f,0),new Vector3(13,.2f,11),wood,root);
            Box("Back wall",p+new Vector3(0,1.65f,5.5f),new Vector3(13,3.3f,.25f),cream,root);
            Box("Left wall",p+new Vector3(-6.5f,1.65f,0),new Vector3(.25f,3.3f,11),cream,root); Box("Right wall",p+new Vector3(6.5f,1.65f,0),new Vector3(.25f,3.3f,11),cream,root);
            Box("Front wall left",p+new Vector3(-4,1.65f,-5.5f),new Vector3(5,3.3f,.25f),cream,root); Box("Front wall right",p+new Vector3(4,1.65f,-5.5f),new Vector3(5,3.3f,.25f),cream,root); Box("Door lintel",p+new Vector3(0,2.9f,-5.5f),new Vector3(3, .8f,.25f),cream,root);
            Box("Roof",p+new Vector3(0,3.5f,0),new Vector3(14,.4f,12),roof,root); Box("Porch",p+new Vector3(0,.1f,-6.7f),new Vector3(6,.2f,2.5f),concrete,root);
            for(int s=-1;s<=1;s+=2){ Box("Front window frame",p+new Vector3(s*4,1.9f,-5.67f),new Vector3(2.6f,1.6f,.08f),white,root); Box("Front window",p+new Vector3(s*4,1.9f,-5.73f),new Vector3(2.3f,1.3f,.08f),glass,root); }
            Box("Home sign",p+new Vector3(0,3.12f,-5.8f),new Vector3(3,.5f,.08f),roof,root); Sign("YOUR HOME",p+new Vector3(0,3.13f,-5.86f),0,.75f,null,root);
            Box("Kitchen counter",p+new Vector3(3, .55f,3.7f),new Vector3(5,1.1f,1.4f),white,root);
            Box("Dining table",p+new Vector3(-3,.7f,1.6f),new Vector3(3,.16f,2),wood,root);
            for(int i=0;i<4;i++)Box("Table leg",p+new Vector3(-3+(i%2==0?-1.2f:1.2f),.35f,1.6f+(i<2?-.7f:.7f)),new Vector3(.15f,.7f,.15f),wood,root);
            Box("Sofa",p+new Vector3(-4,.45f,-2.3f),new Vector3(3.3f,.8f,1.2f),roof,root); Box("Sofa back",p+new Vector3(-4,.9f,-1.8f),new Vector3(3.3f,.8f,.25f),roof,root);
            Box("Emergency notice",p+new Vector3(0,1.8f,5.32f),new Vector3(4.2f,1.5f,.04f),white,root);
            Sign("DAM SAFETY NOTICE\nRIDGE TRAIL →\nKeep an emergency bag ready",p+new Vector3(0,1.8f,5.27f),0,.45f,Color.black,root);
            InteriorLight(p+new Vector3(0,2.9f,0));
        }
        static void Market()
        {
            var p=new Vector3(22,0,17); var root=new GameObject("Cedar Market • accessible interior").transform; root.SetParent(world);
            Box("Market floor",p+new Vector3(0,.11f,0),new Vector3(18,.22f,15),concrete,root);
            Box("Rear",p+new Vector3(0,2.1f,7.5f),new Vector3(18,4.2f,.3f),cream,root); Box("Left",p+new Vector3(-9,2.1f,0),new Vector3(.3f,4.2f,15),cream,root); Box("Right",p+new Vector3(9,2.1f,0),new Vector3(.3f,4.2f,15),cream,root);
            Box("Front left",p+new Vector3(-6,2.1f,-7.5f),new Vector3(6,4.2f,.3f),glass,root); Box("Front right",p+new Vector3(6,2.1f,-7.5f),new Vector3(6,4.2f,.3f),glass,root);
            Box("Shop roof",p+new Vector3(0,4.4f,0),new Vector3(19,.4f,16),roof,root); Box("Market sign",p+new Vector3(0,3.55f,-7.75f),new Vector3(18,1.3f,.3f),roof,root); Sign("CEDAR MARKET",p+new Vector3(0,3.55f,-7.94f),0,1.8f,null,root);
            Box("Awning",p+new Vector3(0,2.9f,-8.5f),new Vector3(19,.14f,2.3f),amber,root);
            for(int row=0;row<2;row++)for(int side=-1;side<=1;side+=2)
            {
                var q=p+new Vector3(side*5,0,-2+row*6); Box("Shelf",q+Vector3.up*.5f,new Vector3(2,1,3.6f),wood,root);
                Box("Shelf top",q+Vector3.up*1.05f,new Vector3(2.2f,.12f,3.8f),white,root);
                for(int k=0;k<4;k++) Box("Remaining stock",q+new Vector3(.4f,1.35f,-1.2f+k*.7f),new Vector3(.5f,.5f,.4f),row==0?amber:red,root);
            }
            Box("Checkout",p+new Vector3(-5,.6f,-5.6f),new Vector3(4,1.2f,1.4f),roof,root); Box("Register",p+new Vector3(-5,1.35f,-5.6f),new Vector3(.8f,.5f,.55f),metal,root);
            Sign("ESSENTIALS ONLY\nLeave some for your neighbors",p+new Vector3(0,2.2f,7.29f),0,.7f,Color.black,root);
            InteriorLight(p+new Vector3(0,3.7f,0));
        }
        static void InteriorLight(Vector3 pos) { var light=new GameObject("Warm ceiling light").AddComponent<Light>(); light.transform.SetParent(world); light.transform.position=pos; light.type=LightType.Point; light.color=new Color(1,.8f,.55f); light.intensity=3; light.range=13; }
        static void Ridge()
        {
            var ramp=Box("Ridge trail • walkable slope",new Vector3(60,7.45f,-46),new Vector3(50.3f,.6f,7),concrete); ramp.transform.rotation=Quaternion.Euler(0,0,17.35f);
            Box("Evacuation plateau",new Vector3(95,7.5f,-46),new Vector3(22,15,25),rock);
            Box("Shelter terrace",new Vector3(95,15.04f,-46),new Vector3(23,.1f,26),concrete);
            Box("Shelter canopy",new Vector3(88,18.4f,-46),new Vector3(12,.3f,11),roof);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box("Canopy pillar",new Vector3(88+x*5.5f,16.7f,-46+z*4.7f),new Vector3(.3f,3.4f,.3f),wood);
            Box("Shelter back wall",new Vector3(93.5f,16.6f,-46),new Vector3(.3f,3.2f,11),wood);
            Sign("RIDGE SHELTER\nEVACUATION POINT  /  15 m",new Vector3(88,18,-51.7f),0,1.05f);
            for(int i=0;i<9;i++)
            {
                float x=39+i*5; float y=Mathf.Max(0,(x-36)*.3125f);
                Box("Amber trail post",new Vector3(x,y+.9f,-49.8f),new Vector3(.18f,1.8f,.18f),amber);
                Box("Trail reflector",new Vector3(x,y+1.7f,-49.8f),new Vector3(.35f,.2f,.35f),white,null,false);
            }
            Box("Trail sign",new Vector3(32,1.9f,-50),new Vector3(5,1.8f,.15f),roof); Box("Signpost",new Vector3(32,.9f,-50),new Vector3(.2f,2,.2f),metal);
            Sign("HIGH GROUND →\nRIDGE SHELTER",new Vector3(32,1.9f,-50.1f),0,.7f,new Color(1,.7f,.28f));
            for(int i=0;i<3;i++)Box("Emergency supply crate",new Vector3(90+i,15.5f,-43),new Vector3(.8f,1,.8f),amber);
        }
        static void Dam()
        {
            Box("Dam west section",new Vector3(-27,16,108),new Vector3(36,34,8),concrete); Box("Dam east section",new Vector3(27,16,108),new Vector3(36,34,8),concrete);
            Box("Dam center • breach",new Vector3(0,16,108),new Vector3(18,34,8),concrete);
            for(int i=-4;i<=4;i++)Box("Dam buttress",new Vector3(i*10,12,102),new Vector3(2,26,6),rock);
            Box("Dam service bridge",new Vector3(0,34,108),new Vector3(98,1,10),roof);
            Sign("CEDAR DAM",new Vector3(0,29,103.7f),0,5);
            Box("Downstream channel",new Vector3(0,-.02f,92),new Vector3(24,.1f,25),rock);
        }
        static void TreesAndDetails()
        {
            for(int i=0;i<58;i++)
            {
                float x=UnityEngine.Random.Range(-100f,100f),z=UnityEngine.Random.Range(-105f,96f);
                if(Mathf.Abs(x)<12 || Mathf.Abs(z+12)<10 || (z<-35 && z>-56 && x>25))continue;
                if(Mathf.Abs(x+22)<12 && Mathf.Abs(z+30)<12 || Mathf.Abs(x-22)<14 && Mathf.Abs(z-17)<14)continue;
                Model(i%2==0?"tree-large":"tree-small",new Vector3(x,0,z),UnityEngine.Random.Range(3.5f,6),UnityEngine.Random.Range(0,360));
            }
            for(int z=-62;z<=80;z+=24)
            {
                var p=new Vector3(8.3f,0,z); Box("Streetlight mast",p+Vector3.up*3,new Vector3(.15f,6,.15f),metal); Box("Streetlight arm",p+new Vector3(-.7f,5.9f,0),new Vector3(1.5f,.13f,.13f),metal);
                var lens=Box("Streetlight lens",p+new Vector3(-1.4f,5.85f,0),new Vector3(.8f,.16f,.4f),amber,null,false);
                if(z%3==0){var light=lens.AddComponent<Light>(); light.type=LightType.Point; light.range=9; light.intensity=1; light.color=new Color(1,.7f,.3f);}
                var source=lens.AddComponent<AudioSource>(); source.spatialBlend=1; source.minDistance=15; source.maxDistance=120; source.rolloffMode=AudioRolloffMode.Linear; source.playOnAwake=false; sirens.Add(source);
            }
            Sign("CEDAR VALLEY\nPopulation 428  /  Elevation 0 m",new Vector3(-11,2,-62),0,.75f);
            Box("Town welcome board",new Vector3(-11,2,-61.8f),new Vector3(8,2.8f,.15f),roof);
            for(int i=0;i<6;i++)Box("Abandoned parcel",new Vector3(9+i*.8f,.25f,-20),new Vector3(.6f,.5f,.5f),wood);
        }
        static void Player(GameManager game,InventorySystem inventory)
        {
            var go=new GameObject("Player"); go.layer=2; go.transform.position=new Vector3(0,.25f,-53);
            var controller=go.AddComponent<CharacterController>(); controller.height=1.8f; controller.radius=.32f; controller.center=new Vector3(0,.9f,0); controller.stepOffset=.32f; controller.slopeLimit=48;
            var player=go.AddComponent<FirstPersonController>(); player.inventory=inventory;
            var camera=new GameObject("First Person Camera").AddComponent<Camera>(); camera.tag="MainCamera"; camera.transform.SetParent(go.transform,false); camera.transform.localPosition=new Vector3(0,1.64f,0); camera.fieldOfView=76; camera.nearClipPlane=.08f; camera.farClipPlane=440; camera.allowHDR=true; camera.gameObject.AddComponent<AudioListener>();
            var data=camera.GetUniversalAdditionalCameraData(); data.renderPostProcessing=true; data.volumeLayerMask=1;
            var lamp=new GameObject("Equipped flashlight").AddComponent<Light>(); lamp.transform.SetParent(camera.transform,false); lamp.transform.localPosition=new Vector3(.2f,-.15f,.2f); lamp.type=LightType.Spot; lamp.range=32; lamp.spotAngle=48; lamp.innerSpotAngle=26; lamp.intensity=12; lamp.color=new Color(1,.9f,.7f); lamp.enabled=false;
            player.view=camera; player.flashlight=lamp; game.player=player;
        }
        static void Water(GameManager game)
        {
            var flood=game.gameObject.AddComponent<FloodManager>(); game.flood=flood;
            var water=GameObject.CreatePrimitive(PrimitiveType.Plane); water.name="Flood surface • single draw"; water.layer=2; UnityEngine.Object.DestroyImmediate(water.GetComponent<Collider>()); water.transform.position=new Vector3(0,-2,0); water.transform.localScale=Vector3.one*28;
            var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Flood water.mat"); if(!mat){var shader=Shader.Find("FloodSurvival/Water"); if(!shader)throw new Exception("Water shader missing"); mat=new Material(shader); AssetDatabase.CreateAsset(mat,Root+"/Materials/Flood water.mat");} water.GetComponent<Renderer>().sharedMaterial=mat; water.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            flood.water=water.transform; flood.damCenter=GameObject.Find("Dam center • breach"); water.SetActive(false);
            var spray=new GameObject("Breach spray").AddComponent<ParticleSystem>(); spray.transform.position=new Vector3(0,12,101); var main=spray.main; main.playOnAwake=false; main.duration=8; main.startLifetime=3; main.startSpeed=12; main.startSize=3; main.startColor=new Color(.45f,.65f,.65f,.7f); main.maxParticles=180; main.gravityModifier=.7f; var emission=spray.emission; emission.rateOverTime=30; var shape=spray.shape; shape.shapeType=ParticleSystemShapeType.Cone; shape.angle=30; shape.radius=8; spray.transform.rotation=Quaternion.Euler(40,180,0);
            var particleMat=Mat("Spray",new Color(.48f,.65f,.65f)); spray.GetComponent<ParticleSystemRenderer>().sharedMaterial=particleMat; spray.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear); flood.breachSpray=spray;
        }
        static void Citizens(GameManager game)
        {
            var list=new List<Evacuee>(); var root=new GameObject("Town residents").transform;
            var starts=new[]{new Vector3(-5,0,-8),new Vector3(9,0,2),new Vector3(-8,0,30),new Vector3(3,0,60),new Vector3(-38,0,-17),new Vector3(22,0,-17),new Vector3(6,0,-39),new Vector3(-7,0,-55)};
            for(int i=0;i<starts.Length;i++)
            {
                var go=new GameObject("Evacuee "+(i+1)); go.layer=2; go.transform.SetParent(root); if(NavMesh.SamplePosition(starts[i],out var hit,5,NavMesh.AllAreas))go.transform.position=hit.position;else go.transform.position=starts[i];
                var agent=go.AddComponent<NavMeshAgent>(); agent.enabled=false; agent.radius=.3f; agent.height=1.8f; agent.speed=1.2f; agent.stoppingDistance=1; agent.obstacleAvoidanceType=ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                var npc=go.AddComponent<Evacuee>(); npc.destination=new Vector3(85+(i%3)*2,15.1f,-49+(i%4)*2);
                var shirt=Mat("Resident clothing "+i,Color.HSVToRGB(i*.117f,.4f,.55f));
                npc.body=Limb(go.transform,"Torso",new Vector3(0,1.05f,0),new Vector3(.48f,.58f,.28f),shirt);
                Shape("Head",PrimitiveType.Sphere,go.transform.position+Vector3.up*1.57f,new Vector3(.31f,.36f,.31f),cream,go.transform);
                Shape("Neck",PrimitiveType.Cylinder,go.transform.position+Vector3.up*1.36f,new Vector3(.12f,.09f,.12f),cream,go.transform);
                npc.leftLeg=Limb(go.transform,"Left leg",new Vector3(-.14f,.76f,0),new Vector3(.18f,.72f,.21f),roof,true); npc.rightLeg=Limb(go.transform,"Right leg",new Vector3(.14f,.76f,0),new Vector3(.18f,.72f,.21f),roof,true);
                npc.leftArm=Limb(go.transform,"Left arm",new Vector3(-.33f,1.3f,0),new Vector3(.15f,.6f,.18f),shirt,true); npc.rightArm=Limb(go.transform,"Right arm",new Vector3(.33f,1.3f,0),new Vector3(.15f,.6f,.18f),shirt,true);
                foreach(var t in go.GetComponentsInChildren<Transform>()){t.gameObject.layer=2; t.gameObject.isStatic=false;} list.Add(npc);
            }
            game.npcs=game.gameObject.AddComponent<NPCManager>(); game.npcs.citizens=list.ToArray();
            PrefabUtility.SaveAsPrefabAsset(list[0].gameObject,Root+"/Prefabs/Evacuee.prefab");
        }
        static Transform Limb(Transform parent,string name,Vector3 local,Vector3 size,Material mat,bool pivot=false)
        {
            var root=new GameObject(name).transform; root.SetParent(parent,false); root.localPosition=local;
            var mesh=Box(name+" mesh",root.position+(pivot?Vector3.down*size.y*.5f:Vector3.zero),size,mat,root,false); mesh.isStatic=false; return root;
        }
        static void Audio(GameManager game)
        {
            var audio=game.gameObject.AddComponent<AtmosphereAudio>(); audio.sirens=sirens.ToArray(); audio.ambience=game.gameObject.AddComponent<AudioSource>(); audio.ambience.playOnAwake=false; audio.effects=game.gameObject.AddComponent<AudioSource>(); audio.effects.playOnAwake=false; game.audioSystem=audio;
        }
        static void Supplies()
        {
            var root=new GameObject("Collectible supplies").transform;
            string[] names={"Food rations","Drinking water","Medicine","Flashlight"}; Color[] colors={new Color(.95f,.58f,.16f),new Color(.2f,.66f,.8f),new Color(.85f,.25f,.2f),new Color(.9f,.8f,.35f)};
            var definitions=new ItemDefinition[4];
            for(int i=0;i<4;i++)
            {
                string path=Root+"/Items/"+((ItemKind)i)+".asset"; var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(path); if(!item){item=ScriptableObject.CreateInstance<ItemDefinition>(); AssetDatabase.CreateAsset(item,path);} item.kind=(ItemKind)i; item.displayName=names[i]; item.color=colors[i]; item.description=i==2?"Restores 40 health. Use H.":i==3?"Press F to equip or toggle.":"An essential emergency supply."; definitions[i]=item;
                EditorUtility.SetDirty(item);
                var p=Pickup(item,Vector3.zero,root); PrefabUtility.SaveAsPrefabAsset(p,Root+"/Prefabs/"+((ItemKind)i)+"Pickup.prefab"); UnityEngine.Object.DestroyImmediate(p);
            }
            Vector3[] locations={new Vector3(17,1.35f,15),new Vector3(27,1.35f,15),new Vector3(17,1.35f,21),new Vector3(-25,.98f,-28.5f),new Vector3(-21,1.4f,-26.3f),new Vector3(-19,1.4f,-26.3f),new Vector3(27,1.35f,21),new Vector3(-24,.98f,-28)};
            int[] kinds={0,1,2,3,0,1,0,2};
            for(int i=0;i<locations.Length;i++){var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+((ItemKind)kinds[i])+"Pickup.prefab"); var p=(GameObject)PrefabUtility.InstantiatePrefab(asset); p.transform.SetParent(root); p.transform.position=locations[i];}
        }
        static GameObject Pickup(ItemDefinition item,Vector3 pos,Transform parent)
        {
            var root=new GameObject(item.displayName); root.transform.SetParent(parent); root.transform.position=pos; root.layer=0; var pickup=root.AddComponent<PickupItem>(); pickup.definition=item;
            var mat=Mat("Supply "+item.kind,item.color);
            var body=Box("Supply",pos,new Vector3(.38f,.45f,.25f),mat,root.transform); body.isStatic=false; body.layer=0;
            if(item.kind==ItemKind.Water){body.transform.localScale=new Vector3(.23f,.5f,.23f); Shape("Bottle cap",PrimitiveType.Cylinder,pos+Vector3.up*.28f,new Vector3(.12f,.06f,.12f),white,root.transform);}
            if(item.kind==ItemKind.Medicine){Box("Medical cross H",pos+new Vector3(0,0,-.132f),new Vector3(.25f,.07f,.01f),white,root.transform,false);Box("Medical cross V",pos+new Vector3(0,0,-.14f),new Vector3(.07f,.25f,.01f),white,root.transform,false);}
            if(item.kind==ItemKind.Flashlight){body.transform.localScale=new Vector3(.16f,.16f,.48f); Shape("Lens",PrimitiveType.Sphere,pos+new Vector3(0,0,-.24f),new Vector3(.2f,.2f,.08f),white,root.transform);}
            foreach(var t in root.GetComponentsInChildren<Transform>()){t.gameObject.layer=0;t.gameObject.isStatic=false;} return root;
        }
        static void Interface(GameManager game)
        {
            var canvas=new GameObject("Survival HUD",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster)); canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
            var ui=canvas.AddComponent<SurvivalUI>(); ui.Build(font); game.ui=ui;
            var events=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(InputSystemUIInputModule)); events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
}
