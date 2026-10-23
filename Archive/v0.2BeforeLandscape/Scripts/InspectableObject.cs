using UnityEngine;
namespace FloodSurvival
{
    public sealed class InspectableObject : MonoBehaviour
    {
        public string title;
        [TextArea] public string description;
    }
}
