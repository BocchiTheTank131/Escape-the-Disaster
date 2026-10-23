using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.AI.Navigation;
using TMPro;
namespace FloodSurvival.Editor
{
    public static class LandscapeRevision
    {
        const string Root="Assets/FloodSurvival",Art=Root+"/Art/PolyHavenLandscape",Folder=Root+"/Landscape";
        static Transform world,root;
        static GameManager game;
        [MenuItem("Flood Survival/Apply wooded landscape and TV revision")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play mode first");
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=Root+"/Scenes/CedarValley.unity")EditorSceneManager.OpenScene(Root+"/Scenes/CedarValley.unity");
            if(GameObject.Find("Wooded valley revision")){Debug.Log("Landscape revision is already applied; edit the saved scene normally.");return;}
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            world=GameObject.Find("Cedar Valley • Environment").transform;game=UnityEngine.Object.FindFirstObjectByType<GameManager>();
            root=new GameObject("Wooded valley revision").transform;root.SetParent(world,false);
            RemoveShelter();CreateTerrain();CreateForest();CreateTrail();
            TelevisionUpgrade.Apply(game);
            game.flood.FinalLevel=10.6f;
            game.objectives.highGround.name="Wooded high ground waypoint";game.objectives.highGround.position=new Vector3(93,15.25f,-46);
            for(int i=0;i<game.npcs.citizens.Length;i++)game.npcs.citizens[i].destination=new Vector3(87+i%3*2.5f,15.25f,-51+i/3*4.2f);
            var nav=world.GetComponent<NavMeshSurface>();nav.BuildNavMesh();
            var navData=UnityEngine.Object.Instantiate(nav.navMeshData);navData.name="Wooded valley navigation";AssetDatabase.CreateAsset(navData,Folder+"/WoodedValleyNavMesh.asset");nav.RemoveData();nav.navMeshData=navData;nav.AddData();
            PlayerSettings.bundleVersion="0.2.1";EditorUtility.SetDirty(game.flood);EditorUtility.SetDirty(game.objectives);EditorUtility.SetDirty(game.player.view);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Debug.Log("Shelter removed. Roof-level flood 10.6 m, forested terrain, natural ridge trail and upgraded TV saved.");
        }
        static void RemoveShelter()
        {
            var removal=new List<GameObject>();
            foreach(var t in world.GetComponentsInChildren<Transform>(true))
            {
                string n=t.name;
                if(n=="Mountain ridge" || n.StartsWith("Town perimeter") || n.StartsWith("Shelter ") || n=="Canopy pillar" || n=="Evacuation plateau" || n=="Emergency supply crate" || n=="Ridge beacon" || n.StartsWith("Sign • RIDGE SHELTER") || n.StartsWith("Notice SAFE ABOVE THE FLOOD"))removal.Add(t.gameObject);
            }
            foreach(var go in removal)if(go)Undo.DestroyObjectImmediate(go);
            foreach(var text in world.GetComponentsInChildren<TMP_Text>(true))text.text=text.text.Replace("RIDGE SHELTER","WOODED HILLS").Replace("Ridge Shelter","wooded high ground");
            var ramp=GameObject.Find("Ridge trail • walkable slope");if(ramp){ramp.name="Ridge trail collision underlay";ramp.GetComponent<MeshRenderer>().enabled=false;}
        }
        // Continuous valley with warped ridges, secondary shoulders and a walkable eastern trail.
        static float Smooth(float low,float high,float value) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(low,high,value));
        public static float Height(float x,float z)
        {
            float warp=(Mathf.PerlinNoise(x*.009f+23,z*.009f+71)-.5f)*30;
            float side=Mathf.Abs(x+warp)-66;
            float south=-z-103;
            float north=z-119;
            float distance=Mathf.Max(side,south,north);
            float rise=Smooth(0,1,Mathf.Clamp01(distance/125));
            float ridge=1-Mathf.Abs(Mathf.PerlinNoise(x*.014f+17,z*.014f+52)*2-1);
            float detail=Mathf.PerlinNoise(x*.042f+80,z*.042f+11)*7+Mathf.PerlinNoise(x*.095f+5,z*.095f+8)*2;
            float h=-1.6f+rise*(58+ridge*73+detail);
            // A saddle behind the dam keeps its face and downstream source legible.
            if(z>92 && z<160 && Mathf.Abs(x)<68)h=Mathf.Lerp(h,-1.6f,(1-Smooth(44,68,Mathf.Abs(x)))*(1-Smooth(142,160,z)));
            if(x>32 && x<134)
            {
                float corridor=(1-Smooth(4.5f,22,Mathf.Abs(z+46)))*Smooth(32,37,x)*(1-Smooth(111,134,x));
                float trail=Mathf.Clamp((x-36)*.3125f+.18f,0,15.18f);
                h=Mathf.Lerp(h,trail,corridor);
            }
            if(x>80 && x<111 && z>-61 && z<-31)
            {
                float bench=Smooth(80,84,x)*(1-Smooth(107,111,x))*Smooth(-61,-58,z)*(1-Smooth(-34,-31,z));
                h=Mathf.Lerp(h,15.18f,bench);
            }
            return h;
        }
        static Texture2D Texture(string file,bool normal)
        {
            string path=Art+"/"+file+(Path.HasExtension(file)?"":".png");var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            if(importer==null)throw new Exception("Missing terrain map "+path);
            importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.maxTextureSize=2048;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static void CreateTerrain()
        {
            var ground=new TerrainLayer{diffuseTexture=Texture("forest_ground_04_Diffuse",false),normalMapTexture=Texture("forest_ground_04_nor_gl",true),tileSize=new Vector2(9,9),normalScale=.65f,smoothness=.05f};ground.name="Forest floor CC0";AssetDatabase.CreateAsset(ground,Folder+"/ForestFloor.terrainlayer");
            var stone=new TerrainLayer{diffuseTexture=Texture("rock_face_03_Diffuse",false),normalMapTexture=Texture("rock_face_03_nor_gl",true),tileSize=new Vector2(20,20),normalScale=.85f,smoothness=.08f};stone.name="Mountain rock CC0";AssetDatabase.CreateAsset(stone,Folder+"/MountainRock.terrainlayer");
            var data=new TerrainData{heightmapResolution=513,alphamapResolution=256,baseMapResolution=1024,size=new Vector3(720,180,720)};data.name="Cedar Valley continuous mountain terrain";
            var heights=new float[513,513];for(int z=0;z<513;z++)for(int x=0;x<513;x++)heights[z,x]=(Height(-360+x*720f/512,-340+z*720f/512)+2)/180;
            data.SetHeights(0,0,heights);data.terrainLayers=new[]{ground,stone};
            var blend=new float[256,256,2];for(int z=0;z<256;z++)for(int x=0;x<256;x++)
            {
                float nx=x/255f,nz=z/255f;float h=data.GetInterpolatedHeight(nx,nz)-2,slope=data.GetSteepness(nx,nz);
                float rock=Mathf.Clamp01(Mathf.InverseLerp(19,48,slope)*.8f+Mathf.InverseLerp(48,108,h)*.7f+(Mathf.PerlinNoise(nx*35,nz*35)-.5f)*.2f);
                blend[z,x,0]=1-rock;blend[z,x,1]=rock;
            }
            data.SetAlphamaps(0,0,blend);AssetDatabase.CreateAsset(data,Folder+"/CedarValleyMountains.asset");
            var go=Terrain.CreateTerrainGameObject(data);go.name="Continuous forested mountains";go.transform.SetParent(root);go.transform.position=new Vector3(-360,-2,-340);go.isStatic=true;
            var terrain=go.GetComponent<Terrain>();terrain.heightmapPixelError=5;terrain.basemapDistance=500;terrain.drawInstanced=true;
            var material=new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));material.name="Photographic mountain terrain";AssetDatabase.CreateAsset(material,Folder+"/MountainTerrain.mat");terrain.materialTemplate=material;
        }
        sealed class MeshParts
        {
            public List<Vector3> v=new List<Vector3>();public List<int>[] triangles={new List<int>(),new List<int>(),new List<int>()};
            public void Ring(Vector3 center,float radius,float height,int sides,int material,float phase,float irregular)
            {
                int start=v.Count;
                for(int level=0;level<3;level++)for(int i=0;i<sides;i++)
                {
                    float a=i*Mathf.PI*2/sides+phase;
                    float r=radius*(level==0?.72f:level==1?1:.055f)*(1+Mathf.Sin(i*2.73f+phase)*irregular);
                    v.Add(center+new Vector3(Mathf.Cos(a)*r,height*(level==0?0:level==1?.16f:1)+Mathf.Sin(i*4+phase)*height*.025f,Mathf.Sin(a)*r));
                }
                for(int k=0;k<2;k++)for(int i=0;i<sides;i++)
                {
                    int a=start+k*sides+i,b=start+k*sides+(i+1)%sides,c=a+sides,d=b+sides;
                    triangles[material].AddRange(new[]{a,c,b,b,c,d});
                }
            }
            public Mesh Create(string name)
            {
                var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.subMeshCount=3;for(int i=0;i<3;i++)mesh.SetTriangles(triangles[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            }
        }
        static Mesh Pine(int seed)
        {
            var saved=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/Pine"+seed+".asset");if(saved)return saved;
            var parts=new MeshParts();parts.Ring(new Vector3(0,0,0),.16f,9.1f,8,0,seed,.05f);
            for(int tier=0;tier<9;tier++)
            {
                float t=tier/8f;float y=1.5f+t*6.4f;float radius=(1-t*.82f)*(1.35f+.17f*Mathf.Sin(seed+tier*2));
                parts.Ring(new Vector3(Mathf.Sin(t*2+seed)*t*.23f,y,Mathf.Cos(t*2+seed)*t*.18f),radius,1.8f-t*.65f,14,tier%3==0?2:1,seed+tier*.73f,.18f);
            }
            var mesh=parts.Create("Branching mountain pine "+seed);AssetDatabase.CreateAsset(mesh,Folder+"/Pine"+seed+".asset");return mesh;
        }
        static Material Mat(string name,Color color)
        {
            var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/"+name+".mat");if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(material,Folder+"/"+name+".mat");}material.color=color;material.SetFloat("_Smoothness",.07f);material.SetFloat("_Cull",0);EditorUtility.SetDirty(material);return material;
        }
        static void CreateForest()
        {
            var materials=new[]{Mat("Pine bark",new Color(.24f,.18f,.12f)),Mat("Pine deep needles",new Color(.16f,.30f,.18f)),Mat("Pine sunlit needles",new Color(.25f,.41f,.24f))};
            var pines=new[]{Pine(1),Pine(2),Pine(3)};var random=new System.Random(5241);var groups=new Dictionary<Vector2Int,List<Matrix4x4>>();var variants=new Dictionary<Vector2Int,List<int>>();int count=0;
            for(int attempt=0;attempt<40000 && count<1400;attempt++)
            {
                float x=-285+(float)random.NextDouble()*570,z=-265+(float)random.NextDouble()*565;float h=Height(x,z);
                if(h<3 || h>105 || Mathf.Abs(x)<65 && z>-110 && z<135 || x>34 && x<120 && Mathf.Abs(z+46)<8)continue;
                if(x>78 && x<115 && z>-62 && z<-30)continue;
                float slope=Mathf.Sqrt(Mathf.Pow(Height(x+1,z)-h,2)+Mathf.Pow(Height(x,z+1)-h,2));if(slope>1.05f)continue;
                if(random.NextDouble()<Mathf.PerlinNoise(x*.025f+2,z*.025f+3)*.23f)continue;
                var key=new Vector2Int(Mathf.FloorToInt(x/75),Mathf.FloorToInt(z/75));if(!groups.ContainsKey(key)){groups[key]=new List<Matrix4x4>();variants[key]=new List<int>();}
                float scale=.75f+(float)random.NextDouble()*.95f;
                groups[key].Add(Matrix4x4.TRS(new Vector3(x,h-.05f,z),Quaternion.Euler(0,(float)random.NextDouble()*360,0),new Vector3(scale*(.9f+(float)random.NextDouble()*.2f),scale,scale)));
                variants[key].Add(count%3);count++;
            }
            // Trees frame the open high-ground landing rather than obstructing the evacuation path.
            foreach(var q in new[]{new Vector2(79,-64),new Vector2(87,-65),new Vector2(98,-65),new Vector2(112,-61),new Vector2(114,-50),new Vector2(113,-35),new Vector2(100,-28),new Vector2(86,-28),new Vector2(76,-30)})
            {
                var key=new Vector2Int(Mathf.FloorToInt(q.x/75),Mathf.FloorToInt(q.y/75));if(!groups.ContainsKey(key)){groups[key]=new List<Matrix4x4>();variants[key]=new List<int>();}
                groups[key].Add(Matrix4x4.TRS(new Vector3(q.x,Height(q.x,q.y),q.y),Quaternion.Euler(0,count*47,0),Vector3.one*1.1f));variants[key].Add(count%3);count++;
            }
            var forest=new GameObject("Pine forest • "+count+" trees").transform;forest.SetParent(root,false);int chunk=0;
            foreach(var pair in groups)
            {
                var components=new Mesh[3];
                for(int material=0;material<3;material++)
                {
                    var combines=new CombineInstance[pair.Value.Count];for(int i=0;i<combines.Length;i++)combines[i]=new CombineInstance{mesh=pines[variants[pair.Key][i]],subMeshIndex=material,transform=pair.Value[i]};
                    components[material]=new Mesh{indexFormat=IndexFormat.UInt32};components[material].CombineMeshes(combines,true,true);
                }
                var result=new Mesh{name="Forest patch "+chunk,indexFormat=IndexFormat.UInt32};var merge=new CombineInstance[3];for(int i=0;i<3;i++)merge[i]=new CombineInstance{mesh=components[i],transform=Matrix4x4.identity};result.CombineMeshes(merge,false,true);string path=Folder+"/ForestPatch"+chunk+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(saved){EditorUtility.CopySerialized(result,saved);UnityEngine.Object.DestroyImmediate(result);result=saved;EditorUtility.SetDirty(result);}else AssetDatabase.CreateAsset(result,path);
                foreach(var part in components)UnityEngine.Object.DestroyImmediate(part);
                var go=new GameObject("Pine forest patch "+chunk,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(forest,false);go.GetComponent<MeshFilter>().sharedMesh=result;var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterials=materials;renderer.shadowCastingMode=ShadowCastingMode.On;go.isStatic=true;chunk++;
            }
            File.WriteAllText(Folder+"/ForestManifest.json","{\"seed\":5241,\"trees\":"+count+",\"patches\":"+chunk+",\"source\":\"original authored branching pine meshes\"}");
        }
        public static void RefineVegetation()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play mode first");
            root=GameObject.Find("Wooded valley revision").transform;world=root.parent;
            var data=root.GetComponentInChildren<Terrain>().terrainData;
            var grass=AssetDatabase.LoadAssetAtPath<TerrainLayer>(Folder+"/MountainGrass.terrainlayer");if(!grass){grass=new TerrainLayer{name="Rocky mountain grass CC0"};AssetDatabase.CreateAsset(grass,Folder+"/MountainGrass.terrainlayer");}
            grass.diffuseTexture=Texture("aerial_grass_rock_Diffuse.jpg",false);grass.normalMapTexture=Texture("aerial_grass_rock_nor_gl.jpg",true);grass.tileSize=new Vector2(14,14);grass.normalScale=.55f;grass.smoothness=.03f;EditorUtility.SetDirty(grass);
            var ground=AssetDatabase.LoadAssetAtPath<TerrainLayer>(Folder+"/ForestFloor.terrainlayer");var rock=AssetDatabase.LoadAssetAtPath<TerrainLayer>(Folder+"/MountainRock.terrainlayer");data.terrainLayers=new[]{grass,ground,rock};
            var blend=new float[256,256,3];for(int z=0;z<256;z++)for(int x=0;x<256;x++)
            {
                float nx=x/255f,nz=z/255f,h=data.GetInterpolatedHeight(nx,nz)-2,slope=data.GetSteepness(nx,nz);
                float stone=Mathf.Clamp01(Mathf.InverseLerp(30,58,slope)*.78f+Mathf.InverseLerp(75,120,h)*.85f);
                float soil=(.08f+Mathf.PerlinNoise(nx*35+8,nz*35+42)*.16f)*(1-stone);
                blend[z,x,0]=1-stone-soil;blend[z,x,1]=soil;blend[z,x,2]=stone;
            }
            data.SetAlphamaps(0,0,blend);EditorUtility.SetDirty(data);
            foreach(var t in root.GetComponentsInChildren<Transform>())if(t && t.name.StartsWith("Pine forest •")){Undo.DestroyObjectImmediate(t.gameObject);break;}
            CreateForest();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static void CreateTrail()
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
            for(int i=0;i<=60;i++)
            {
                float x=35+i*1.15f;for(int side=0;side<2;side++){float z=-46+(side==0?-1.7f:1.7f);vertices.Add(new Vector3(x,Height(x,z)+.035f,z));uv.Add(new Vector2(side,i*.4f));}
                if(i>0){int k=i*2;triangles.AddRange(new[]{k-2,k-1,k,k-1,k+1,k});}
            }
            var mesh=new Mesh{name="Forest footpath"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,Folder+"/ForestFootpath.asset");
            var path=new GameObject("Earth ridge trail",typeof(MeshFilter),typeof(MeshRenderer));path.transform.SetParent(root,false);path.GetComponent<MeshFilter>().sharedMesh=mesh;
            var material=Mat("Packed earth trail",new Color(.67f,.57f,.40f));material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/forest_ground_04_Diffuse.png");EditorUtility.SetDirty(material);path.GetComponent<MeshRenderer>().sharedMaterial=material;path.isStatic=true;
            foreach(var t in world.GetComponentsInChildren<Transform>())if(t.name=="Amber trail post" || t.name=="Trail reflector"){var p=t.position;p.y=Height(p.x,p.z)+(t.name=="Amber trail post"?.9f:1.7f);t.position=p;}
        }
    }
}

