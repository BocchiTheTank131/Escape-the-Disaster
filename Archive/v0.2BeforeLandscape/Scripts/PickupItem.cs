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
            if(!inventory.TryAdd(definition))return false;
            Collected=true; gameObject.SetActive(false); return true;
        }
    }
}
