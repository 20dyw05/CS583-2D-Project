using TMPro;
using UnityEngine;

public class ManaManager : MonoBehaviour
{
    public static ManaManager Instance { get; private set; }

    [SerializeField] TMP_Text manaText;
    [SerializeField] double startingMana = 5000;

    public double Mana { get; private set; }

    void Awake() {
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

    void Refresh()
    {
        manaText.text = Mana.ToString("0.##") + " mana";
    }

    // Spend the amount if there's enough. Returns whether it worked.
    public bool TrySpend(double amount) {
        // 1. If Mana is less than amount, return false.
        // 2. Otherwise, subtract amount from Mana.
        // 3. Update the text (the same method Add uses).
        // 4. Return true.

        if(Mana < amount) {
            return false;
        } else {
            Mana -= amount;
            Refresh();
            return true;
        }

    }

    // Back to the starting amount (used by Rebirth).
    public void ResetToStart()
    {
        // Set Mana to startingMana, then update the text.
        Mana = startingMana;
        Refresh();
    }
}