using UnityEngine;
namespace FloodSurvival
{
    public sealed class AtmosphereAudio : MonoBehaviour
    {
        public AudioSource[] sirens;
        public AudioSource ambience, effects;
        public AudioSource television,birds,damSource,floodSource;
        public AudioSource[] voices;
        AudioClip sirenClip,rumble,pickup;
        AudioClip tvTune,birdsong,alert,cracking,woodStep,roadStep,grassStep,splash,panicVoice,radioTone,floodNoise;
        readonly System.Collections.Generic.List<AudioClip> clips=new System.Collections.Generic.List<AudioClip>();
        static readonly int[] Melody={262,330,392,330,294,349,440,392};
        void Awake()
        {
            sirenClip=Tone("Emergency siren",6,0); rumble=Tone("Dam rupture",5,1); pickup=Tone("Supply confirmation",.18f,2);
            var wind=Tone("Valley wind",8,3); ambience.clip=wind; ambience.loop=true; ambience.volume=.19f; ambience.Play();
            foreach(var siren in sirens) { siren.clip=sirenClip; siren.loop=true; siren.volume=.36f; }
            AudioListener.volume=PlayerPrefs.GetFloat("MasterVolume",.65f);
            tvTune=Tone("Original cartoon jingle",8,4); birdsong=Tone("Synthesized valley birds",6,5); alert=Tone("Breaking news alert",1.1f,6); cracking=Tone("Concrete fracture",2.5f,7);
            woodStep=Tone("Wood footstep",.14f,8);roadStep=Tone("Pavement footstep",.14f,9);grassStep=Tone("Grass footstep",.18f,10);splash=Tone("Water splash",.3f,11);panicVoice=Tone("Nonverbal panic call",.75f,12);radioTone=Tone("Radio signal",.5f,14);floodNoise=Tone("Flood roar",8,13);
            if(television){television.clip=tvTune;television.loop=true;television.volume=.24f;}
            if(birds){birds.clip=birdsong;birds.loop=true;birds.volume=.2f;birds.Play();}
            if(floodSource){floodSource.clip=floodNoise;floodSource.loop=true;floodSource.volume=0;}
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
                else if(type==4){float f=Melody[(int)(t*2)%8];float envelope=Mathf.Exp(-Mathf.Repeat(t,.5f)*8);data[i]=(Mathf.Sin(t*2*Mathf.PI*f)+.12f*Mathf.Sin(t*4*Mathf.PI*f))*.22f*envelope;}
                else if(type==5){float chirp=Mathf.Repeat(t,1.5f);data[i]=chirp<.3f?Mathf.Sin(2*Mathf.PI*(1800*t+180*Mathf.Sin(t*15)))*Mathf.Sin(chirp/.3f*Mathf.PI)*.15f:0;}
                else if(type==6)data[i]=Mathf.Sin(t*2*Mathf.PI*((int)(t*8)%2==0?840:620))*.25f*Mathf.Min(t*20,1)*Mathf.Clamp01((seconds-t)*8);
                else if(type==7)data[i]=(noise*.3f*Mathf.Pow(Mathf.Abs(Mathf.Sin(t*41)),18)+filtered+Mathf.Sin(t*220)*.15f)*Mathf.Exp(-t*1.5f);
                else if(type>=8 && type<=11)data[i]=(type==8?Mathf.Sin(t*2*Mathf.PI*115)*.3f+filtered*.4f:type==11?filtered*.9f+noise*.08f:noise*(type==9?.2f:.12f)+filtered*.2f)*Mathf.Exp(-t*(type==11?8:30))*Mathf.Min(t*200,1);
                else if(type==12){float env=Mathf.Sin(t/seconds*Mathf.PI);float freq=180+50*Mathf.Sin(t*12);data[i]=(Mathf.Sin(t*2*Mathf.PI*freq)*.16f+Mathf.Sin(t*2*Mathf.PI*freq*3)*.08f)*env;}
                else if(type==13)data[i]=(filtered*.9f+noise*.025f)*(.8f+.2f*Mathf.Sin(t*2*Mathf.PI/8));
                else if(type==14)data[i]=Mathf.Sin(t*2*Mathf.PI*1200)*.15f*Mathf.Exp(-t*8)+filtered*.15f;
                else data[i]=filtered*.5f*(.7f+.3f*Mathf.Sin(t*Mathf.PI/4));
            }
            var clip=AudioClip.Create(title,data.Length,1,rate,false); clip.SetData(data,0);clips.Add(clip); return clip;
        }
        public void StartWarning() { foreach(var source in sirens) source.Play(); }
        public void Collapse() { if(damSource)damSource.PlayOneShot(rumble,1);else effects.PlayOneShot(rumble,.9f); ambience.volume=.25f;if(floodSource)floodSource.Play(); }
        public void Pickup() { effects.PlayOneShot(pickup,.7f); }
        public void Tick(bool flooding,float remaining) { foreach(var source in sirens) source.volume=flooding?.16f:remaining<30?.42f:.3f; if(floodSource)floodSource.volume=Mathf.MoveTowards(floodSource.volume,flooding?.32f:0,Time.deltaTime*.08f); }
        public void Television(bool play) { if(television){if(play)television.Play();else television.Stop();} }
        public void BreakingNews() { Television(false); if(television)television.PlayOneShot(alert,.8f);else effects.PlayOneShot(alert,.6f);if(birds)birds.volume=.07f; }
        public void Crack() { if(damSource)damSource.PlayOneShot(cracking,.85f);else effects.PlayOneShot(cracking,.7f); }
        public void Radio() { effects.PlayOneShot(radioTone,.5f); }
        public void NpcReaction(Vector3 position,int index) { if(voices==null || voices.Length==0)return;var source=voices[index%voices.Length];if(source.isPlaying)return;source.transform.position=position;source.pitch=.9f+(index%4)*.09f;source.PlayOneShot(panicVoice,.35f); }
        public void Footstep(string surface,bool wet) { var clip=wet?splash:surface.IndexOf("Floor",System.StringComparison.OrdinalIgnoreCase)>=0 || surface.Contains("Table")?woodStep:surface.Contains("Street") || surface.Contains("sidewalk") || surface.Contains("trail") || surface.Contains("terrace")?roadStep:grassStep;effects.PlayOneShot(clip,wet?.25f:.2f); }
        public void Silence() { foreach(var source in sirens) source.Stop(); }
        public void SetVolume(float volume) { AudioListener.volume=volume; PlayerPrefs.SetFloat("MasterVolume",volume); }
        void OnDestroy() { foreach(var clip in clips)if(clip)Destroy(clip); }
    }
}
