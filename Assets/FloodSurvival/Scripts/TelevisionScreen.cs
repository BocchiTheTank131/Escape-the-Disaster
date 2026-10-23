using UnityEngine;
using TMPro;
namespace FloodSurvival
{
    public sealed class TelevisionScreen : MonoBehaviour
    {
        public GameObject cartoon,news;
        public RectTransform fox,bird,cloud;
        public TMP_Text ticker;
        public Light screenGlow;
        public AudioSource speaker;
        public Camera screenCamera;
        public RectTransform tail,leftWing,rightWing,anchorMouth,anchorEyes,tickerTrack,secondCloud;
        public TMP_Text headline,newsClock;
        public CanvasGroup interruption;
        public float tickerPeriod=1400;
        public bool IsNews {get; private set;}
        float refresh;
        float transition;
        static readonly string[] Headlines={"DAM DAMAGE CONFIRMED","ENGINEERS WARN OF COLLAPSE","DOWNSTREAM EVACUATION ORDER","MOVE TO HIGHER GROUND NOW"};
        public void SetNews(bool value)
        {
            if(value && !IsNews)transition=.42f;
            IsNews=value; if(cartoon)cartoon.SetActive(!value); if(news)news.SetActive(value);
            if(screenGlow)screenGlow.color=value?new Color(.72f,.83f,1):new Color(.55f,.76f,1);
        }
        void Update()
        {
            if(Time.time<refresh)return; refresh=Time.time+1f/24;
            var game=GameManager.Instance;
            if(screenCamera && game && game.player)screenCamera.enabled=(game.player.transform.position-transform.position).sqrMagnitude<24*24;
            if(interruption){transition=Mathf.Max(0,transition-1f/24);interruption.alpha=transition>0?(.35f+.55f*Mathf.Abs(Mathf.Sin(Time.time*97))):0;}
            if(!IsNews)
            {
                if(fox)fox.anchoredPosition=new Vector2(-155+Mathf.Sin(Time.time*.7f)*22,-48+Mathf.Abs(Mathf.Sin(Time.time*2))*5);
                if(tail)tail.localRotation=Quaternion.Euler(0,0,-15+Mathf.Sin(Time.time*2)*12);
                if(bird)bird.anchoredPosition=new Vector2(145+Mathf.Sin(Time.time*.9f)*45,40+Mathf.Sin(Time.time*2)*14);
                if(leftWing)leftWing.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*9)*30);
                if(rightWing)rightWing.localRotation=Quaternion.Euler(0,0,-Mathf.Sin(Time.time*9)*30);
                if(cloud)cloud.anchoredPosition=new Vector2(Mathf.Repeat(Time.time*5+180,1000)-500,140);
                if(secondCloud)secondCloud.anchoredPosition=new Vector2(Mathf.Repeat(Time.time*3+690,1000)-500,112);
            }
            else
            {
                if(tickerTrack)tickerTrack.anchoredPosition=new Vector2(-400-Mathf.Repeat(Time.time*55,Mathf.Max(1,tickerPeriod)),0);
                if(anchorMouth)anchorMouth.localScale=new Vector3(1,speaker && speaker.isPlaying?.4f+Mathf.Abs(Mathf.Sin(Time.time*16))*.9f:.25f,1);
                if(anchorEyes)anchorEyes.localScale=new Vector3(1,Mathf.Repeat(Time.time,4.7f)<.12f?.12f:1,1);
                if(headline && game && game.story)headline.text=Headlines[Mathf.Clamp(game.story.NewsSection,0,3)];
                if(newsClock && game){int seconds=Mathf.CeilToInt(game.Remaining);newsClock.text=$"DAM WATCH   {seconds/60:00}:{seconds%60:00}";}
            }
        }
    }
}
