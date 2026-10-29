using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The bottom row: one button per item you own, with how many.
public class Hotbar : MonoBehaviour
{
    [SerializeField] PlacementController placement;
    [SerializeField] Button buttonPrefab;

    void Start()
    {
        Inventory.Instance.Changed += Rebuild; // subscribe
        Rebuild();
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null) Inventory.Instance.Changed -= Rebuild; // unsubscribe
    }

    void Rebuild()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        List<PlacedItem> items = placement.HotbarItems;
        for (int i = 0; i < items.Count; i++)
        {
            PlacedItem item = items[i];

            Button button = Instantiate(buttonPrefab, transform);
            button.GetComponentInChildren<TMP_Text>().text =
                item.DisplayName + "\nx" + Inventory.Instance.Count(item);

            button.onClick.AddListener(() => placement.Select(item));
        }
    }
}