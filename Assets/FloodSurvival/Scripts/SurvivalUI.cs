using UnityEngine;
using TMPro;
namespace FloodSurvival
{
    public sealed class SurvivalUI : MonoBehaviour
    {
        public TMP_FontAsset font;
        [SerializeField] GameObject hud,menu,pause,bag,end,broadcast;
        [SerializeField] TMP_Text timer,objective,health,supplies,prompt,marker,announcementTitle,announcementBody,endTitle,endBody,bagBody,floodStatus,phaseLabel,clockLabel;
        [SerializeField] UnityEngine.UI.Image healthBar,oxygenBar,waterTint;
        [SerializeField] RectTransform waypoint;
        [SerializeField] UnityEngine.UI.Button startButton,resumeButton,restartButton,endRestart,quitButton,endQuit,healButton,flashButton,closeBag;
        [SerializeField] UnityEngine.UI.Slider sensitivity,volume;
        public ItemDefinition[] definitions;
        [SerializeField] TMP_Text subtitleText,capacityText;
        [SerializeField] GameObject subtitlePanel;
        [SerializeField] UnityEngine.UI.Button skipButton,subtitleButton;
        [SerializeField] TMP_Text[] inventoryRows;
        [SerializeField] UnityEngine.UI.Button[] itemButtons;
        bool subtitlesEnabled=true;
        float nextRefresh;
        static readonly Color Ink=new Color(.035f,.065f,.08f,.94f), Muted=new Color(.64f,.73f,.73f), Amber=new Color(1,.69f,.27f), White=new Color(.93f,.96f,.92f);
        void Start()
        {
            var game=GameManager.Instance;
            startButton.onClick.AddListener(game.Begin); resumeButton.onClick.AddListener(game.Resume);
            restartButton.onClick.AddListener(game.Restart); endRestart.onClick.AddListener(game.Restart);
            quitButton.onClick.AddListener(game.Quit); endQuit.onClick.AddListener(game.Quit);
            healButton.onClick.AddListener(game.player.UseMedicine); flashButton.onClick.AddListener(game.player.ToggleFlashlight); closeBag.onClick.AddListener(game.ToggleInventory);
            sensitivity.SetValueWithoutNotify(game.player.sensitivity); sensitivity.onValueChanged.AddListener(game.player.SetSensitivity);
            volume.SetValueWithoutNotify(AudioListener.volume); volume.onValueChanged.AddListener(game.audioSystem.SetVolume);
            subtitlesEnabled=PlayerPrefs.GetInt("SubtitlesV02",1)>0;
            if(skipButton)skipButton.onClick.AddListener(game.BeginSkipped);
            if(subtitleButton)subtitleButton.onClick.AddListener(()=>{subtitlesEnabled=!subtitlesEnabled;PlayerPrefs.SetInt("SubtitlesV02",subtitlesEnabled?1:0);});
            if(itemButtons!=null)for(int i=0;i<itemButtons.Length;i++){int kind=i;itemButtons[i].onClick.AddListener(()=>game.player.UseItem((ItemKind)kind));}
            RefreshState();
        }
        public void RefreshState()
        {
            var game=GameManager.Instance; if(!game)return;
            hud.SetActive(game.IsPlaying); menu.SetActive(game.State==GameState.Menu); pause.SetActive(game.State==GameState.Paused);
            bag.SetActive(game.InventoryOpen); end.SetActive(game.State==GameState.Victory || game.State==GameState.Defeat);
            if(skipButton)skipButton.gameObject.SetActive(OpeningSequence.CanSkip);
            if(end.activeSelf)
            {
                bool won=game.State==GameState.Victory;
                endTitle.text=won?"ABOVE THE FLOOD":"LOST TO THE WATER";
                endTitle.color=won?Amber:new Color(1,.4f,.3f);
                endBody.text=won?"You reached Ridge Shelter and outlasted the surge.\n\n"+(game.player.inventory.Prepared?"Food, water, first aid and light: all secured.":"You survived with the supplies you could carry.")+"\n"+$"Emergency bag: {game.player.inventory.Used}/{game.player.inventory.Capacity} slots. House supplies: {game.objectives.HouseSupplies}.\n\nDAY 1 / THE FIRST NIGHT LIES AHEAD":"Your air ran out beneath the rising water.\n\nFollow the amber trail east of the house.\nHold Space to swim up; avoid the fast current.\n\nSupplies help, but high ground saves your life.";
            }
            nextRefresh=0;
        }
        void Update()
        {
            if(Time.unscaledTime<nextRefresh)return; nextRefresh=Time.unscaledTime+.08f;
            var game=GameManager.Instance; if(!game)return;
            var player=game.player; var inv=player.inventory;
            int seconds=Mathf.CeilToInt(game.Remaining);
            timer.text=game.State==GameState.Flood?"BREACHED":$"{seconds/60:00}:{seconds%60:00}";
            timer.fontSize=game.flood.IsFlooding?30:44;
            if(!game.EmergencyStarted)timer.text="--:--";
            phaseLabel.text=game.flood.IsFlooding?"03   /   THE SURGE":game.EmergencyStarted?"02   /   EVACUATE":"01   /   A NORMAL DAY";
            clockLabel.text=game.flood.IsFlooding?"SURGE IN PROGRESS":game.EmergencyStarted?"TIME UNTIL DAM COLLAPSE":"CEDAR VALLEY / LIVE";
            timer.color=game.Remaining<30?new Color(1,.4f,.28f):Amber;
            objective.text=game.objectives.Text;
            health.text=$"HEALTH  {Mathf.CeilToInt(player.Health)}     AIR  {Mathf.CeilToInt(player.Oxygen)}";
            healthBar.rectTransform.localScale=new Vector3(player.Health/100,1,1); oxygenBar.rectTransform.localScale=new Vector3(player.Oxygen/100,1,1);
            supplies.text=$"BAG {inv.Used}/{inv.Capacity}    FOOD {inv.Count(ItemKind.Food)}    WATER {inv.Count(ItemKind.Water)}    AID {inv.Count(ItemKind.Medicine)}    LIGHT {player.BatteryCharge:0}%";
            prompt.text=game.story && game.story.Seated?(game.EmergencyStarted?"[ E ]  GET UP FROM COUCH":"MOUSE  LOOK AROUND"+(OpeningSequence.CanSkip?"     SPACE  SKIP INTRO":"")):player.FocusedItem?"[ E ] TAKE "+player.FocusedItem.definition.displayName.ToUpperInvariant()+"   [ I ] INSPECT":player.FocusedObject?"[ I ] INSPECT "+player.FocusedObject.title:player.Swimming?"HOLD SPACE TO SWIM UP  /  CURRENT PUSHES DOWNSTREAM":"";
            floodStatus.text=game.State==GameState.Flood?$"WATER  {game.flood.Level:0.0} m  /  {game.flood.FinalLevel:0.0} m\n"+(game.FloodElapsed<game.flood.RiseDuration?$"SURGE STABILIZES IN {Mathf.CeilToInt(game.flood.RiseDuration-game.FloodElapsed)} s":"SURGE STABLE / REACH SHELTER"):"CEDAR VALLEY   /   EMERGENCY CHANNEL 04";
            broadcast.SetActive(game.IsPlaying && !string.IsNullOrEmpty(game.AnnouncementTitle)); announcementTitle.text=game.AnnouncementTitle; announcementBody.text=game.AnnouncementBody;
            bagBody.text="Food +12 health. Water +8 health on dry ground.\nFirst aid +40 health. Batteries recharge the light.\nAll supplies are optional. The countdown continues.";
            if(capacityText)capacityText.text=$"CAPACITY {inv.Used} / {inv.Capacity}   |   FLASHLIGHT {player.BatteryCharge:0}%";
            if(inventoryRows!=null)for(int i=0;i<inventoryRows.Length;i++)
            {
                var kind=(ItemKind)i;var definition=definitions!=null && i<definitions.Length?definitions[i]:null;
                inventoryRows[i].text=(definition?definition.displayName:kind.ToString())+"   x"+inv.Count(kind);
                itemButtons[i].interactable=inv.Count(kind)>0 && kind!=ItemKind.Backpack;
            }
            if(subtitlePanel){subtitlePanel.SetActive(game.IsPlaying && subtitlesEnabled && game.story && !string.IsNullOrEmpty(game.story.Subtitle));if(game.story)subtitleText.text=game.story.Subtitle;}
            if(subtitleButton)subtitleButton.GetComponentInChildren<TMP_Text>().text="SUBTITLES  "+(subtitlesEnabled?"ON":"OFF");
            healButton.interactable=inv.Count(ItemKind.Medicine)>0 && player.Health<100; flashButton.interactable=inv.Count(ItemKind.Flashlight)>0;
            bool underwater=game.flood.GetLevel(player.transform.position)>player.view.transform.position.y;
            waterTint.color=new Color(.02f,.22f,.27f,underwater?.58f:0);
            if(game.IsPlaying)
            {
                waypoint.gameObject.SetActive(!game.story || !game.story.Seated);
                Vector3 target=game.objectives.Target.position+Vector3.up*3;
                Vector3 point=player.view.WorldToViewportPoint(target);
                if(point.z<0) { point.x=1-point.x; point.y=.5f; }
                float x=Mathf.Clamp(point.x,.1f,.9f),y=Mathf.Clamp(point.y,.25f,.72f);
                if(x>.7f && y>.45f) y=.43f; // Keep the marker clear of the control legend.
                waypoint.anchorMin=waypoint.anchorMax=new Vector2(x,y); waypoint.anchoredPosition=Vector2.zero;
                marker.text=$"[ + ]\n{game.objectives.TargetName}\n{Vector3.Distance(player.transform.position,game.objectives.Target.position):0} m";
            }
        }
        RectTransform Rect(string title,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
        {
            var go=new GameObject(title,typeof(RectTransform)); var rt=go.GetComponent<RectTransform>(); rt.SetParent(parent,false); rt.anchorMin=rt.anchorMax=anchor; rt.pivot=new Vector2(0,1); rt.anchoredPosition=position; rt.sizeDelta=size; return rt;
        }
        GameObject Panel(string title,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size,Color color)
        {
            var rt=Rect(title,parent,anchor,pos,size); var image=rt.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color=color; image.raycastTarget=false; return rt.gameObject;
        }
        TMP_Text Label(string title,Transform parent,string text,int size,Vector2 pos,Vector2 bounds,Color color)
        {
            var rt=Rect(title,parent,Vector2.up,pos,bounds); var label=rt.gameObject.AddComponent<TextMeshProUGUI>(); label.font=font; label.text=text; label.fontSize=size; label.color=color; label.raycastTarget=false; label.textWrappingMode=TextWrappingModes.Normal; return label;
        }
        UnityEngine.UI.Button Button(string name,Transform parent,Vector2 pos,string caption,bool accent=false)
        {
            var panel=Panel(name,parent,Vector2.up,pos,new Vector2(370,49),accent?Amber:new Color(.13f,.2f,.22f)); panel.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var button=panel.AddComponent<UnityEngine.UI.Button>(); var colors=button.colors; colors.highlightedColor=new Color(.8f,.88f,.88f); colors.pressedColor=new Color(.55f,.65f,.65f); button.colors=colors;
            var label=Label(name+"Label",panel.transform,caption,19,new Vector2(18,-12),new Vector2(336,32),accent?Ink:White); label.fontStyle=FontStyles.Bold; return button;
        }
        UnityEngine.UI.Slider Slider(Transform parent,string caption,Vector2 pos,float min,float max)
        {
            Label(caption,parent,caption,15,pos,new Vector2(370,30),Muted);
            var bg=Panel(caption+"Slider",parent,Vector2.up,pos+new Vector2(0,-34),new Vector2(370,14),new Color(.18f,.26f,.28f)); bg.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var slider=bg.AddComponent<UnityEngine.UI.Slider>(); slider.minValue=min; slider.maxValue=max;
            var handle=Panel("Handle",bg.transform,Vector2.zero,new Vector2(0,7),new Vector2(18,26),Amber); handle.GetComponent<RectTransform>().pivot=new Vector2(.5f,.5f); handle.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            slider.handleRect=handle.GetComponent<RectTransform>(); slider.targetGraphic=handle.GetComponent<UnityEngine.UI.Image>(); return slider;
        }
        GameObject FullPanel(string name,Color color)
        {
            var go=Panel(name,transform,Vector2.up,Vector2.zero,Vector2.zero,color); var rt=go.GetComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero; return go;
        }
        public void Build(TMP_FontAsset selectedFont)
        {
            font=selectedFont;
            waterTint=FullPanel("Underwater",Color.clear).GetComponent<UnityEngine.UI.Image>();
            hud=FullPanel("HUD",Color.clear);
            var top=Panel("ObjectiveCard",hud.transform,Vector2.up,new Vector2(28,-26),new Vector2(540,107),Ink);
            phaseLabel=Label("Status",top.transform,"01   /   THE WARNING",14,new Vector2(18,-13),new Vector2(500,22),Amber);
            objective=Label("Objective",top.transform,"",21,new Vector2(18,-41),new Vector2(504,62),White);
            var clock=Panel("Countdown",hud.transform,Vector2.one,new Vector2(-260,-26),new Vector2(232,107),Ink);
            clockLabel=Label("ClockLabel",clock.transform,"TIME UNTIL DAM COLLAPSE",12,new Vector2(15,-12),new Vector2(212,22),Muted);
            timer=Label("Clock",clock.transform,"02:30",44,new Vector2(15,-36),new Vector2(210,65),Amber);
            timer.textWrappingMode=TextWrappingModes.NoWrap;
            var bottom=Panel("Vitals",hud.transform,Vector2.zero,new Vector2(28,111),new Vector2(430,86),Ink);
            health=Label("Health",bottom.transform,"",14,new Vector2(15,-10),new Vector2(400,24),White);
            healthBar=Panel("HealthBar",bottom.transform,Vector2.up,new Vector2(15,-39),new Vector2(195,5),Amber).GetComponent<UnityEngine.UI.Image>(); healthBar.type=UnityEngine.UI.Image.Type.Filled; healthBar.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;
            oxygenBar=Panel("OxygenBar",bottom.transform,Vector2.up,new Vector2(220,-39),new Vector2(195,5),new Color(.3f,.8f,.88f)).GetComponent<UnityEngine.UI.Image>(); oxygenBar.type=UnityEngine.UI.Image.Type.Filled; oxygenBar.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;
            supplies=Label("Supplies",bottom.transform,"",12,new Vector2(15,-57),new Vector2(410,22),Muted);
            var controls=Rect("Controls",hud.transform,Vector2.one,new Vector2(-350,-150),new Vector2(320,200));
            Label("Keys",controls,"WASD   MOVE\nSHIFT   SPRINT\nSPACE   JUMP\nE   TAKE SUPPLY\nTAB   BAG     F   LIGHT\nH   MEDICINE     ESC   PAUSE",13,Vector2.zero,new Vector2(320,180),Muted).alignment=TextAlignmentOptions.TopRight;
            var status=Rect("FloodStatus",hud.transform,Vector2.right,new Vector2(-400,98),new Vector2(372,74)); floodStatus=Label("FloodReadout",status,"",13,Vector2.zero,new Vector2(372,74),Amber); floodStatus.alignment=TextAlignmentOptions.TopRight;
            var cross=Rect("Crosshair",hud.transform,new Vector2(.5f,.5f),new Vector2(-8,12),new Vector2(16,24)); Label("Cross",cross,"+",20,Vector2.zero,new Vector2(24,28),White);
            var interact=Rect("Interaction",hud.transform,new Vector2(.5f,.5f),new Vector2(-250,-46),new Vector2(500,36)); prompt=Label("Prompt",interact,"",20,Vector2.zero,new Vector2(500,36),Amber); prompt.alignment=TextAlignmentOptions.Center;
            waypoint=Rect("Waypoint",hud.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(180,83)); waypoint.pivot=new Vector2(.5f,.5f); marker=Label("Marker",waypoint,"",15,Vector2.zero,new Vector2(180,90),Amber); marker.alignment=TextAlignmentOptions.Center;
            broadcast=Panel("Broadcast",hud.transform,new Vector2(.5f,1),new Vector2(-360,-158),new Vector2(720,112),Ink);
            announcementTitle=Label("BroadcastTitle",broadcast.transform,"",17,new Vector2(20,-13),new Vector2(680,28),Amber);
            announcementBody=Label("BroadcastBody",broadcast.transform,"",17,new Vector2(20,-44),new Vector2(680,62),White);
            menu=FullPanel("MainMenu",new Color(.015f,.035f,.045f,.5f));
            var card=Panel("MenuCard",menu.transform,Vector2.up,new Vector2(64,-62),new Vector2(580,596),Ink);
            Label("Edition",card.transform,"CEDAR VALLEY   /   PROTOTYPE 0.1",15,new Vector2(32,-29),new Vector2(510,30),Amber);
            Label("Title",card.transform,"FLOOD\nSURVIVAL",65,new Vector2(28,-75),new Vector2(530,166),White).fontStyle=FontStyles.Bold;
            Label("Subtitle",card.transform,"5 DAYS",30,new Vector2(32,-249),new Vector2(490,45),Amber);
            Label("Intro",card.transform,"A dam above your town is failing.\n150 seconds to gather what matters.\nReturn home. Then escape to high ground.",20,new Vector2(32,-310),new Vector2(500,100),White);
            startButton=Button("Start",card.transform,new Vector2(32,-432),"BEGIN EMERGENCY   →",true);
            quitButton=Button("Quit",card.transform,new Vector2(32,-494),"QUIT TO DESKTOP");
            Label("Footer",menu.transform,"SINGLE PLAYER  /  DAY 1: THE BREACH                                      BUILD 0.1",13,new Vector2(65,-685),new Vector2(1100,26),White);
            pause=FullPanel("PauseMenu",new Color(.015f,.035f,.045f,.86f));
            var pc=Panel("PauseCard",pause.transform,new Vector2(.5f,1),new Vector2(-220,-68),new Vector2(440,585),Ink);
            Label("PauseTitle",pc.transform,"TAKE A BREATH",30,new Vector2(32,-28),new Vector2(390,55),White);
            Label("PauseSubtitle",pc.transform,"TIME IS PAUSED",14,new Vector2(32,-83),new Vector2(390,27),Amber);
            resumeButton=Button("Resume",pc.transform,new Vector2(32,-130),"RESUME",true);
            restartButton=Button("Restart",pc.transform,new Vector2(32,-191),"RESTART EMERGENCY");
            sensitivity=Slider(pc.transform,"MOUSE SENSITIVITY",new Vector2(32,-280),.035f,.3f);
            volume=Slider(pc.transform,"MASTER VOLUME",new Vector2(32,-366),0,1);
            Label("PauseControls",pc.transform,"WASD move / Mouse look / Shift sprint\nSpace jump / E take / Tab bag\nF flashlight / H medicine / Esc resume",15,new Vector2(32,-460),new Vector2(380,90),Muted);
            bag=FullPanel("Inventory",new Color(.015f,.035f,.045f,.78f));
            var bc=Panel("BagCard",bag.transform,new Vector2(.5f,1),new Vector2(-265,-64),new Vector2(530,592),Ink);
            Label("BagTitle",bc.transform,"EMERGENCY BAG",30,new Vector2(30,-27),new Vector2(460,50),Amber);
            bagBody=Label("BagItems",bc.transform,"",21,new Vector2(30,-100),new Vector2(470,302),White);
            healButton=Button("UseMedicine",bc.transform,new Vector2(30,-399),"USE MEDICINE  [ H ]");
            flashButton=Button("ToggleLight",bc.transform,new Vector2(30,-460),"TOGGLE FLASHLIGHT  [ F ]");
            closeBag=Button("CloseBag",bc.transform,new Vector2(30,-521),"CLOSE BAG  [ TAB ]",true);
            end=FullPanel("Outcome",new Color(.015f,.035f,.045f,.91f));
            var ec=Panel("OutcomeCard",end.transform,new Vector2(.5f,1),new Vector2(-305,-106),new Vector2(610,510),Ink);
            endTitle=Label("OutcomeTitle",ec.transform,"",36,new Vector2(30,-30),new Vector2(550,65),Amber);
            endBody=Label("OutcomeBody",ec.transform,"",20,new Vector2(30,-112),new Vector2(550,230),White);
            endRestart=Button("TryAgain",ec.transform,new Vector2(30,-363),"PLAY AGAIN",true); endQuit=Button("EndQuit",ec.transform,new Vector2(30,-427),"QUIT TO DESKTOP");
            pause.SetActive(false); bag.SetActive(false); end.SetActive(false); broadcast.SetActive(false); hud.SetActive(false);
        }
        public void UpgradeV02(ItemDefinition[] items)
        {
            definitions=items;
            var menuCard=menu.transform.Find("MenuCard");
            foreach(var label in menuCard.GetComponentsInChildren<TMP_Text>(true))
            {
                if(label.name=="Edition")label.text="CEDAR VALLEY   /   PROTOTYPE 0.2";
                if(label.name=="Intro"){label.text="An ordinary afternoon. A sudden warning.\nGather what matters. Leave the valley.\nYour first day starts on the couch.";label.rectTransform.anchoredPosition=new Vector2(32,-306);label.fontSize=19;}
            }
            var footer=menu.transform.Find("Footer").GetComponent<TMP_Text>();footer.text="STORY & EVACUATION   /   DAY 1: THE BREACH                                      BUILD 0.2";
            startButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(32,-402);startButton.GetComponentInChildren<TMP_Text>().text="START STORY   >";
            quitButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(32,-524);
            if(!skipButton)skipButton=Button("SkipOpening",menuCard,new Vector2(32,-463),"SKIP OPENING   >");
            var pc=pause.transform.Find("PauseCard");
            if(!subtitleButton)subtitleButton=Button("SubtitleToggle",pc,new Vector2(32,-434),"SUBTITLES ON");
            var pauseKeys=pc.Find("PauseControls").GetComponent<TMP_Text>();pauseKeys.rectTransform.anchoredPosition=new Vector2(32,-501);pauseKeys.fontSize=13;pauseKeys.text="WASD / Mouse / Shift / Space\nE take / I inspect / Tab bag / F light\nH first aid / R batteries / V radio / Esc pause";
            var bc=bag.transform.Find("BagCard");
            healButton.gameObject.SetActive(false);flashButton.gameObject.SetActive(false);
            bagBody.rectTransform.anchoredPosition=new Vector2(30,-418);bagBody.rectTransform.sizeDelta=new Vector2(470,81);bagBody.fontSize=14;
            capacityText=Label("Capacity",bc,"",15,new Vector2(30,-77),new Vector2(470,28),Muted);
            inventoryRows=new TMP_Text[items.Length];itemButtons=new UnityEngine.UI.Button[items.Length];
            for(int i=0;i<items.Length;i++)
            {
                float y=-114-i*42;
                var row=Panel("SupplyRow"+i,bc,Vector2.up,new Vector2(30,y),new Vector2(470,38),new Color(.1f,.16f,.18f));
                var icon=Panel("Icon",row.transform,Vector2.up,new Vector2(4,-3),new Vector2(32,32),Color.white).GetComponent<UnityEngine.UI.Image>();icon.sprite=items[i].icon;icon.preserveAspect=true;
                inventoryRows[i]=Label("ItemName",row.transform,"",17,new Vector2(46,-8),new Vector2(310,28),White);
                var btn=Button("Use"+i,row.transform,new Vector2(370,0),i==5?"WORN":i==6?"TUNE":i==3?"LIGHT":i==4?"LOAD":"USE");btn.GetComponent<RectTransform>().sizeDelta=new Vector2(100,38);var text=btn.GetComponentInChildren<TMP_Text>();text.fontSize=13;text.rectTransform.sizeDelta=new Vector2(85,28);text.rectTransform.anchoredPosition=new Vector2(12,-9);itemButtons[i]=btn;
            }
            var interaction=prompt.rectTransform;interaction.sizeDelta=new Vector2(690,66);interaction.anchoredPosition=new Vector2(-95,0);prompt.fontSize=17;
            var keys=transform.Find("HUD/Controls/Keys").GetComponent<TMP_Text>();keys.text="WASD   MOVE\nSHIFT   SPRINT\nSPACE   JUMP / SWIM UP\nE   TAKE     I   INSPECT\nTAB   BAG     F   LIGHT\nH   AID   R   BATTERIES\nV   RADIO     ESC   PAUSE";keys.fontSize=12;
            subtitlePanel=Panel("NewsSubtitles",transform,new Vector2(.5f,0),new Vector2(-440,244),new Vector2(880,104),Ink);
            Label("SubtitleSource",subtitlePanel.transform,"CEDAR VALLEY NEWS / EMERGENCY BULLETIN",12,new Vector2(18,-9),new Vector2(844,22),Amber);
            subtitleText=Label("SubtitleText",subtitlePanel.transform,"",17,new Vector2(18,-34),new Vector2(844,68),White);
            subtitlePanel.SetActive(false);
        }
    }
}
