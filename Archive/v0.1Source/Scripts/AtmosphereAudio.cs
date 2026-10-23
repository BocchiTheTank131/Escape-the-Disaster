using UnityEngine;
namespace FloodSurvival
{
    public sealed class AtmosphereAudio : MonoBehaviour
    {
        public AudioSource[] sirens;
        public AudioSource ambience, effects;
        AudioClip sirenClip,rumble,pickup;
        void Awake()
        {
            sirenClip=Tone("Emergency siren",6,0); rumble=Tone("Dam rupture",5,1); pickup=Tone("Supply confirmation",.18f,2);
            var wind=Tone("Valley wind",8,3); ambience.clip=wind; ambience.loop=true; ambience.volume=.19f; ambience.Play();
            foreach(var siren in sirens) { siren.clip=sirenClip; siren.loop=true; siren.volume=.36f; }
            AudioListener.volume=PlayerPrefs.GetFloat("MasterVolume",.65f);
        }
        AudioClip Tone(string title,float seconds,int type)
        {
            const int rate=22050; var data=new float[(int)(rate*seconds)]; var random=new System.Random(173+type); float phase=0,filtered=0;
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate; float noise=(float)random.NextDouble()*2-1; filtered=Mathf.Lerp(filtered,noise,.06f);
                if(type==0) { phase+=2*Mathf.PI*(510+230*Mathf.Sin(t*Mathf.PI/3))/rate; data[i]=(Mathf.Sin(phase)+.2f*Mathf.Sin(phase*2))*.36f; }
                else if(type==1) data[i]=(filtered*1.7f+Mathf.Sin(t*2*Mathf.PI*42)*.18f)*Mathf.Exp(-t*.65f)*Mathf.Min(t*30,1);
                else if(type==2) data[i]=Mathf.Sin(t*2*Mathf.PI*(740+t*1200))*.25f*Mathf.Sin(t/seconds*Mathf.PI);
                else data[i]=filtered*.5f*(.7f+.3f*Mathf.Sin(t*Mathf.PI/4));
            }
            var clip=AudioClip.Create(title,data.Length,1,rate,false); clip.SetData(data,0); return clip;
        }
        public void StartWarning() { foreach(var source in sirens) source.Play(); }
        public void Collapse() { effects.PlayOneShot(rumble,.9f); ambience.volume=.32f; }
        public void Pickup() { effects.PlayOneShot(pickup,.7f); }
        public void Tick(bool flooding,float remaining) { foreach(var source in sirens) source.volume=flooding?.22f:remaining<30?.5f:.36f; }
        public void Silence() { foreach(var source in sirens) source.Stop(); }
        public void SetVolume(float volume) { AudioListener.volume=volume; PlayerPrefs.SetFloat("MasterVolume",volume); }
        void OnDestroy() { if(sirenClip)Destroy(sirenClip); if(rumble)Destroy(rumble); if(pickup)Destroy(pickup); if(ambience && ambience.clip)Destroy(ambience.clip); }
    }
}
