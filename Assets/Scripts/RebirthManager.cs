using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RebirthManager : MonoBehaviour
{
    public static RebirthManager Instance { get; private set; }

    [SerializeField] GridManager grid;
    [SerializeField] Button rebirthButton;
    [SerializeField] TMP_Text buttonText;
    [SerializeField] TMP_Text statusText;

    [Header("Rewards")]
    [SerializeField] PlacedItem[] rewards;
    [SerializeField] GameObject choicePanel;
    [SerializeField] Transform choiceButtons;
    [SerializeField] ItemCard choiceCardPrefab;

    [Header("Balance")]
    [SerializeField] double firstCost = 5000;
    [Tooltip("Each Rebirth costs this many times more than the last.")]
    [SerializeField] double costGrowth = 5;
    [Tooltip("Extra income per Rebirth. 0.5 = +50% each time.")]
    [SerializeField] double bonusPerRebirth = 0.5;

    public int Rebirths { get; private set; }
    public double Cost => firstCost * System.Math.Pow(costGrowth, Rebirths);
    public double IncomeMultiplier => 1 + bonusPerRebirth * Rebirths;

    // Fired after every Rebirth. The background listens to this.
    public event System.Action Reborn;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rebirthButton.onClick.AddListener(Rebirth);
        RefreshText();
    }

    void Update()
    {
        // The button can only be clicked once you can afford it.
        rebirthButton.interactable = ManaManager.Instance.Mana >= Cost;
    }

    public void Rebirth()
    {
        if (ManaManager.Instance.Mana < Cost) return;

        Rebirths += 1;

        // Return Rebirth-tier items on the board to the inventory before wiping.
        foreach (PlacedItem item in grid.AllItems)
        {
            if (item.SourcePrefab.RebirthTier)
            {
                Inventory.Instance.Add(item.SourcePrefab);
            }
        }

        grid.ClearAll();

        foreach (Element element in FindObjectsByType<Element>())
        {
            element.Dissolve();
        }

        ManaManager.Instance.ResetToStart();
        Inventory.Instance.ResetForRebirth();   // starting kit + kept rewards
        RefreshText();
        Reborn?.Invoke();
        ShowChoices();
    }

    void RefreshText()
    {
        buttonText.text = "Rebirth\n" + Format.Price(Cost);
        statusText.text = "Rebirths: " + Rebirths + "    Income x" + IncomeMultiplier.ToString("0.##");
    }
    
    // Offer every reward the player doesn't own yet.
    void ShowChoices()
    {
        foreach (Transform child in choiceButtons)
        {
            Destroy(child.gameObject);
        }

        bool anyLeft = false;
        foreach (PlacedItem reward in rewards)
        {
            if (Inventory.Instance.Count(reward) > 0) continue; // already owned
            anyLeft = true;

            ItemCard card = Instantiate(choiceCardPrefab, choiceButtons);
            card.Set(reward);
            card.SetDetail("Rebirth reward");
            card.Button.onClick.AddListener(() => Choose(reward));
        }

        if (!anyLeft) return; // everything owned: nothing to choose

        choicePanel.SetActive(true);
        Time.timeScale = 0f; // freeze the game while choosing
    }

    void Choose(PlacedItem reward)
    {
        Inventory.Instance.Add(reward);
        choicePanel.SetActive(false);
        Time.timeScale = 1f; // Unfreeze

    }
}