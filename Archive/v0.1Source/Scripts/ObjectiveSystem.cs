using UnityEngine;
namespace FloodSurvival
{
    public sealed class ObjectiveSystem : MonoBehaviour
    {
        public Transform home, shelter;
        public bool ReturnedHome { get; private set; }
        public bool MissedHome { get; private set; }
        public Transform Target => GameManager.Instance.State == GameState.Flood ? shelter : home;
        public string Text => GameManager.Instance.State == GameState.Flood ? "Reach Ridge Shelter. Stay above the water." : ReturnedHome ? "Home reached. Gather supplies, then prepare to evacuate." : "Return home and prepare for the flood.";
        public void Tick(Vector3 position)
        {
            if(GameManager.Instance.State==GameState.Preparation && Vector3.Distance(position,home.position)<5.5f && !ReturnedHome)
            { ReturnedHome=true; GameManager.Instance.Announce("HOME REACHED", "Your house cannot withstand the flood. Follow the ridge trail when the dam breaks.",7); }
        }
        public void OnBreach() { MissedHome=!ReturnedHome; }
    }
}
