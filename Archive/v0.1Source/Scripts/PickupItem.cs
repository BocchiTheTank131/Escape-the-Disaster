using UnityEngine;
namespace FloodSurvival
{
    public sealed class PickupItem : MonoBehaviour
    {
        public ItemDefinition definition;
        public bool Collected { get; private set; }
        public bool Collect(InventorySystem inventory)
        {
            if (Collected || !definition || !inventory) return false;
            Collected=true; inventory.Add(definition); gameObject.SetActive(false); return true;
        }
    }
}
