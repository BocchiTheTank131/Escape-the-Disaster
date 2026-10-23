using UnityEngine;
namespace FloodSurvival
{
    public sealed class FloodManager : MonoBehaviour
    {
        public Transform water;
        public GameObject damCenter;
        public ParticleSystem breachSpray;
        public float FinalLevel=9;
        public float RiseDuration=75;
        public bool IsFlooding {get; private set;}
        public float Level {get; private set;}=-2;
        float elapsed;
        public void Begin() { if(IsFlooding)return; IsFlooding=true; if(damCenter) damCenter.SetActive(false); if(breachSpray) breachSpray.Play(); water.gameObject.SetActive(true); }
        public void Tick(float dt)
        {
            if(!IsFlooding)return; elapsed+=dt;
            Level=Mathf.Lerp(-2,FinalLevel,Mathf.Clamp01(elapsed/RiseDuration));
            water.position=new Vector3(0,Level,0);
        }
    }
}
