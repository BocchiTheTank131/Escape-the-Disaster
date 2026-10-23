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
        public bool IsNews {get; private set;}
        float refresh;
        public void SetNews(bool value)
        {
            IsNews=value; if(cartoon)cartoon.SetActive(!value); if(news)news.SetActive(value);
            if(screenGlow)screenGlow.color=value?new Color(1,.2f,.1f):new Color(.4f,.7f,1);
        }
        void Update()
        {
            if(Time.time<refresh)return; refresh=Time.time+.07f;
            if(!IsNews)
            {
                if(fox)fox.anchoredPosition=new Vector2(-90+Mathf.Sin(Time.time*1.6f)*90,-30+Mathf.Abs(Mathf.Sin(Time.time*3))*25);
                if(bird)bird.anchoredPosition=new Vector2(180+Mathf.Sin(Time.time*.9f)*65,55+Mathf.Sin(Time.time*3)*25);
                if(cloud)cloud.anchoredPosition=new Vector2(Mathf.Repeat(Time.time*15,1000)-500,125);
            }
            else if(ticker)ticker.color=Mathf.Sin(Time.time*3)>0?Color.white:new Color(1,.75f,.55f);
        }
    }
}
