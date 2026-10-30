using TMPro;
using UnityEngine;

// Keeps track of the player's mana (the money of the game) and shows it in the bottom bar.
public class ManaManager : MonoBehaviour
{
    public static ManaManager Instance { get; private set; }

    [SerializeField] TMP_Text manaText;
    [SerializeField] double startingMana = 5000;

    public double Mana { get; private set; }

    void Awake()
    {
        Instance = this;
        Mana = startingMana;
    }

    void Start()
    {
        Refresh();
    }

    public void Add(double amount)
    {
        Mana += amount;
        Refresh();
    }

    // Is there enough mana for this? (In God Mode everything is affordable.)
    public bool CanAfford(double amount)
    {
        if (GodMode.IsOn)
        {
            return true;
        }
        return Mana >= amount;
    }

    // Spend the amount if there's enough. Returns whether it worked.
    // In God Mode it always works and nothing is taken away.
    public bool TrySpend(double amount)
    {
        if (GodMode.IsOn)
        {
            return true;
        }

        if (Mana < amount)
        {
            return false;
        }

        Mana -= amount;
        Refresh();
        return true;
    }

    // Back to the starting amount (used by Rebirth).
    public void ResetToStart()
    {
        Mana = startingMana;
        Refresh();
    }

    // Just the number: the gold mana symbol sits next to it in the bottom bar.
    void Refresh()
    {
        manaText.text = Format.Number(Mana);
    }
}