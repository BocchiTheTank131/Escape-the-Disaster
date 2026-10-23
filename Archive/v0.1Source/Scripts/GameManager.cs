using UnityEngine;
using UnityEngine.SceneManagement;
namespace FloodSurvival
{
    public enum GameState { Menu, Preparation, Flood, Paused, Victory, Defeat }
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set;}
        public FirstPersonController player;
        public FloodManager flood;
        public NPCManager npcs;
        public ObjectiveSystem objectives;
        public SurvivalUI ui;
        public AtmosphereAudio audioSystem;
        public GameState State {get; private set;}=GameState.Menu;
        public bool InventoryOpen {get; private set;}
        public float Remaining {get; private set;}=150;
        public float FloodElapsed {get; private set;}
        public string AnnouncementTitle {get; private set;}
        public string AnnouncementBody {get; private set;}
        public bool IsPlaying => State==GameState.Preparation || State==GameState.Flood;
        float announcementUntil;
        GameState resumeState;
        bool warned60,warned30,warned10;
        void Awake() { Instance=this; Time.timeScale=1; Application.runInBackground=true; Application.targetFrameRate=90; }
        void Start() { SetCursor(false); ui.RefreshState(); }
        void Update()
        {
            if(!IsPlaying) return;
            if(Time.time>announcementUntil) AnnouncementTitle="";
            objectives.Tick(player.transform.position);
            if(State==GameState.Preparation)
            {
                Remaining=Mathf.Max(0,Remaining-Time.deltaTime);
                if(Remaining<=60 && !warned60) { warned60=true; Announce("ONE MINUTE", "Dam integrity critical. Return home now. Locate the ridge trail.",6); }
                if(Remaining<=30 && !warned30) { warned30=true; Announce("30 SECONDS", "Abandon low ground. Prepare to move toward Ridge Shelter.",5); }
                if(Remaining<=10 && !warned10) { warned10=true; Announce("BREACH IMMINENT", "Get ready. The river is about to rise.",5); }
                if(Remaining<=0) Breach();
            }
            else
            {
                FloodElapsed+=Time.deltaTime; flood.Tick(Time.deltaTime); player.WaterTick(flood.Level,Time.deltaTime);
                if(State==GameState.Flood && FloodElapsed>=75 && Vector3.Distance(player.transform.position,objectives.shelter.position)<9 && player.transform.position.y>flood.FinalLevel+1) Finish(true);
            }
            audioSystem.Tick(State==GameState.Flood,Remaining);
        }
        public void Begin()
        {
            if(State!=GameState.Menu) return;
            State=GameState.Preparation; Remaining=150; SetCursor(true); npcs.Activate(); audioSystem.StartWarning();
            Announce("EMERGENCY BROADCAST", "Cedar Dam is failing. You have 150 seconds. Gather supplies and return home. High ground is your only refuge.",11); ui.RefreshState();
        }
        public void Breach()
        {
            if(State!=GameState.Preparation) return;
            State=GameState.Flood; Remaining=0; objectives.OnBreach(); flood.Begin(); player.Shake(3); audioSystem.Collapse();
            Announce("THE DAM HAS COLLAPSED", "Run to Ridge Shelter. Follow the amber trail markers. Do not remain in your house.",10); ui.RefreshState();
        }
        public void Escape()
        {
            if(InventoryOpen) { ToggleInventory(); return; }
            if(State==GameState.Paused) Resume();
            else if(IsPlaying) { resumeState=State; State=GameState.Paused; Time.timeScale=0; SetCursor(false); ui.RefreshState(); }
        }
        public void Resume() { if(State!=GameState.Paused) return; State=resumeState; Time.timeScale=1; SetCursor(!InventoryOpen); ui.RefreshState(); }
        public void ToggleInventory() { if(!IsPlaying) return; InventoryOpen=!InventoryOpen; SetCursor(!InventoryOpen); ui.RefreshState(); }
        public void Finish(bool survived)
        {
            if(!IsPlaying) return;
            InventoryOpen=false; State=survived?GameState.Victory:GameState.Defeat; Time.timeScale=0; audioSystem.Silence(); SetCursor(false); ui.RefreshState();
        }
        public void Restart() { Time.timeScale=1; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        public void Quit() { Time.timeScale=1; Application.Quit(); }
        public void Announce(string title,string body,float seconds) { AnnouncementTitle=title; AnnouncementBody=body; announcementUntil=Time.time+seconds; }
        void SetCursor(bool locked) { Cursor.lockState=locked?CursorLockMode.Locked:CursorLockMode.None; Cursor.visible=!locked; }
        void OnDestroy() { Time.timeScale=1; if(Instance==this) Instance=null; Cursor.lockState=CursorLockMode.None; Cursor.visible=true; }
    }
}
