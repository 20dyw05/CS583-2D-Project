using System.Collections.Generic;
using UnityEngine;

// How many of each item the player owns (items waiting to be placed on the board).
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

    // How many of this item the player owns.
    public int Count(PlacedItem item)
    {
        if (item == null)
        {
            return 0;
        }

        int count;
        if (counts.TryGetValue(item, out count))
        {
            return count;
        }
        return 0;
    }

    // Can one of these be placed right now? Always yes in God Mode.
    public bool CanTake(PlacedItem item)
    {
        return GodMode.IsOn || Count(item) > 0;
    }

    public void Add(PlacedItem item, int amount = 1)
    {
        counts[item] = Count(item) + amount;
        TellListeners();
    }

    // Take one out to place it, if there is one. Returns whether it worked.
    // In God Mode it always works and the count doesn't go down.
    public bool TryTake(PlacedItem item)
    {
        if (GodMode.IsOn)
        {
            return true;
        }

        if (Count(item) <= 0)
        {
            return false;
        }

        counts[item] = Count(item) - 1;
        TellListeners();
        return true;
    }

    // Back to just the starting kit.
    public void ResetToStart()
    {
        counts.Clear();
        foreach (Stack stack in startingItems)
        {
            counts[stack.item] = Count(stack.item) + stack.count;
        }
        TellListeners();
    }

    // Back to the starting kit, but keep every Rebirth-tier item (Rebirth rewards are never lost).
    public void ResetForRebirth()
    {
        // Remember the Rebirth-tier items...
        Dictionary<PlacedItem, int> kept = new Dictionary<PlacedItem, int>();
        foreach (KeyValuePair<PlacedItem, int> entry in counts)
        {
            if (entry.Key.RebirthTier)
            {
                kept[entry.Key] = entry.Value;
            }
        }

        // ...start over...
        ResetToStart();

        // ...and put them back.
        foreach (KeyValuePair<PlacedItem, int> entry in kept)
        {
            Add(entry.Key, entry.Value);
        }
    }

    void TellListeners()
    {
        if (Changed != null)
        {
            Changed();
        }
    }
}