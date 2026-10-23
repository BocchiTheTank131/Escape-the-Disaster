using UnityEngine;
namespace FloodSurvival
{
    public enum ItemKind { Food, Water, Medicine, Flashlight }
    [CreateAssetMenu(menuName = "Flood Survival/Item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        public ItemKind kind;
        public string displayName;
        [TextArea] public string description;
        public Color color = Color.white;
    }
}
