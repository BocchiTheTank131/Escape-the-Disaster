using UnityEngine;
namespace FloodSurvival
{
    public sealed class OpeningSequence : MonoBehaviour
    {
        public TelevisionScreen television;
        public Transform seat,standing;
        public Bounds HouseBounds=new Bounds(new Vector3(-22,2,-30),new Vector3(13.5f,6,12));
        [Range(10,15)] public float peacefulDuration=12;
        public ParticleSystem startledBirds,damDust,leaks;
        public GameObject damCracks;
        public Light warningLamp;
        public Light daylight;
        public AudioClip[] newsVoices;
        public bool Seated {get; private set;}=true;
        public bool OutsideTriggered {get; private set;}
        public float Elapsed {get; private set;}
        public float NewsElapsed {get; private set;}
        public string Subtitle {get; private set;}="";
        public static bool CanSkip => PlayerPrefs.GetInt("OpeningSeenV02",0)>0;
        bool begun;
        int voicedSection=-1;
        public int NewsSection => voicedSection;
        float initialSun;
        Color initialFog;
        static readonly string[] News={
            "Breaking news! Emergency officials have confirmed severe structural damage to the nearby dam.",
            "Large cracks have appeared across the dam's main structure, and engineers warn that a catastrophic collapse may occur at any moment.",
            "All residents living downstream are ordered to evacuate immediately. Do not wait for further instructions.",
            "Gather essential supplies and move to higher ground!"
        };
        public void Begin()
        {
            if(begun)return; begun=true; Seated=true; Elapsed=0;
            initialSun=daylight?daylight.intensity:2.2f;initialFog=RenderSettings.fogColor;
            GameManager.Instance.player.Sit(seat.position,seat.eulerAngles.y);
            television.SetNews(false);
            GameManager.Instance.audioSystem.Television(true);
        }
        public void Tick(float dt)
        {
            if(!begun)return;
            var game=GameManager.Instance;
            if(!game.EmergencyStarted)
            {
                Elapsed+=dt;
                if(Elapsed>=peacefulDuration)
                {
                    game.StartWarning(); television.SetNews(true); game.audioSystem.BreakingNews();
                }
            }
            if(game.EmergencyStarted)
            {
                NewsElapsed+=dt;
                float tension=Mathf.Clamp01(NewsElapsed/40);
                if(daylight)daylight.intensity=Mathf.Lerp(initialSun,initialSun*.78f,tension);
                RenderSettings.fogColor=Color.Lerp(initialFog,new Color(.36f,.46f,.49f),tension);RenderSettings.fogDensity=Mathf.Lerp(.0026f,.004f,tension);
                int section=-1;float cursor=1.2f;
                for(int i=0;i<News.Length;i++)
                {
                    float duration=newsVoices!=null && i<newsVoices.Length && newsVoices[i]?newsVoices[i].length:.0f;
                    if(duration<=0)duration=i==1?10:7;
                    if(NewsElapsed<cursor+duration+.35f)
                    {
                        section=i;
                        if(NewsElapsed>=cursor && voicedSection!=i){voicedSection=i;if(newsVoices!=null && i<newsVoices.Length)game.audioSystem.PlayNews(newsVoices[i]);}
                        break;
                    }
                    cursor+=duration+.35f;
                }
                Subtitle=section>=0?News[section]:"";
                if(warningLamp)warningLamp.intensity=(Mathf.Sin(Time.time*7)>0?5:.3f);
                if(!Seated && !HouseBounds.Contains(game.player.transform.position) && !OutsideTriggered) TriggerOutside();
            }
        }
        public void StandUp()
        {
            var game=GameManager.Instance;
            if(!begun || !Seated || !game.EmergencyStarted)return;
            Seated=false; game.player.Stand(standing.position); PlayerPrefs.SetInt("OpeningSeenV02",1);
            game.Announce("TAKE ONLY WHAT MATTERS", "Water, food, first aid and light. The backpack adds space. The ridge trail is east of the house.",6);
        }
        public void Skip()
        {
            if(!CanSkip || !begun)return;
            Elapsed=peacefulDuration; var game=GameManager.Instance;
            if(!game.EmergencyStarted){game.StartWarning();television.SetNews(true);game.audioSystem.BreakingNews();}
            NewsElapsed=1000; voicedSection=4; Subtitle="";game.audioSystem.Television(false); StandUp();
        }
        public void TriggerOutside()
        {
            if(OutsideTriggered || GameManager.Instance.State!=GameState.Preparation || !GameManager.Instance.EmergencyStarted)return;
            OutsideTriggered=true;
            if(damCracks)damCracks.SetActive(true);
            if(startledBirds)startledBirds.Play(); if(damDust)damDust.Play(); if(leaks)leaks.Play();
            var game=GameManager.Instance; game.player.Shake(1.3f); game.audioSystem.Crack(); game.npcs.Escalate();
            game.Announce("DAM COLLAPSE IMMINENT. EVACUATE NOW!", "That was the dam cracking. It has not collapsed yet. Keep moving toward the amber trail.",8);
        }
    }
}
