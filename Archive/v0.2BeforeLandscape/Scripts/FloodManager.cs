using UnityEngine;
namespace FloodSurvival
{
    public sealed class FloodManager : MonoBehaviour
    {
        public Transform water;
        public GameObject damCenter;
        public ParticleSystem breachSpray;
        public Transform surgeFront;
        public Transform[] fallingDebris;
        public FloatingObject[] floaters;
        public ParticleSystem collapseDust;
        public float originZ=108, frontSpeed=4.2f,currentStrength=1.7f;
        public float FrontZ {get; private set;}=108;
        public float FinalLevel=9;
        public float RiseDuration=75;
        public bool IsFlooding {get; private set;}
        public float Level {get; private set;}=-2;
        float elapsed;
        Vector3[] velocities;
        void Awake() { velocities=new Vector3[fallingDebris==null?0:fallingDebris.Length]; FrontZ=originZ; }
        public void Begin()
        {
            if(IsFlooding)return; IsFlooding=true; FrontZ=originZ;
            if(damCenter)damCenter.SetActive(false); if(breachSpray)breachSpray.Play(); if(collapseDust)collapseDust.Play(); water.gameObject.SetActive(true);
            if(surgeFront)surgeFront.gameObject.SetActive(true);
            for(int i=0;i<velocities.Length;i++){fallingDebris[i].gameObject.SetActive(true);velocities[i]=new Vector3(Mathf.Sin(i*2.4f)*5,3+i%3,-8-i%4);}
            Shader.SetGlobalFloat("_FloodFrontZ",FrontZ);
        }
        public void Tick(float dt)
        {
            if(!IsFlooding)return; elapsed+=dt;
            Level=Mathf.Lerp(-2,FinalLevel,Mathf.Clamp01(elapsed/RiseDuration));
            FrontZ=originZ-frontSpeed*elapsed;
            water.position=new Vector3(0,Level,0);
            Shader.SetGlobalFloat("_FloodFrontZ",FrontZ);
            if(surgeFront)surgeFront.position=new Vector3(0,Level+.1f,FrontZ+1.8f);
            for(int i=0;i<velocities.Length;i++)
            {
                if(!fallingDebris[i])continue;
                if(elapsed<8){velocities[i]+=Vector3.down*9*dt;fallingDebris[i].position+=velocities[i]*dt;fallingDebris[i].Rotate(new Vector3(13+i*3,21,12)*dt);}
                else fallingDebris[i].gameObject.SetActive(false);
            }
            if(floaters!=null)foreach(var floater in floaters)if(floater)floater.Tick(this,dt);
        }
        public float GetLevel(Vector3 position) => IsFlooding && position.z>=FrontZ?Level:-100;
        public Vector3 CurrentAt(Vector3 position)
        {
            float depth=GetLevel(position)-position.y;
            if(depth<.7f)return Vector3.zero;
            return new Vector3(Mathf.Sin(Time.time+position.x*.1f)*.18f,0,-currentStrength*Mathf.Clamp01(depth/2));
        }
    }
}
