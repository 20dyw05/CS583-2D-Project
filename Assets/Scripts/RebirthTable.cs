using System.Collections.Generic;
using UnityEngine;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

// Which items a Rebirth can give, like Miner's Haven's reborn items. Every item has a rarity and the
// first Rebirth that can give it (some also have a last one). Each Rebirth draws its rewards at random
// from everything it can give, rarer items less often. A few Rebirths always give one particular item
// (the milestones), and one Rebirth never gives the same item twice.
[CreateAssetMenu(menuName = "Elemental/Rebirth Table")]
public class RebirthTable : ScriptableObject
{
    // One line of the table.
    [System.Serializable]
    public struct Entry
    {
        public PlacedItem item;
        public Rarity rarity;
        [Tooltip("The first Rebirth that can give it.")]
        public int fromRebirth;
        [Tooltip("The last Rebirth that can give it. 0 = no last one.")]
        public int untilRebirth;
        [Tooltip("This Rebirth always gives it, as one of its rewards. 0 = none.")]
        public int alwaysAt;
    }

    public Entry[] entries;

    [Header("Rewards per Rebirth")]
    [Tooltip("How many rewards a Rebirth gives at first.")]
    public int rewards = 1;
    [Tooltip("One more reward every this many Rebirths (5: two from Rebirth 5, three from Rebirth 10...).")]
    public int extraRewardEvery = 5;

    [Header("Rarities, Common to Legendary")]
    [Tooltip("How often each rarity comes up, compared to the others.")]
    public float[] weights = { 100f, 50f, 22f, 9f, 3f };
    [Tooltip("The colour each rarity is shown in.")]
    public Color[] colors =
    {
        new Color32(200, 204, 214, 255),
        new Color32(104, 222, 120, 255),
        new Color32(88, 162, 255, 255),
        new Color32(186, 112, 250, 255),
        new Color32(255, 196, 64, 255),
    };

    // How many rewards this Rebirth gives.
    public int RewardsFor(int rebirth)
    {
        return rewards + rebirth / Mathf.Max(1, extraRewardEvery);
    }

    // How likely this rarity is, compared to the others.
    public float WeightOf(Rarity rarity)
    {
        return weights[(int)rarity];
    }

    // The colour this rarity is shown in.
    public Color ColorOf(Rarity rarity)
    {
        return colors[(int)rarity];
    }

    // Can this Rebirth give this line's item?
    public bool CanGive(Entry entry, int rebirth)
    {
        if (entry.item == null || rebirth < entry.fromRebirth)
        {
            return false;
        }
        bool hasLastRebirth = entry.untilRebirth != 0;
        if (hasLastRebirth && rebirth > entry.untilRebirth)
        {
            return false;
        }
        return true;
    }

    // Everything this Rebirth can give.
    public List<Entry> PoolFor(int rebirth)
    {
        List<Entry> pool = new List<Entry>();
        foreach (Entry entry in entries)
        {
            if (CanGive(entry, rebirth))
            {
                pool.Add(entry);
            }
        }
        return pool;
    }

    // Draw this Rebirth's rewards: its milestone item first, then random picks from the pool.
    public List<PlacedItem> Roll(int rebirth)
    {
        List<PlacedItem> won = new List<PlacedItem>();
        int count = RewardsFor(rebirth);

        // Milestones: items this Rebirth always gives.
        foreach (Entry entry in entries)
        {
            bool isMilestone = entry.alwaysAt == rebirth && entry.item != null;
            if (isMilestone && won.Count < count && !won.Contains(entry.item))
            {
                won.Add(entry.item);
            }
        }

        // The rest are random. Whatever has been given leaves the pool, so nothing comes up twice.
        List<Entry> pool = PoolFor(rebirth);
        foreach (PlacedItem item in won)
        {
            RemoveFromPool(pool, item);
        }

        while (won.Count < count && pool.Count > 0)
        {
            PlacedItem picked = PickWeighted(pool);
            won.Add(picked);
            RemoveFromPool(pool, picked);
        }
        return won;
    }

    // Pick one item at random, where each rarity's weight is its share of the chances.
    // Imagine the weights laid end to end on a line: pick a random point on the line and see whose
    // stretch it lands in.
    PlacedItem PickWeighted(List<Entry> pool)
    {
        float total = 0f;
        foreach (Entry entry in pool)
        {
            total += WeightOf(entry.rarity);
        }

        float point = Random.value * total;
        foreach (Entry entry in pool)
        {
            float weight = WeightOf(entry.rarity);
            if (point < weight)
            {
                return entry.item;
            }
            point -= weight;
        }

        // Only reached if the random number landed exactly on the end of the line.
        return pool[pool.Count - 1].item;
    }

    static void RemoveFromPool(List<Entry> pool, PlacedItem item)
    {
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            if (pool[i].item == item)
            {
                pool.RemoveAt(i);
            }
        }
    }

    // This item's line in the table, if it has one.
    public bool TryFind(PlacedItem item, out Entry found)
    {
        foreach (Entry entry in entries)
        {
            if (entry.item == item)
            {
                found = entry;
                return true;
            }
        }
        found = new Entry();
        return false;
    }
}