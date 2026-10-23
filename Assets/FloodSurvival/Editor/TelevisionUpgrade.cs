using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;
namespace FloodSurvival.Editor
{
    public static class TelevisionUpgrade
    {
        const string Folder="Assets/FloodSurvival/Landscape";
        static Sprite circle,round,triangle;
        static TMP_FontAsset font;
        static Color navy=new Color(.025f,.065f,.115f),white=new Color(.96f,.97f,.91f),red=new Color(.78f,.07f,.10f),gold=new Color(1,.69f,.22f);
        public static void Apply(GameManager game)
        {
            var tv=game.story.television;font=game.ui.font;
            circle=Shape("Circle",0);round=Shape("Round",1);triangle=Shape("Triangle",2);
            // Keep the existing television component, two programme roots and spatial speaker.
            foreach(Transform child in tv.cartoon.transform)child.gameObject.SetActive(false);
            foreach(Transform child in tv.news.transform)child.gameObject.SetActive(false);
            tv.cartoon.GetComponent<UnityEngine.UI.Image>().color=new Color(.43f,.73f,.79f);tv.news.GetComponent<UnityEngine.UI.Image>().color=navy;
            Cartoon(tv);News(tv);
            var canvas=tv.cartoon.GetComponentInParent<Canvas>();
            var transition=Rect("Signal interruption",canvas.transform,new Vector2(800,450),Vector2.zero,white);tv.interruption=transition.gameObject.AddComponent<CanvasGroup>();tv.interruption.alpha=0;tv.interruption.blocksRaycasts=false;
            Label("CVN / EMERGENCY SIGNAL",transition,new Vector2(620,55),Vector2.zero,30,navy);
            var model=GameObject.Find("televisionModern");if(model)Undo.DestroyObjectImmediate(model);
            var dark=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="TV graphite bezel",color=new Color(.025f,.029f,.035f)};dark.SetFloat("_Smoothness",.45f);AssetDatabase.CreateAsset(dark,Folder+"/TVBezel.mat");
            Box("Television thin bezel",tv.transform,new Vector3(-26,1.64f,-35),new Vector3(2.47f,1.42f,.24f),dark);
            Box("Television rear enclosure",tv.transform,new Vector3(-26,1.62f,-35.16f),new Vector3(1.65f,.90f,.14f),dark);
            foreach(float side in new[]{-1f,1f})
            {
                Box("Television foot",tv.transform,new Vector3(-26+side*.71f,.85f,-35.01f),new Vector3(.23f,.05f,.48f),dark);
                var stem=Box("Television stand",tv.transform,new Vector3(-26+side*.68f,.94f,-35.03f),new Vector3(.045f,.20f,.06f),dark);stem.transform.rotation=Quaternion.Euler(0,0,side*17);
            }
            var led=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="TV power light",color=new Color(.4f,1,.65f)};AssetDatabase.CreateAsset(led,Folder+"/TVPower.mat");Box("Television power indicator",tv.transform,new Vector3(-24.87f,.973f,-34.874f),new Vector3(.018f,.009f,.007f),led);
            var rt=new RenderTexture(1280,720,16,RenderTextureFormat.ARGB32){name="Cedar TV 720p",antiAliasing=1,filterMode=FilterMode.Bilinear,useMipMap=false};AssetDatabase.CreateAsset(rt,Folder+"/Television.renderTexture");
            var camera=new GameObject("Television programme camera",typeof(Camera)).GetComponent<Camera>();camera.transform.SetParent(tv.transform);camera.transform.SetPositionAndRotation(new Vector3(-1000,-500,-1008),Quaternion.identity);camera.orthographic=true;camera.orthographicSize=2.25f;camera.aspect=16f/9;camera.nearClipPlane=.1f;camera.farClipPlane=12;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.cullingMask=1<<30;camera.targetTexture=rt;camera.allowHDR=false;camera.allowMSAA=false;camera.depth=-10;
            var additional=camera.GetUniversalAdditionalCameraData();additional.renderPostProcessing=false;additional.renderShadows=false;additional.requiresColorOption=CameraOverrideOption.Off;additional.requiresDepthOption=CameraOverrideOption.Off;
            canvas.transform.SetParent(camera.transform,false);var canvasRect=canvas.GetComponent<RectTransform>();canvasRect.anchoredPosition3D=new Vector3(0,0,8);canvasRect.localRotation=Quaternion.identity;canvasRect.localScale=Vector3.one*.01f;canvas.worldCamera=camera;
            foreach(var t in canvas.GetComponentsInChildren<Transform>(true)){t.gameObject.layer=30;t.gameObject.isStatic=false;}
            game.player.view.cullingMask&=~(1<<30);tv.screenCamera=camera;
            var displayMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Television live display",mainTexture=rt,color=Color.white};AssetDatabase.CreateAsset(displayMaterial,Folder+"/TelevisionDisplay.mat");
            var display=GameObject.CreatePrimitive(PrimitiveType.Quad);display.name="Television live screen";display.transform.SetParent(tv.transform);display.transform.SetPositionAndRotation(new Vector3(-26,1.64f,-34.864f),Quaternion.Euler(0,180,0));display.transform.localScale=new Vector3(2.32f,1.305f,1);display.GetComponent<MeshRenderer>().sharedMaterial=displayMaterial;display.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;Undo.DestroyObjectImmediate(display.GetComponent<Collider>());
            tv.screenGlow.transform.position=new Vector3(-26,1.64f,-34.4f);tv.screenGlow.intensity=.8f;tv.screenGlow.range=3;tv.screenGlow.color=new Color(.55f,.76f,1);tv.SetNews(false);
            // Move interaction text below the physical television while seated.
            var prompt=game.ui.transform.Find("HUD/Interaction");if(prompt){var rect=prompt.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(-250,-170);rect.sizeDelta=new Vector2(500,60);}
            EditorUtility.SetDirty(tv);EditorUtility.SetDirty(game.player.view);
        }
        static Sprite Shape(string name,int kind)
        {
            const int size=128;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="TV "+name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=(x+.5f)/size-.5f,v=(y+.5f)/size-.5f;float distance;
                if(kind==0)distance=Mathf.Sqrt(u*u+v*v)-.48f;
                else if(kind==1){var p=new Vector2(Mathf.Abs(u)-.32f,Mathf.Abs(v)-.32f);distance=new Vector2(Mathf.Max(p.x,0),Mathf.Max(p.y,0)).magnitude+Mathf.Min(Mathf.Max(p.x,p.y),0)-.16f;}
                else distance=Mathf.Max(-v-.47f,Mathf.Abs(u)*.8944f+v*.4472f-.215f);
                pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(-distance*size+.5f));
            }
            texture.SetPixels(pixels);texture.Apply();AssetDatabase.CreateAsset(texture,Folder+"/TV"+name+".asset");var sprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),128);sprite.name="TV "+name;AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;
        }
        static RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 pos,Color color,Sprite sprite=null,float angle=0)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=pos;rt.localRotation=Quaternion.Euler(0,0,angle);var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.sprite=sprite;image.raycastTarget=false;return rt;
        }
        static TMP_Text Label(string text,Transform parent,Vector2 size,Vector2 pos,int fontSize,Color color,TextAlignmentOptions alignment=TextAlignmentOptions.Center)
        {
            var go=new GameObject(text,typeof(RectTransform),typeof(TextMeshProUGUI));var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=pos;var label=go.GetComponent<TMP_Text>();label.font=font;label.text=text;label.fontSize=fontSize;label.color=color;label.alignment=alignment;label.raycastTarget=false;return label;
        }
        static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent);go.transform.position=pos;go.transform.rotation=Quaternion.identity;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;Undo.DestroyObjectImmediate(go.GetComponent<Collider>());return go;
        }
        static RectTransform Cloud(Transform parent,Vector2 pos,float scale)
        {
            var cloud=Rect("Soft drifting cloud",parent,new Vector2(120,30)*scale,pos,white,round);
            Rect("Cloud crown",cloud,new Vector2(55,55)*scale,new Vector2(-20,15)*scale,white,circle);Rect("Cloud crown",cloud,new Vector2(65,60)*scale,new Vector2(20,18)*scale,white,circle);return cloud;
        }
        static void Pine(Transform parent,Vector2 pos,float size,Color color)
        {
            Rect("Cartoon tree trunk",parent,new Vector2(size*.1f,size*.65f),pos+new Vector2(0,-size*.05f),new Color(.30f,.25f,.17f),round);
            for(int i=0;i<3;i++)Rect("Cartoon pine bough",parent,new Vector2(size*(.75f-i*.13f),size*.58f),pos+new Vector2(0,i*size*.18f),color,triangle);
        }
        static void Cartoon(TelevisionScreen tv)
        {
            var p=tv.cartoon.transform;
            Rect("Morning sun",p,new Vector2(100,100),new Vector2(245,115),new Color(1,.89f,.51f),circle);
            for(int i=0;i<5;i++)Rect("Distant painted mountain",p,new Vector2(330,230),new Vector2(-370+i*190,-20+i%2*25),i%2==0?new Color(.28f,.51f,.59f):new Color(.34f,.59f,.65f),triangle);
            Rect("Meadow hill",p,new Vector2(1100,220),new Vector2(-150,-160),new Color(.26f,.48f,.33f),circle);
            Rect("Meadow hill",p,new Vector2(1000,190),new Vector2(240,-175),new Color(.39f,.61f,.35f),circle);
            for(int i=0;i<8;i++)Pine(p,new Vector2(-375+i*108,-66+i%2*17),80+i%3*17,new Color(.15f,.35f,.29f));
            tv.cloud=Cloud(p,new Vector2(-200,140),1);tv.secondCloud=Cloud(p,new Vector2(180,110),.7f);
            Rect("CVN Kids channel",p,new Vector2(122,35),new Vector2(-314,193),white,round);Label("CVN kids",p,new Vector2(114,30),new Vector2(-314,193),20,new Color(.14f,.32f,.4f));
            Label("CLOUD FOX & SUNBIRD",p,new Vector2(470,36),new Vector2(90,193),23,white);
            Rect("Trail",p,new Vector2(590,43),new Vector2(-20,-142),new Color(.79f,.72f,.46f),circle,-6);
            tv.fox=Rect("Animated fox",p,new Vector2(170,150),new Vector2(-155,-48),Color.clear);
            var fox=tv.fox;Color fur=new Color(.89f,.35f,.10f),tip=new Color(1,.89f,.64f),ink=new Color(.10f,.14f,.17f);
            tv.tail=Rect("Fox swishing tail",fox,new Vector2(112,56),new Vector2(-67,-22),fur,round,-15);Rect("Tail cream tip",tv.tail,new Vector2(42,53),new Vector2(-35,0),tip,round);
            Rect("Fox body",fox,new Vector2(80,97),new Vector2(0,-25),fur,round);Rect("Fox chest",fox,new Vector2(45,58),new Vector2(5,-21),tip,circle);
            Rect("Fox hind paw",fox,new Vector2(37,19),new Vector2(-20,-71),ink,round);Rect("Fox front paw",fox,new Vector2(30,19),new Vector2(24,-71),ink,round);
            Rect("Fox left ear",fox,new Vector2(48,72),new Vector2(-34,67),fur,triangle,18);Rect("Fox right ear",fox,new Vector2(48,72),new Vector2(34,67),fur,triangle,-18);
            Rect("Ear inside",fox,new Vector2(25,43),new Vector2(-33,71),new Color(.39f,.18f,.14f),triangle,18);Rect("Ear inside",fox,new Vector2(25,43),new Vector2(33,71),new Color(.39f,.18f,.14f),triangle,-18);
            Rect("Fox face",fox,new Vector2(114,94),new Vector2(1,29),fur,round);Rect("Fox left cheek",fox,new Vector2(68,48),new Vector2(-21,7),tip,circle,-18);Rect("Fox right cheek",fox,new Vector2(68,48),new Vector2(23,7),tip,circle,18);
            foreach(int side in new[]{-1,1}){Rect("Fox eye",fox,new Vector2(12,18),new Vector2(side*25,37),ink,circle);Rect("Fox eye glint",fox,new Vector2(4,5),new Vector2(side*25+2,41),white,circle);}
            Rect("Fox nose",fox,new Vector2(19,13),new Vector2(2,12),ink,round);Rect("Fox smile",fox,new Vector2(18,3),new Vector2(2,-2),ink,round);
            tv.bird=Rect("Animated sunbird",p,new Vector2(95,90),new Vector2(145,40),Color.clear);
            tv.leftWing=Rect("Sunbird left wing",tv.bird,new Vector2(68,28),new Vector2(-36,-2),new Color(.96f,.51f,.10f),circle,15);tv.rightWing=Rect("Sunbird right wing",tv.bird,new Vector2(68,28),new Vector2(36,-2),new Color(.96f,.51f,.10f),circle,-15);
            Rect("Sunbird body",tv.bird,new Vector2(59,68),Vector2.zero,new Color(1,.8f,.19f),circle);Rect("Sunbird belly",tv.bird,new Vector2(41,36),new Vector2(0,-17),new Color(1,.93f,.63f),circle);
            Rect("Sunbird eye",tv.bird,new Vector2(8,11),new Vector2(-13,12),ink,circle);Rect("Sunbird eye",tv.bird,new Vector2(8,11),new Vector2(13,12),ink,circle);Rect("Sunbird beak",tv.bird,new Vector2(21,17),new Vector2(0,-1),new Color(.94f,.36f,.08f),triangle,180);
            for(int i=0;i<18;i++){var color=i%2==0?new Color(1,.80f,.49f):new Color(.83f,.89f,.65f);Rect("Meadow flower",p,new Vector2(7,7),new Vector2(-365+i*43,-162+Mathf.Sin(i*3)*12),color,circle);}
            Rect("Episode lower third",p,new Vector2(800,43),new Vector2(0,-203),new Color(.08f,.20f,.22f,.90f));Label("THE WAY HOME    /    AN ORIGINAL CEDAR VALLEY CARTOON",p,new Vector2(770,35),new Vector2(0,-203),15,white);
        }
        static void News(TelevisionScreen tv)
        {
            var p=tv.news.transform;
            Rect("News top bar",p,new Vector2(800,57),new Vector2(0,196),red);Label("CVN",p,new Vector2(85,45),new Vector2(-345,196),30,white);Label("BREAKING NEWS",p,new Vector2(480,45),new Vector2(-30,196),32,white);
            Rect("LIVE badge",p,new Vector2(76,27),new Vector2(335,196),white,round);Label("LIVE",p,new Vector2(75,26),new Vector2(335,196),19,red);
            Rect("Studio screen",p,new Vector2(454,235),new Vector2(-150,31),new Color(.10f,.23f,.33f),round);
            for(int i=0;i<7;i++)Rect("Studio light column",p,new Vector2(5,180),new Vector2(-360+i*58,30),new Color(.14f,.32f,.43f));
            Rect("Reporter's shoulders",p,new Vector2(235,157),new Vector2(-153,-40),new Color(.05f,.10f,.18f),round);
            Rect("Reporter's blouse",p,new Vector2(70,95),new Vector2(-153,-18),white,triangle,180);
            Rect("Reporter's hair",p,new Vector2(116,147),new Vector2(-153,66),new Color(.17f,.09f,.07f),round);
            Rect("Reporter's neck",p,new Vector2(42,54),new Vector2(-153,6),new Color(.68f,.43f,.27f),round);
            Rect("Reporter's face",p,new Vector2(85,112),new Vector2(-153,62),new Color(.83f,.60f,.40f),round);
            Rect("Reporter's hair sweep",p,new Vector2(95,35),new Vector2(-160,113),new Color(.17f,.09f,.07f),round,12);
            tv.anchorEyes=Rect("Presenter blinking eyes",p,new Vector2(60,16),new Vector2(-153,73),Color.clear);
            foreach(int side in new[]{-1,1}){Rect("Eye",tv.anchorEyes,new Vector2(10,7),new Vector2(side*20,0),navy,circle);Rect("Brow",p,new Vector2(18,3),new Vector2(-153+side*20,86),new Color(.19f,.10f,.08f),round,-side*8);}
            Rect("Presenter nose",p,new Vector2(10,18),new Vector2(-153,58),new Color(.70f,.47f,.30f),round);tv.anchorMouth=Rect("Presenter speaking mouth",p,new Vector2(25,10),new Vector2(-153,35),new Color(.34f,.11f,.10f),round);
            Rect("Studio desk",p,new Vector2(454,31),new Vector2(-150,-77),new Color(.20f,.31f,.39f));Rect("Presenter lower third",p,new Vector2(454,29),new Vector2(-150,-103),new Color(.03f,.12f,.20f));Label("MAYA REED   /   CEDAR VALLEY NEWS",p,new Vector2(440,25),new Vector2(-150,-103),16,white);
            Rect("Dam analysis panel",p,new Vector2(285,263),new Vector2(235,17),new Color(.07f,.15f,.19f),round);Label("CEDAR DAM  /  VALLEY MAP",p,new Vector2(275,29),new Vector2(235,128),15,gold);
            Rect("Reservoir",p,new Vector2(185,48),new Vector2(230,88),new Color(.12f,.46f,.61f),circle);
            for(int i=0;i<3;i++)Rect("Map mountain",p,new Vector2(70,85),new Vector2(143+i*83,55),new Color(.22f,.34f,.31f),triangle);
            Rect("Dam structure",p,new Vector2(174,53),new Vector2(232,42),new Color(.61f,.66f,.62f));
            for(int i=0;i<4;i++)Rect("Dam crack",p,new Vector2(3,27),new Vector2(202+i*12,43-i%2*9),new Color(.24f,.19f,.16f),null,i%2==0?28:-24);
            Rect("Downstream river",p,new Vector2(19,77),new Vector2(232,-20),new Color(.12f,.46f,.61f),round,-15);
            for(int i=0;i<4;i++){Rect("Town building",p,new Vector2(19,15),new Vector2(181+i%2*88,-12-i/2*28),new Color(.85f,.73f,.51f));Rect("Town roof",p,new Vector2(26,14),new Vector2(181+i%2*88,-2-i/2*28),new Color(.54f,.31f,.21f),triangle);}
            Label("FLOOD-RISK AREA",p,new Vector2(270,26),new Vector2(235,-78),16,new Color(1,.54f,.34f));Label("TAKE THE EASTERN RIDGE TRAIL  >",p,new Vector2(276,23),new Vector2(235,-106),12,white);
            Rect("Headline backing",p,new Vector2(800,44),new Vector2(0,-143),white);tv.headline=Label("DAM DAMAGE CONFIRMED",p,new Vector2(770,40),new Vector2(0,-143),27,navy);
            var viewport=Rect("Scrolling alert viewport",p,new Vector2(800,33),new Vector2(0,-181),red);
            tv.ticker=Label("EVACUATE NOW   /   GATHER ESSENTIAL SUPPLIES   /   MOVE TO HIGHER GROUND   /   DO NOT WAIT FOR FURTHER INSTRUCTIONS",viewport,new Vector2(1500,30),new Vector2(600,0),18,white);tv.tickerTrack=tv.ticker.rectTransform;
            ConfigureTicker(tv);
            Label("CEDAR VALLEY NEWS NETWORK",p,new Vector2(410,23),new Vector2(-177,-213),12,new Color(.62f,.74f,.79f));tv.newsClock=Label("DAM WATCH   02:30",p,new Vector2(290,23),new Vector2(238,-213),13,gold);
        }
        static void ConfigureTicker(TelevisionScreen tv)
        {
            const string text="EVACUATE NOW   /   GATHER ESSENTIAL SUPPLIES   /   MOVE TO HIGHER GROUND   /   DO NOT WAIT FOR FURTHER INSTRUCTIONS      ";
            tv.ticker.textWrappingMode=TextWrappingModes.NoWrap;tv.ticker.overflowMode=TextOverflowModes.Overflow;tv.ticker.alignment=TextAlignmentOptions.MidlineLeft;tv.tickerPeriod=tv.ticker.GetPreferredValues(text).x;tv.ticker.text=text+text;tv.ticker.rectTransform.pivot=new Vector2(0,.5f);tv.ticker.rectTransform.sizeDelta=new Vector2(tv.tickerPeriod*2+50,30);
        }
        public static void RepairPresentation()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play mode first");
            var tv=UnityEngine.Object.FindFirstObjectByType<GameManager>().story.television;var canvas=tv.cartoon.GetComponentInParent<Canvas>();canvas.transform.SetParent(tv.screenCamera.transform,false);var rect=canvas.GetComponent<RectTransform>();rect.anchoredPosition3D=new Vector3(0,0,8);rect.localRotation=Quaternion.identity;rect.localScale=Vector3.one*.01f;
            var mask=tv.ticker.transform.parent.GetComponent<UnityEngine.UI.RectMask2D>();if(mask)Undo.DestroyObjectImmediate(mask);ConfigureTicker(tv);EditorUtility.SetDirty(tv);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
    }
}
