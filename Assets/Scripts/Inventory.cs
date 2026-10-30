using System.Collections.Generic;
using UnityEngine;

// How many of each item the player owns.
public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    // One line of the starting kit, editable in the Inspector.
    [System.Serializable]
    public struct Stack
    {
        public PlacedItem item;
        public int count;
    }

    [SerializeField] Stack[] startingItems;

    readonly Dictionary<PlacedItem, int> counts = new Dictionary<PlacedItem, int>();

    // Fired whenever any count changes. The inventory and shop windows listen to this.
    public event System.Action Changed;

    void Awake()
    {
        Instance = this;
        ResetToStart();
    }

    public int Count(PlacedItem item) =>
        item != null && counts.TryGetValue(item, out int n) ? n : 0;

    public void Add(PlacedItem item, int amount = 1)
    {
        counts[item] = Count(item) + amount;
        Changed?.Invoke();
    }

    // Take one if there is one. Returns whether it worked.
    public bool TryTake(PlacedItem item)
    {
        if(Count(item) <= 0) {
            return false;
        } else {
            counts[item] = Count(item) - 1; 
            Changed?.Invoke();
            return true;
        }
    }

    public void ResetToStart()
    {
        counts.Clear();
        foreach (Stack stack in startingItems)
        {
            counts[stack.item] = Count(stack.item) + stack.count;
        }
        Changed?.Invoke();
    }

    // Reset to the starting kit, but keep every Rebirth-tier item.
    public void ResetForRebirth()
    {
        Dictionary<PlacedItem, int> kept = new Dictionary<PlacedItem, int>();
        foreach(KeyValuePair<PlacedItem, int> entry in counts) {
            if(entry.Key.RebirthTier) {
                kept[entry.Key] = entry.Value;
            }    
        }

        ResetToStart();

        foreach(KeyValuePair<PlacedItem, int> entry in kept) {
            Add(entry.Key, entry.Value);
        }
    }
}