using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Builds one shop button per palette item when the game starts.
public class ShopPanel : MonoBehaviour
{
    [SerializeField] PlacementController placement;
    [SerializeField] Button buttonPrefab;

    void Start()
    {
        PlacedItem[] items = placement.Palette;
        for (int i = 0; i < items.Length; i++)
        {
            int index = i; // a fresh copy for this button (see question 2)

            Button button = Instantiate(buttonPrefab, transform);
            button.GetComponentInChildren<TMP_Text>().text =
                items[i].DisplayName + "\n" + items[i].Price.ToString("0.##") + " mana";

            button.onClick.AddListener(() => placement.Select(index));
        }
    }
}