using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
namespace FloodSurvival.Editor
{
    public static class PrototypeUpgradeV02
    {
        const string Root="Assets/FloodSurvival", ScenePath=Root+"/Scenes/CedarValley.unity";
        static Transform world,content;
        static TMP_FontAsset font;
        static GameManager game;
        static Material wood,cream,slate,white,amber,metal,glass,cloth,particle;
        static readonly Dictionary<string,Material> converted=new Dictionary<string,Material>();
        [MenuItem("Flood Survival/Upgrade existing scene to v0.2")]
        public static void Upgrade()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode before upgrading");
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=ScenePath)EditorSceneManager.OpenScene(ScenePath);
            if(GameObject.Find("V02 Upgrade Completed")){Debug.Log("This scene is already upgraded. Edit it in place; the upgrader does not overwrite it.");return;}
            if(!File.Exists("Archive/v0.1Source/README.md"))throw new InvalidOperationException("Keep a v0.1 backup before upgrading");
            foreach(string folder in new[]{"Icons","Animations","Settings","Materials","Prefabs"})Directory.CreateDirectory(Root+"/"+folder);
            AssetDatabase.Refresh();converted.Clear();
            world=GameObject.Find("Cedar Valley • Environment").transform;
            game=UnityEngine.Object.FindFirstObjectByType<GameManager>();font=game.ui.font;
            content=new GameObject("V02 New Content").transform;content.SetParent(world,false);
            wood=Mat("V02 Walnut",new Color(.34f,.19f,.11f));cream=Mat("V02 Ivory",new Color(.81f,.77f,.65f));slate=Mat("V02 Navy",new Color(.055f,.13f,.17f));white=Mat("V02 White",new Color(.93f,.92f,.82f));amber=Mat("V02 Amber",new Color(1,.58f,.13f));metal=Mat("V02 Steel",new Color(.1f,.15f,.17f),.5f);cloth=Mat("V02 Upholstery",new Color(.25f,.43f,.41f));glass=Mat("V02 Window",new Color(.55f,.76f,.83f,.1f),.85f,true);
            particle=ParticleMaterial();
            var story=game.gameObject.AddComponent<OpeningSequence>();game.story=story;game.preparationDuration=150;
            UpgradeHouse(story);UpgradeTown();Items();Television(story);UpgradeDam(story);UpgradeNPCs();Ambient(story);Polish();
            game.ui.UpgradeV02(Definitions());EditorUtility.SetDirty(game.ui);
            game.objectives.supplies=Point("House supplies waypoint",new Vector3(-24,1.2f,-25.8f));
            game.player.transform.SetPositionAndRotation(story.seat.position,story.seat.rotation);game.player.view.transform.localPosition=Vector3.up*1.15f;
            var surface=world.GetComponent<NavMeshSurface>();surface.BuildNavMesh();
            string navPath=Root+"/Settings/CedarValleyV02NavMesh.asset";if(AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath))AssetDatabase.DeleteAsset(navPath);AssetDatabase.CreateAsset(surface.navMeshData,navPath);
            var floaters=content.GetComponentsInChildren<FloatingObject>(true);game.flood.floaters=floaters;
            PlayerSettings.bundleVersion="0.2.0";
            var marker=new GameObject("V02 Upgrade Completed");marker.transform.SetParent(content);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            Debug.Log("v0.2 integrated into the existing CedarValley scene. v0.1 backup retained. Press Play > START STORY.");
        }
        static Material Mat(string name,Color color,float smooth=.15f,bool transparent=false)
        {
            string path=Root+"/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}mat.color=color;mat.SetFloat("_Smoothness",smooth);
            if(transparent){mat.SetFloat("_Surface",1);mat.SetFloat("_SrcBlend",(int)BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);mat.SetFloat("_ZWrite",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.renderQueue=3000;}
            EditorUtility.SetDirty(mat);return mat;
        }
        static GameObject Box(string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool collision=true)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent?parent:content);go.transform.position=pos;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;go.isStatic=true;if(!collision)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static Transform Point(string name,Vector3 pos) {var t=new GameObject(name).transform;t.SetParent(content);t.position=pos;return t;}
        static void Hide(Transform parent,params string[] names) {foreach(string name in names){var t=parent.Find(name);if(t)t.gameObject.SetActive(false);}}
        static Bounds BoundsOf(GameObject go)
        {
            var renderers=go.GetComponentsInChildren<Renderer>();var bounds=new Bounds(go.transform.position,Vector3.zero);bool first=true;
            foreach(var renderer in renderers){if(first){bounds=renderer.bounds;first=false;}else bounds.Encapsulate(renderer.bounds);}return bounds;
        }
        static GameObject Model(string pack,string file,Vector3 pos,float width,float yaw=0,bool collision=true,Transform parent=null)
        {
            string path=Root+"/Art/"+pack+"/"+file+".fbx";var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!source)throw new Exception("Missing model "+path);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(source);go.transform.SetParent(parent?parent:content);go.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);var original=BoundsOf(go);
            float scale=width/Mathf.Max(.01f,original.size.x);go.transform.localScale=Vector3.one*scale;go.transform.rotation=Quaternion.Euler(0,yaw,0);var bounds=BoundsOf(go);go.transform.position=pos-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            foreach(var renderer in go.GetComponentsInChildren<Renderer>())
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    var old=materials[i];if(!old)continue;string key=pack+"-"+old.name;
                    if(!converted.TryGetValue(key,out var mat))
                    {
                        Color color=old.HasProperty("_Color")?old.GetColor("_Color"):Color.gray;
                        mat=Mat("Imported "+key,color);
                        if(old.mainTexture)mat.mainTexture=old.mainTexture;
                        if(pack=="car-kit"){var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/car-kit/Textures/colormap.png");if(atlas){mat.mainTexture=atlas;mat.color=Color.white;}}
                        EditorUtility.SetDirty(mat);converted[key]=mat;
                    }
                    materials[i]=mat;
                }
                renderer.sharedMaterials=materials;
            }
            foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.isStatic=true;
            if(collision)
            {
                bounds=BoundsOf(go);var collider=go.AddComponent<BoxCollider>();collider.center=go.transform.InverseTransformPoint(bounds.center);
                // Bounds must be expressed in the object's rotated local coordinates.
                var local=new Bounds(Vector3.zero,Vector3.zero);bool first=true;
                foreach(var filter in go.GetComponentsInChildren<MeshFilter>())if(filter.sharedMesh)
                {
                    var b=filter.sharedMesh.bounds;var matrix=go.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                    for(int c=0;c<8;c++){var v=matrix.MultiplyPoint3x4(b.center+Vector3.Scale(b.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1)));if(first){local=new Bounds(v,Vector3.zero);first=false;}else local.Encapsulate(v);}
                }
                collider.center=local.center;collider.size=local.size;
            }
            return go;
        }
        static void Text(string caption,Vector3 pos,float yaw,float size,Color color,Transform parent=null)
        {
            var go=new GameObject("Notice "+caption);go.transform.SetParent(parent?parent:content);go.transform.SetPositionAndRotation(pos,Quaternion.Euler(0,yaw,0));var t=go.AddComponent<TextMeshPro>();t.font=font;t.text=caption;t.fontSize=size;t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=new Vector2(8,2.5f);
        }
        static void UpgradeHouse(OpeningSequence story)
        {
            var home=GameObject.Find("Your home • accessible interior").transform;
            Hide(home,"Sofa","Sofa back","Dining table","Kitchen counter","Emergency notice");
            foreach(var t in home.GetComponentsInChildren<Transform>(true))if(t.name=="Table leg" || t.name.StartsWith("Sign • DAM SAFETY"))t.gameObject.SetActive(false);
            Model("furniture-kit","loungeSofa",new Vector3(-26,.2f,-32.3f),2.8f,0,true);
            Model("furniture-kit","tableCoffee",new Vector3(-26,.2f,-33.9f),2.1f,0,true);
            Model("furniture-kit","rugRectangle",new Vector3(-26,.215f,-33.4f),4.1f,0,false);
            Model("furniture-kit","pillowBlue",new Vector3(-27,.9f,-32.2f),.5f,0,false);
            Model("furniture-kit","cabinetTelevision",new Vector3(-26,.2f,-35.0f),3,0,true);
            Model("furniture-kit","televisionModern",new Vector3(-26,.82f,-35.1f),2.65f,180,false);
            Model("furniture-kit","lampRoundFloor",new Vector3(-28,.2f,-31.5f),.65f,0,true);
            Model("furniture-kit","bookcaseOpen",new Vector3(-28,.2f,-27.5f),1.2f,90,true);
            Model("furniture-kit","books",new Vector3(-26.4f,.85f,-33.9f),.5f,0,false);
            Model("furniture-kit","plantSmall1",new Vector3(-25.3f,.85f,-33.9f),.34f,0,false);
            Model("furniture-kit","pottedPlant",new Vector3(-28,.2f,-34.8f),.6f,0,true);
            for(int i=0;i<4;i++)Model("furniture-kit",i==1?"kitchenSink":i==2?"kitchenStove":"kitchenCabinet",new Vector3(-26.7f+i*1.2f,.2f,-25.4f),1.15f,0,true);
            Model("furniture-kit","kitchenFridge",new Vector3(-21.6f,.2f,-25.1f),1.05f,0,true);
            Model("furniture-kit","kitchenCoffeeMachine",new Vector3(-26.6f,1.16f,-25.4f),.4f,0,false);
            Model("furniture-kit","kitchenCabinetUpperDouble",new Vector3(-24.9f,2.1f,-24.85f),2.3f,0,true);
            Model("furniture-kit","tableRound",new Vector3(-23.6f,.2f,-28.3f),1.7f,0,true);
            Model("furniture-kit","chairCushion",new Vector3(-23.5f,.2f,-29.7f),.6f,180,true);
            Box("Bedroom partition south",new Vector3(-20.2f,1.65f,-33.3f),new Vector3(.16f,3.1f,4.1f),cream);
            Box("Bedroom partition north",new Vector3(-20.2f,1.65f,-26.3f),new Vector3(.16f,3.1f,3.3f),cream);
            Box("Bedroom door lintel",new Vector3(-20.2f,2.85f,-29.5f),new Vector3(.16f,.7f,3.1f),cream);
            Model("furniture-kit","bedDouble",new Vector3(-17.8f,.2f,-31.7f),2,0,true);
            Model("furniture-kit","cabinetBed",new Vector3(-16.35f,.2f,-32.2f),.65f,0,true);
            Model("furniture-kit","lampRoundTable",new Vector3(-16.35f,.83f,-32.2f),.36f,0,false);
            Model("furniture-kit","bookcaseClosedDoors",new Vector3(-17.3f,.2f,-24.9f),1.7f,0,true);
            Model("furniture-kit","sideTable",new Vector3(-19,.2f,-34.6f),1,0,true);
            Model("furniture-kit","radio",new Vector3(-19,.94f,-34.6f),.55f,0,false);
            Model("furniture-kit","rugDoormat",new Vector3(-22,.22f,-36.2f),1.4f,0,false);
            Model("furniture-kit","coatRackStanding",new Vector3(-21.1f,.2f,-34.6f),.6f,0,true);
            Window(home,-26,-35.5f,"Front wall left");Window(home,-18,-35.5f,"Front wall right");
            foreach(var t in home.GetComponentsInChildren<Transform>(true))if(t.name=="Front window" || t.name=="Front window frame")t.gameObject.SetActive(false);
            Box("Family photograph",new Vector3(-28.34f,2.1f,-29.8f),new Vector3(.04f,.85f,1.2f),wood);
            Text("CEDAR LAKE\nSUMMER 1998",new Vector3(-28.3f,2.1f,-29.8f),90,.28f,Color.white);
            var photo=Box("Family album",new Vector3(-25.8f,.9f,-33.8f),new Vector3(.36f,.03f,.25f),slate);var info=photo.AddComponent<InspectableObject>();info.title="FAMILY ALBUM";info.description="A summer picnic beside Cedar Lake, before the dam was built. A handwritten note says: 'The ridge is the way out.'";
            Text("KITCHEN",new Vector3(-24,2.85f,-24.65f),0,.35f,new Color(.22f,.27f,.26f));
            Text("BEDROOM",new Vector3(-20.32f,2.6f,-29.5f),90,.35f,Color.white);
            story.seat=Point("Couch seated position",new Vector3(-26,.22f,-33.05f));story.seat.rotation=Quaternion.Euler(0,180,0);
            story.standing=Point("Couch stand position",new Vector3(-22,.25f,-33.2f));
            var lamp=new GameObject("TV warm room fill").AddComponent<Light>();lamp.transform.SetParent(content);lamp.transform.position=new Vector3(-25,2.9f,-30.5f);lamp.type=LightType.Point;lamp.color=new Color(1,.81f,.58f);lamp.intensity=2.5f;lamp.range=8;
        }
        static void Window(Transform home,float x,float z,string oldWall)
        {
            Hide(home,oldWall);
            Box("Window sill wall",new Vector3(x,.6f,z),new Vector3(5,1.2f,.22f),cream);
            Box("Window header wall",new Vector3(x,2.95f,z),new Vector3(5,.7f,.22f),cream);
            for(int side=-1;side<=1;side+=2){Box("Window pillar",new Vector3(x+side*2,1.9f,z),new Vector3(1,1.4f,.22f),cream);Box("Curtain",new Vector3(x+side*1.35f,1.9f,z+.18f),new Vector3(.4f,1.7f,.08f),cloth);}
            Box("Clear window",new Vector3(x,1.9f,z),new Vector3(3,1.4f,.05f),glass);
            Box("Window crossbar",new Vector3(x,1.9f,z+.06f),new Vector3(.05f,1.4f,.05f),white);Box("Window crossbar",new Vector3(x,1.9f,z+.06f),new Vector3(3,.05f,.05f),white);
        }
        static void UpgradeTown()
        {
            var cars=new[]{new Vector3(8.8f,.2f,-30),new Vector3(-8.8f,.2f,20),new Vector3(20,.2f,3),new Vector3(-46,.2f,-56),new Vector3(38,.2f,-7)};
            string[] names={"sedan","hatchback-sports","van","suv","ambulance"};
            for(int i=0;i<cars.Length;i++)Model("car-kit",names[i],cars[i],1.9f,i%2==0?0:180,true);
            var p=new Vector3(-51,0,-75);
            Box("Moreno garage floor",p+new Vector3(0,.1f,0),new Vector3(11,.2f,12),cream);
            Box("Moreno garage rear",p+new Vector3(0,1.7f,6),new Vector3(11,3.4f,.2f),cream);Box("Moreno garage left",p+new Vector3(-5.5f,1.7f,0),new Vector3(.2f,3.4f,12),cream);Box("Moreno garage right",p+new Vector3(5.5f,1.7f,0),new Vector3(.2f,3.4f,12),cream);
            Box("Moreno garage roof",p+new Vector3(0,3.5f,0),new Vector3(12,.25f,13),slate);
            Model("furniture-kit","table",p+new Vector3(-3,.2f,3),2.3f,0,true);Model("furniture-kit","washer",p+new Vector3(3,.2f,4),1.1f,0,true);
            Model("furniture-kit","dryer",p+new Vector3(3,.2f,2.5f),1.1f,0,true);Model("furniture-kit","cardboardBoxOpen",p+new Vector3(-4,.2f,-2),1,0,true);Model("furniture-kit","loungeChair",p+new Vector3(0,.2f,2),1,180,true);
            Text("MORENO GARAGE\nTOOLS / LAUNDRY",p+new Vector3(0,2.9f,-6.2f),0,.65f,Color.white);
            Box("Garage footpath",new Vector3(-37,.03f,-76),new Vector3(20,.05f,2.5f),cream);
            for(int i=0;i<8;i++)
            {
                var q=new Vector3(-40+i*11,.5f,-19.7f);var prop=Model("furniture-kit",i%2==0?"cardboardBoxClosed":"cardboardBoxOpen",q,.7f,15*i,false);var floater=prop.AddComponent<FloatingObject>();floater.draft=.1f;foreach(var t in prop.GetComponentsInChildren<Transform>())t.gameObject.isStatic=false;
            }
            Text("EVACUATION ROUTE\nRIDGE SHELTER  >",new Vector3(10,2.2f,-43),-90,.55f,new Color(1,.7f,.2f));
            Box("Route sign backing",new Vector3(10.1f,2.2f,-43),new Vector3(.12f,1.5f,4),slate);
            var beacon=new GameObject("Ridge beacon").AddComponent<Light>();beacon.transform.SetParent(content);beacon.transform.position=new Vector3(88,19,-46);beacon.type=LightType.Point;beacon.range=15;beacon.intensity=5;beacon.color=new Color(1,.6f,.18f);
            Text("SAFE ABOVE THE FLOOD\nRIDGE SHELTER  /  15 m",new Vector3(84.1f,17.5f,-46),90,.75f,new Color(1,.72f,.25f));
            for(int i=0;i<30;i++)
            {
                float x=(i%2==0?-1:1)*(65+i%7*5);float z=-93+(i*19)%170;
                var stone=GameObject.CreatePrimitive(PrimitiveType.Sphere);stone.name="Valley boulder";stone.transform.SetParent(content);stone.transform.position=new Vector3(x,.5f,z);stone.transform.localScale=new Vector3(2.5f+i%3,1.5f,2);stone.GetComponent<Renderer>().sharedMaterial=Mat("V02 Stone",new Color(.32f,.36f,.32f));stone.isStatic=true;
            }
            var market=GameObject.Find("Cedar Market • accessible interior").transform;
            Model("furniture-kit","trashcan",new Vector3(31.6f,.2f,7),.6f,0,true);
            Text("WATER / FOOD / FIRST AID",new Vector3(22,2.65f,24.2f),0,.6f,new Color(.12f,.22f,.22f));
            for(int i=0;i<6;i++)Model("furniture-kit","cardboardBoxClosed",new Vector3(15+i*2.3f,1.18f,21),.45f,0,false);
        }
        static ItemDefinition[] Definitions()
        {
            var result=new ItemDefinition[7];for(int i=0;i<7;i++)result[i]=AssetDatabase.LoadAssetAtPath<ItemDefinition>(Root+"/Items/"+(ItemKind)i+".asset");return result;
        }
        static void Items()
        {
            string[] names={"Canned food","Bottled water","First-aid kit","Flashlight","Batteries","Backpack","Emergency radio"};
            string[] descriptions={"Easy energy: restores 12 health. No hunger meter in this prototype.","Fresh drinking water: restores 8 health on dry ground.","Clean dressings and medicine. Restores up to 40 health. Press H.","F toggles the beam. Charge drains only while lit. R loads batteries.","One pack restores the flashlight to 100% charge. Press R.","A larger bag. Collect to expand capacity from 6 to 12 slots.","Two slots. Press V for the current evacuation advisory."};
            Color[] colors={new Color(.9f,.5f,.13f),new Color(.24f,.72f,.9f),new Color(.85f,.22f,.17f),new Color(.94f,.8f,.3f),new Color(.45f,.76f,.46f),new Color(.26f,.5f,.55f),new Color(.66f,.68f,.63f)};
            var definitions=new ItemDefinition[7];
            for(int i=0;i<7;i++)
            {
                string path=Root+"/Items/"+(ItemKind)i+".asset";var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);if(!item){item=ScriptableObject.CreateInstance<ItemDefinition>();AssetDatabase.CreateAsset(item,path);}
                item.kind=(ItemKind)i;item.displayName=names[i];item.description=descriptions[i];item.color=colors[i];item.slotCost=i==5?0:i==6?2:1;item.icon=Icon((ItemKind)i,colors[i]);EditorUtility.SetDirty(item);definitions[i]=item;
            }
            var existing=UnityEngine.Object.FindObjectsByType<PickupItem>();
            // Existing food/water/medicine remain in town; move the home supplies to their new furniture.
            int food=0,water=0,aid=0;
            foreach(var pickup in existing)
            {
                if(pickup.transform.position.x<0)
                {
                    if(pickup.definition.kind==ItemKind.Food)pickup.transform.position=new Vector3(-24.4f,1.64f,-25.5f);
                    if(pickup.definition.kind==ItemKind.Water)pickup.transform.position=new Vector3(-23.2f,1.65f,-25.5f);
                    if(pickup.definition.kind==ItemKind.Medicine)pickup.transform.position=new Vector3(-16.35f,.99f,-32.1f);
                    if(pickup.definition.kind==ItemKind.Flashlight)pickup.transform.position=new Vector3(-24,1.21f,-28.3f);
                }
                if(pickup.definition.kind==ItemKind.Food)food++;if(pickup.definition.kind==ItemKind.Water)water++;if(pickup.definition.kind==ItemKind.Medicine)aid++;
            }
            Pickup(definitions[5],new Vector3(-17.1f,.55f,-27.1f));Pickup(definitions[4],new Vector3(-26.7f,1.56f,-25.4f));Pickup(definitions[6],new Vector3(-19,1.15f,-34.55f));
            Pickup(definitions[4],new Vector3(-54,1.26f,-72));Pickup(definitions[1],new Vector3(-53.3f,1.26f,-72));Pickup(definitions[2],new Vector3(27,1.35f,21));Pickup(definitions[0],new Vector3(17,1.35f,20));
            game.ui.definitions=definitions;
            AssetDatabase.SaveAssets();
        }
        static void Pickup(ItemDefinition definition,Vector3 pos)
        {
            var go=new GameObject(definition.displayName+" pickup v02");go.transform.SetParent(content);go.transform.position=pos;go.AddComponent<PickupItem>().definition=definition;
            var mat=Mat("V02 Item "+definition.kind,definition.color);var body=Box("Collectible body",pos,definition.kind==ItemKind.Backpack?new Vector3(.5f,.65f,.3f):new Vector3(.3f,.3f,.2f),mat,go.transform);body.isStatic=false;
            if(definition.kind==ItemKind.Radio){Box("Radio antenna",pos+new Vector3(.12f,.3f,0),new Vector3(.025f,.45f,.025f),metal,go.transform,false);Box("Radio dial",pos+new Vector3(-.07f,.04f,-.11f),new Vector3(.1f,.08f,.03f),white,go.transform,false);}
            if(definition.kind==ItemKind.Batteries){Box("Battery terminals",pos+Vector3.up*.18f,new Vector3(.2f,.05f,.1f),white,go.transform,false);}
            if(definition.kind==ItemKind.Backpack)for(int s=-1;s<=1;s+=2)Box("Backpack strap",pos+new Vector3(s*.16f,0,-.18f),new Vector3(.055f,.55f,.04f),slate,go.transform,false);
            foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.isStatic=false;
            string path=Root+"/Prefabs/V02"+definition.kind+"Pickup.prefab";if(!File.Exists(path))PrefabUtility.SaveAsPrefabAsset(go,path);
        }
        static Sprite Icon(ItemKind kind,Color tint)
        {
            const int size=48;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                bool fill=false;int dx=x-24,dy=y-24;
                switch(kind)
                {
                    case ItemKind.Food:fill=Mathf.Abs(dx)<13 && Mathf.Abs(dy)<17;break;
                    case ItemKind.Water:fill=Mathf.Abs(dx)<10 && y>7 && y<34 || Mathf.Abs(dx)<6 && y>=34 && y<43;break;
                    case ItemKind.Medicine:fill=Mathf.Abs(dx)<6 && Mathf.Abs(dy)<17 || Mathf.Abs(dx)<17 && Mathf.Abs(dy)<6;break;
                    case ItemKind.Flashlight:fill=Mathf.Abs(dx)<7 && y>6 && y<31 || Mathf.Abs(dx)<13 && y>=31 && y<42;break;
                    case ItemKind.Batteries:fill=((x>7 && x<21)||(x>27 && x<41)) && y>9 && y<38;break;
                    case ItemKind.Backpack:fill=Mathf.Abs(dx)<16 && y>6 && y<36 || Mathf.Abs(dx)<9 && y>=36 && y<42;break;
                    case ItemKind.Radio:fill=Mathf.Abs(dx)<17 && y>7 && y<32 || x>32 && x<36 && y>=32 && y<45;break;
                }
                pixels[y*size+x]=fill?(Color32)tint:new Color32(0,0,0,0);
                if(fill && kind==ItemKind.Food && Mathf.Abs(dy)<4)pixels[y*size+x]=new Color32(245,237,211,255);
                if(fill && kind==ItemKind.Radio && x>11 && x<25 && y>13 && y<24)pixels[y*size+x]=new Color32(30,44,48,255);
            }
            texture.SetPixels32(pixels);texture.Apply();string path=Root+"/Icons/"+kind+".png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static void Television(OpeningSequence story)
        {
            var root=new GameObject("Original cartoon / emergency television");root.transform.SetParent(content);root.transform.SetPositionAndRotation(new Vector3(-26,1.66f,-34.91f),Quaternion.Euler(0,180,0));
            var tv=root.AddComponent<TelevisionScreen>();story.television=tv;
            var canvas=new GameObject("TV canvas",typeof(RectTransform),typeof(Canvas));canvas.transform.SetParent(root.transform,false);canvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            var rt=canvas.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(800,450);rt.localScale=Vector3.one*.00305f;
            var cartoon=UiRect("CLOUD FOX",canvas.transform,new Vector2(800,450),Vector2.zero,new Color(.27f,.65f,.84f));tv.cartoon=cartoon.gameObject;
            UiRect("Hills",cartoon,new Vector2(800,175),new Vector2(0,-150),new Color(.35f,.65f,.25f));
            var cloud=UiRect("Cloud",cartoon,new Vector2(170,45),new Vector2(0,125),Color.white);tv.cloud=cloud;
            var fox=UiRect("Original fox character",cartoon,new Vector2(120,100),new Vector2(-90,-30),new Color(.94f,.43f,.13f));tv.fox=fox;
            UiRect("Fox muzzle",fox,new Vector2(85,40),new Vector2(0,-25),new Color(1,.9f,.62f));
            UiRect("Left ear",fox,new Vector2(28,45),new Vector2(-40,63),new Color(.9f,.33f,.08f));UiRect("Right ear",fox,new Vector2(28,45),new Vector2(40,63),new Color(.9f,.33f,.08f));
            UiRect("Eye",fox,new Vector2(10,17),new Vector2(-28,10),Color.black);UiRect("Eye",fox,new Vector2(10,17),new Vector2(28,10),Color.black);
            UiRect("Nose",fox,new Vector2(14,12),new Vector2(0,-14),Color.black);
            tv.bird=UiRect("Sunbird",cartoon,new Vector2(42,33),new Vector2(180,60),new Color(1,.85f,.18f));UiRect("Bird wing",tv.bird,new Vector2(32,12),new Vector2(-25,7),new Color(.95f,.58f,.1f));
            UiText("CLOUD FOX & SUNBIRD",cartoon,new Vector2(650,45),new Vector2(0,185),27,Color.white);
            UiText("An original Cedar Valley cartoon",cartoon,new Vector2(700,30),new Vector2(0,-200),16,Color.white);
            var news=UiRect("Breaking news",canvas.transform,new Vector2(800,450),Vector2.zero,new Color(.035f,.08f,.14f));tv.news=news.gameObject;
            UiRect("Red band",news,new Vector2(800,95),new Vector2(0,175),new Color(.72f,.09f,.07f));
            UiText("BREAKING NEWS",news,new Vector2(760,65),new Vector2(0,175),48,Color.white);
            UiText("CEDAR DAM EMERGENCY",news,new Vector2(750,65),new Vector2(0,70),36,new Color(1,.72f,.28f));
            UiText("SEVERE STRUCTURAL DAMAGE\nDOWNSTREAM EVACUATION ORDER",news,new Vector2(740,120),new Vector2(0,-30),26,Color.white);
            tv.ticker=UiText("MOVE TO HIGHER GROUND / DO NOT WAIT",news,new Vector2(750,65),new Vector2(0,-170),25,Color.white);
            news.gameObject.SetActive(false);
            var glow=new GameObject("TV screen glow").AddComponent<Light>();glow.transform.SetParent(content);glow.transform.position=new Vector3(-26,1.7f,-34.4f);glow.type=LightType.Point;glow.intensity=1.3f;glow.range=4;tv.screenGlow=glow;
            var speaker=root.AddComponent<AudioSource>();speaker.playOnAwake=false;speaker.spatialBlend=.8f;speaker.minDistance=2;speaker.maxDistance=20;tv.speaker=speaker;game.audioSystem.television=speaker;
        }
        static RectTransform UiRect(string name,Transform parent,Vector2 size,Vector2 position,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=position;var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return rt;
        }
        static TMP_Text UiText(string text,Transform parent,Vector2 size,Vector2 position,int fontSize,Color color)
        {
            var go=new GameObject(text,typeof(RectTransform),typeof(TextMeshProUGUI));var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=position;var label=go.GetComponent<TMP_Text>();label.font=font;label.text=text;label.fontSize=fontSize;label.color=color;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;return label;
        }
        static Material ParticleMaterial()
        {
            string path=Root+"/Materials/V02 Soft particles.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(mat,path);}
            var texture=new Texture2D(32,32,TextureFormat.RGBA32,false);var colors=new Color[1024];for(int y=0;y<32;y++)for(int x=0;x<32;x++)colors[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/16),1.6f));texture.SetPixels(colors);texture.Apply();string texPath=Root+"/Icons/SoftParticle.png";File.WriteAllBytes(texPath,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(texPath);mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);mat.SetFloat("_Surface",1);mat.SetFloat("_Blend",0);mat.SetFloat("_SrcBlend",5);mat.SetFloat("_DstBlend",10);mat.SetFloat("_ZWrite",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.renderQueue=3000;EditorUtility.SetDirty(mat);return mat;
        }
        static ParticleSystem Effect(string name,Vector3 pos,int burst,Color color,float size,float speed,float lifetime)
        {
            var system=new GameObject(name).AddComponent<ParticleSystem>();system.transform.SetParent(content);system.transform.position=pos;system.transform.rotation=Quaternion.Euler(-70,0,0);
            var main=system.main;main.playOnAwake=false;main.loop=false;main.duration=2;main.startLifetime=lifetime;main.startSpeed=speed;main.startSize=size;main.startColor=color;main.maxParticles=400;
            var emission=system.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)burst)});var shape=system.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=5;shape.angle=25;
            system.GetComponent<ParticleSystemRenderer>().sharedMaterial=particle;system.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);return system;
        }
        static void UpgradeDam(OpeningSequence story)
        {
            var cracks=new GameObject("Dam visible fractures");cracks.transform.SetParent(content);story.damCracks=cracks;cracks.SetActive(false);
            var dark=Mat("V02 Fracture",new Color(.025f,.04f,.04f));
            for(int i=0;i<5;i++)
            {
                var line=new GameObject("Dam crack "+i).AddComponent<LineRenderer>();line.transform.SetParent(cracks.transform);line.sharedMaterial=dark;line.startWidth=.15f;line.endWidth=.05f;line.positionCount=8;
                for(int j=0;j<8;j++)line.SetPosition(j,new Vector3(-14+i*7+Mathf.Sin(j*2+i)*1.1f,29-j*3.3f,103.8f));
            }
            story.damDust=Effect("Pre-breach dust",new Vector3(0,26,102),90,new Color(.65f,.57f,.43f,.55f),3,3,6);
            story.leaks=Effect("Dam leak jets",new Vector3(0,13,102),150,new Color(.6f,.8f,.81f,.7f),.6f,10,4);
            game.flood.collapseDust=Effect("Dam collapse dust",new Vector3(0,20,103),240,new Color(.62f,.55f,.43f,.65f),5,8,9);
            var debris=new Transform[18];var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Concrete.mat");
            for(int i=0;i<debris.Length;i++){var go=Box("Collapse chunk "+i,new Vector3(-7+(i%6)*2.8f,5+(i/6)*10,104),new Vector3(2.4f,4+i%3,3),mat,null,false);go.isStatic=false;go.SetActive(false);debris[i]=go.transform;}game.flood.fallingDebris=debris;
            var front=Box("Downstream whitewater front",new Vector3(0,-2,108),new Vector3(235,.32f,5),Mat("V02 Surge foam",new Color(.58f,.76f,.75f,.82f),.3f,true),null,false);front.isStatic=false;front.SetActive(false);game.flood.surgeFront=front.transform;
            if(game.flood.breachSpray){game.flood.breachSpray.GetComponent<ParticleSystemRenderer>().sharedMaterial=particle;var main=game.flood.breachSpray.main;main.startSize=1.4f;main.maxParticles=400;}
            game.flood.frontSpeed=4.2f;game.flood.currentStrength=1.7f;
            var lamp=new GameObject("Home emergency warning lamp").AddComponent<Light>();lamp.transform.SetParent(content);lamp.transform.position=new Vector3(-22,2.8f,-36);lamp.color=new Color(1,.25f,.05f);lamp.range=7;lamp.intensity=0;story.warningLamp=lamp;
        }
        static void UpgradeNPCs()
        {
            string modelPath=Root+"/Art/Survivors/Model/characterMedium.fbx";
            var modelImporter=(ModelImporter)AssetImporter.GetAtPath(modelPath);modelImporter.animationType=ModelImporterAnimationType.Human;modelImporter.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;modelImporter.SaveAndReimport();
            var avatar=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath).GetComponent<Animator>().avatar;
            foreach(string animation in new[]{"idle","run"})
            {
                var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"/Art/Survivors/Animations/"+animation+".fbx");importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.sourceAvatar=null;
                var clips=importer.defaultClipAnimations;foreach(var clip in clips){clip.loopTime=true;clip.lockRootPositionXZ=true;clip.lockRootHeightY=true;clip.lockRootRotation=true;clip.keepOriginalPositionXZ=true;}importer.clipAnimations=clips;importer.SaveAndReimport();
            }
            AnimationClip idle=null,run=null;
            foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(Root+"/Art/Survivors/Animations/idle.fbx"))if(asset is AnimationClip clip && !clip.name.StartsWith("__") && clip.name.IndexOf("Idle",StringComparison.OrdinalIgnoreCase)>=0)idle=clip;
            foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(Root+"/Art/Survivors/Animations/run.fbx"))if(asset is AnimationClip clip && !clip.name.StartsWith("__") && clip.name.Contains("Run"))run=clip;
            if(!idle || !run || !avatar || !avatar.isValid)throw new Exception("Kenney character rig or animation is invalid");
            string controllerPath=Root+"/Animations/EvacueeV02.controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if(!controller)
            {
                controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);var machine=controller.layers[0].stateMachine;var idleState=machine.AddState("Idle");idleState.motion=idle;var moving=machine.AddState("Move");moving.motion=run;moving.speed=.85f;
                var start=idleState.AddTransition(moving);start.hasExitTime=false;start.duration=.18f;start.AddCondition(AnimatorConditionMode.Greater,.2f,"Speed");var stop=moving.AddTransition(idleState);stop.hasExitTime=false;stop.duration=.2f;stop.AddCondition(AnimatorConditionMode.Less,.15f,"Speed");
            }
            for(int i=0;i<game.npcs.citizens.Length;i++)
            {
                var npc=game.npcs.citizens[i];npc.GetComponent<NavMeshAgent>().enabled=false;
                foreach(var child in npc.GetComponentsInChildren<Renderer>())child.gameObject.SetActive(false);
                var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));visual.transform.SetParent(npc.transform,false);visual.transform.localPosition=Vector3.zero;var bounds=BoundsOf(visual);float scale=1.78f/bounds.size.y;visual.transform.localScale=Vector3.one*scale;
                var material=Mat("V02 Survivor skin "+i,Color.white);material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/Survivors/Skins/"+(i%2==0?"survivorMaleB":"survivorFemaleA")+".png");EditorUtility.SetDirty(material);foreach(var r in visual.GetComponentsInChildren<Renderer>())r.sharedMaterial=material;
                var animator=visual.GetComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.CullUpdateTransforms;npc.animator=animator;npc.body=npc.leftArm=npc.rightArm=npc.leftLeg=npc.rightLeg=null;
                foreach(var t in visual.GetComponentsInChildren<Transform>()){t.gameObject.layer=2;t.gameObject.isStatic=false;}
                npc.collectBelongings=i%3==0;npc.homeStop=new Vector3(-22,.25f,-37-i%2);
                if(i==0)npc.transform.position=new Vector3(-22.5f,.25f,-27.8f);
                if(i==1)npc.transform.position=new Vector3(-51,.25f,-75);
                EditorUtility.SetDirty(npc);
            }
            PrefabUtility.SaveAsPrefabAsset(game.npcs.citizens[0].gameObject,Root+"/Prefabs/EvacueeV02.prefab");
        }
        static void Ambient(OpeningSequence story)
        {
            var birdAudio=new GameObject("Birdsong over Cedar Avenue").AddComponent<AudioSource>();birdAudio.transform.SetParent(content);birdAudio.transform.position=new Vector3(-5,7,-20);birdAudio.spatialBlend=.7f;birdAudio.minDistance=20;birdAudio.maxDistance=110;birdAudio.playOnAwake=false;game.audioSystem.birds=birdAudio;
            var damAudio=new GameObject("Dam cracking spatial audio").AddComponent<AudioSource>();damAudio.transform.SetParent(content);damAudio.transform.position=new Vector3(0,20,100);damAudio.spatialBlend=.7f;damAudio.minDistance=70;damAudio.maxDistance=350;damAudio.playOnAwake=false;game.audioSystem.damSource=damAudio;
            var floodAudio=new GameObject("Flood roar").AddComponent<AudioSource>();floodAudio.transform.SetParent(content);floodAudio.playOnAwake=false;game.audioSystem.floodSource=floodAudio;
            var voices=new AudioSource[4];for(int i=0;i<4;i++){var source=new GameObject("Resident reaction voice "+i).AddComponent<AudioSource>();source.transform.SetParent(content);source.playOnAwake=false;source.spatialBlend=1;source.minDistance=3;source.maxDistance=40;voices[i]=source;}game.audioSystem.voices=voices;
            var birds=Effect("Startled birds",new Vector3(0,9,-10),26,new Color(.12f,.18f,.2f),.65f,10,8);var main=birds.main;main.gravityModifier=0;var shape=birds.shape;shape.radius=25;
            var mesh=new Mesh();mesh.name="Original bird silhouette";mesh.vertices=new[]{new Vector3(-.7f,.15f,0),new Vector3(0,-.08f,0),new Vector3(.7f,.15f,0),new Vector3(0,.1f,.18f)};mesh.triangles=new[]{0,1,3,3,1,2};mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,Root+"/Settings/BirdSilhouette.asset");var renderer=birds.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Mesh;renderer.mesh=mesh;renderer.sharedMaterial=metal;story.startledBirds=birds;
        }
        static void Polish()
        {
            var light=GameObject.Find("Late afternoon sun").GetComponent<Light>();light.intensity=2.2f;light.color=new Color(1,.87f,.69f);
            RenderSettings.fogDensity=.0026f;RenderSettings.fogColor=new Color(.49f,.6f,.62f);RenderSettings.ambientSkyColor=new Color(.52f,.62f,.65f);
            var profile=GameObject.Find("Storm atmosphere").GetComponent<Volume>().sharedProfile;if(profile.TryGet<ColorAdjustments>(out var color)){color.saturation.value=-3;color.postExposure.Override(.15f);EditorUtility.SetDirty(color);}if(profile.TryGet<Vignette>(out var vignette)){vignette.intensity.value=.16f;EditorUtility.SetDirty(vignette);}EditorUtility.SetDirty(profile);
            foreach(var t in content.GetComponentsInChildren<Transform>(true))if(t.GetComponent<Light>() || t.GetComponent<ParticleSystem>() || t.GetComponent<AudioSource>())t.gameObject.isStatic=false;
        }
        [MenuItem("Flood Survival/Polish seated view")]
        public static void PolishSeatedView()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play mode first");
            game=UnityEngine.Object.FindFirstObjectByType<GameManager>();
            FitExisting("loungeSofa",new Vector3(-26,.2f,-32.3f),2.8f,0);
            FitExisting("tableCoffee",new Vector3(-26,.2f,-34),1.45f,0);
            FitExisting("plantSmall1",new Vector3(-25.65f,.71f,-34),.26f,0);
            FitExisting("books",new Vector3(-26.4f,.71f,-34),.35f,0);
            var cabinet=GameObject.Find("cabinetTelevision");var cabinetBounds=BoundsOf(cabinet);var cabinetScale=cabinet.transform.localScale;cabinetScale.y*=.62f/cabinetBounds.size.y;cabinet.transform.localScale=cabinetScale;cabinetBounds=BoundsOf(cabinet);cabinet.transform.position+=Vector3.up*(.2f-cabinetBounds.min.y);
            game.story.seat.position=new Vector3(-26,.22f,-33.05f);
            game.story.television.transform.position=new Vector3(-26,1.65f,-34.7f);
            game.story.daylight=GameObject.Find("Late afternoon sun").GetComponent<Light>();
            game.player.transform.SetPositionAndRotation(game.story.seat.position,game.story.seat.rotation);game.player.view.transform.localPosition=Vector3.up*1.15f;
            var surface=GameObject.Find("Cedar Valley • Environment").GetComponent<NavMeshSurface>();surface.BuildNavMesh();string path=Root+"/Settings/CedarValleyV02NavMesh.asset";if(AssetDatabase.LoadAssetAtPath<NavMeshData>(path))AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(surface.navMeshData,path);AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        static void FitExisting(string name,Vector3 pos,float width,float yaw)
        {
            var go=GameObject.Find(name);if(!go)return;var bounds=BoundsOf(go);Vector3 center=bounds.center;go.transform.RotateAround(center,Vector3.up,Mathf.DeltaAngle(go.transform.eulerAngles.y,yaw));
            bounds=BoundsOf(go);go.transform.localScale*=width/bounds.size.x;bounds=BoundsOf(go);go.transform.position+=pos-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        }
        public static void BindDialogue()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play mode first");
            AssetDatabase.Refresh();game=UnityEngine.Object.FindFirstObjectByType<GameManager>();
            var news=new AudioClip[4];for(int i=0;i<4;i++)news[i]=LoadSpeech("News0"+(i+1));game.story.newsVoices=news;
            var residents=new AudioClip[3];for(int i=0;i<3;i++)residents[i]=LoadSpeech("Resident0"+(i+1));game.audioSystem.residentLines=residents;game.audioSystem.radioSpeech=LoadSpeech("RadioAdvisory");
            EditorUtility.SetDirty(game.story);EditorUtility.SetDirty(game.audioSystem);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static AudioClip LoadSpeech(string name)
        {
            string path=Root+"/Audio/"+name+".wav";var importer=AssetImporter.GetAtPath(path) as AudioImporter;if(!importer)throw new Exception("Speech asset missing: "+path);
            importer.forceToMono=true;var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;importer.defaultSampleSettings=settings;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
