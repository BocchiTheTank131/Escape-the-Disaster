using UnityEngine;
namespace FloodSurvival
{
    public sealed class FloatingObject : MonoBehaviour
    {
        public float draft=.25f;
        public float drift=.65f;
        float restY;
        Collider solid;
        bool floating;
        void Awake() { restY=transform.position.y; solid=GetComponent<Collider>(); }
        public void Tick(FloodManager flood,float dt)
        {
            float level=flood.GetLevel(transform.position);
            if(level<restY-draft)return;
            if(!floating){floating=true;if(solid)solid.enabled=false;}
            Vector3 p=transform.position; p.y=Mathf.Lerp(p.y,level+draft+Mathf.Sin(Time.time*1.7f+restY)*.12f,1-Mathf.Exp(-4*dt)); p.z-=drift*dt;
            transform.position=p;transform.Rotate(0,dt*3,0,Space.World);
        }
    }
}
