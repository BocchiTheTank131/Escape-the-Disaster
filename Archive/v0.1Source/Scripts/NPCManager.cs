using UnityEngine;
namespace FloodSurvival
{
    public sealed class NPCManager : MonoBehaviour
    {
        public Evacuee[] citizens;
        public void Activate() { foreach(var citizen in citizens) if(citizen) citizen.Panic(); }
    }
}
