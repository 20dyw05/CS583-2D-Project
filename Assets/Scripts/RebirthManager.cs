using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Rebirth: once you have enough mana, start over with a permanent income bonus and new items.
// The board and mana are wiped (Rebirth rewards on the board go back to the inventory), the Rebirth
// table rolls this Rebirth's rewards, and the reveal window shows them.
public class RebirthManager : MonoBehaviour
{
    public static RebirthManager Instance { get; private set; }

    [SerializeField] GridManager grid;
    [SerializeField] Button rebirthButton;
    [SerializeField] TMP_Text buttonText;
    [SerializeField] TMP_Text statusText;

    [Header("Rewards")]
    [Tooltip("Which items each Rebirth can give, and how rare they are.")]
    [SerializeField] RebirthTable table;
    [Tooltip("The flash of light and the window that shows what you got.")]
    [SerializeField] RebirthReveal reveal;

    [Header("Balance")]
    [SerializeField] double firstCost = 5000;
    [Tooltip("Each Rebirth costs this many times more than the last.")]
    [SerializeField] double costGrowth = 5;
    [Tooltip("Extra income per Rebirth. 0.5 = +50% each time.")]
    [SerializeField] double bonusPerRebirth = 0.5;

    public int Rebirths { get; private set; }

    // What the next Rebirth costs.
    public double Cost
    {
        get { return firstCost * System.Math.Pow(costGrowth, Rebirths); }
    }

    // How much more every Element pays than it did before any Rebirth (x1.5 after one, x2 after two...).
    public double IncomeMultiplier
    {
        get { return 1 + bonusPerRebirth * Rebirths; }
    }

    public RebirthTable Table
    {
        get { return table; }
    }

    // Fired after every Rebirth. The background and the placement controller listen to this.
    public event System.Action Reborn;

    bool rebirthing; // between pressing the button and the board being cleared

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rebirthButton.onClick.AddListener(Rebirth);
        if (GodMode.Instance != null)
        {
            GodMode.Instance.Changed += RefreshText;
        }
        RefreshText();
    }

    void OnDestroy()
    {
        if (GodMode.Instance != null)
        {
            GodMode.Instance.Changed -= RefreshText;
        }
    }

    void Update()
    {
        // The button can only be clicked once you can afford it.
        rebirthButton.interactable = !rebirthing && ManaManager.Instance.CanAfford(Cost);
    }

    public void Rebirth()
    {
        if (rebirthing || !ManaManager.Instance.CanAfford(Cost))
        {
            return;
        }
        StartCoroutine(RebirthRoutine());
    }

    // The screen fills with light, the board is cleared behind it, then the rewards are shown.
    IEnumerator RebirthRoutine()
    {
        rebirthing = true;
        Time.timeScale = 0f; // everything stops while the light comes up
        yield return reveal.FlashIn();

        Rebirths += 1;
        ClearBoard();
        ManaManager.Instance.ResetToStart();
        Inventory.Instance.ResetForRebirth(); // the starting kit, plus every Rebirth reward you had

        List<PlacedItem> won = table.Roll(Rebirths);
        List<bool> isNew = WhichAreNew(won);
        foreach (PlacedItem item in won)
        {
            Inventory.Instance.Add(item);
        }

        RefreshText();
        if (Reborn != null)
        {
            Reborn();
        }

        // The game carries on when the rewards are collected.
        reveal.Show(Rebirths, IncomeMultiplier, won, isNew);
        rebirthing = false;
    }

    // Wipe the board. Rebirth rewards on it go back to the inventory (unless God Mode placed them).
    void ClearBoard()
    {
        foreach (PlacedItem item in grid.AllItems)
        {
            if (item.SourcePrefab.RebirthTier && !item.PlacedFree)
            {
                Inventory.Instance.Add(item.SourcePrefab);
            }
        }
        grid.ClearAll();

        foreach (Element element in FindObjectsByType<Element>())
        {
            element.Dissolve();
        }
    }

    // For each reward: is it something the player has never had before? (Gets a NEW! ribbon.)
    List<bool> WhichAreNew(List<PlacedItem> won)
    {
        List<bool> isNew = new List<bool>();
        List<PlacedItem> seen = new List<PlacedItem>();
        foreach (PlacedItem item in won)
        {
            bool neverHadOne = Inventory.Instance.Count(item) == 0 && !seen.Contains(item);
            isNew.Add(neverHadOne);
            seen.Add(item);
        }
        return isNew;
    }

    void RefreshText()
    {
        if (GodMode.IsOn)
        {
            buttonText.text = "Rebirth\nFree";
        }
        else
        {
            buttonText.text = "Rebirth\n" + Format.Price(Cost);
        }
        statusText.text = "Rebirths: " + Rebirths + "    Income x" + IncomeMultiplier.ToString("0.##");
    }
}