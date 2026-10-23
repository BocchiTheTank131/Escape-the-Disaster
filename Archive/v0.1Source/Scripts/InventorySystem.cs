using System;
using UnityEngine;
namespace FloodSurvival
{
    public sealed class InventorySystem : MonoBehaviour
    {
        readonly int[] quantities = new int[4];
        public event Action Changed;
        public int Count(ItemKind kind) => quantities[(int)kind];
        public bool Prepared => Count(ItemKind.Food)>0 && Count(ItemKind.Water)>0 && Count(ItemKind.Medicine)>0 && Count(ItemKind.Flashlight)>0;
        public void Add(ItemDefinition item) { if (!item) return; quantities[(int)item.kind]++; Changed?.Invoke(); }
        public bool Consume(ItemKind kind) { if(Count(kind)==0) return false; quantities[(int)kind]--; Changed?.Invoke(); return true; }
    }
}
