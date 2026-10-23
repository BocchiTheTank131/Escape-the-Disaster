using System;
using UnityEngine;
namespace FloodSurvival
{
    public sealed class InventorySystem : MonoBehaviour
    {
        readonly int[] quantities = new int[7];
        readonly int[] costs = {1,1,1,1,1,0,2};
        public int baseCapacity=6, backpackCapacity=12;
        public int Capacity => Count(ItemKind.Backpack)>0?backpackCapacity:baseCapacity;
        public int Used { get { int used=0; for(int i=0;i<quantities.Length;i++)used+=quantities[i]*costs[i]; return used; } }
        public event Action Changed;
        public int Count(ItemKind kind) => quantities[(int)kind];
        public bool Prepared => Count(ItemKind.Food)>0 && Count(ItemKind.Water)>0 && Count(ItemKind.Medicine)>0 && Count(ItemKind.Flashlight)>0;
        public void Add(ItemDefinition item) { TryAdd(item); }
        public bool TryAdd(ItemDefinition item)
        {
            if(!item)return false;
            int index=(int)item.kind;
            if((item.kind==ItemKind.Backpack || item.kind==ItemKind.Flashlight || item.kind==ItemKind.Radio) && quantities[index]>0)return false;
            if(item.kind!=ItemKind.Backpack && Used+costs[index]>Capacity)return false;
            quantities[index]++; Changed?.Invoke(); return true;
        }
        public bool Consume(ItemKind kind) { if(Count(kind)==0) return false; quantities[(int)kind]--; Changed?.Invoke(); return true; }
    }
}
