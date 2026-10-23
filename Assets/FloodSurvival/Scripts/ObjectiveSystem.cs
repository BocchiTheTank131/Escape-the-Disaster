using UnityEngine;
namespace FloodSurvival
{
    public sealed class ObjectiveSystem : MonoBehaviour
    {
        public Transform home;
        [UnityEngine.Serialization.FormerlySerializedAs("shelter")] public Transform highGround;
        public Transform supplies;
        public int HouseSupplies {get; private set;}
        public bool LeftHouse {get; private set;}
        public bool ReturnedHome { get; private set; }
        public bool MissedHome { get; private set; }
        public Transform Target => GameManager.Instance.State!=GameState.Flood && !LeftHouse && HouseSupplies<2 && supplies?supplies:highGround;
        public string TargetName => Target==supplies?"HOUSE SUPPLIES":"WOODED HIGH GROUND";
        public string Text
        {
            get
            {
                var g=GameManager.Instance;
                if(g.State==GameState.Opening)return "A normal day. Watch the television.";
                if(g.story && g.story.Seated)return "Listen to the warning. Press E to get up.";
                if(g.State==GameState.Flood)return "Reach high ground. Hold Space to swim up in deep water.";
                if(!LeftHouse && HouseSupplies<2)return "Prepare to evacuate. Gather house supplies (optional).";
                return "Leave the flood-risk area. Follow the amber ridge trail.";
            }
        }
        public void Tick(Vector3 position)
        {
            var game=GameManager.Instance;
            if(!game.EmergencyStarted)return;
            ReturnedHome=true;
            bool inside=game.story?game.story.HouseBounds.Contains(position):Vector3.Distance(position,home.position)<7;
            if(!inside)LeftHouse=true;
        }
        public void CollectedAt(Vector3 position) { if(GameManager.Instance.story && GameManager.Instance.story.HouseBounds.Contains(position))HouseSupplies++; }
        public void OnBreach() { MissedHome=!ReturnedHome; }
    }
}
