using TMPro;
using UnityEngine;

public class ManaManager : MonoBehaviour
{
    public static ManaManager Instance { get; private set; }

    [SerializeField] TMP_Text manaText;

    public double Mana { get; private set; }

    void Awake()
    {
        Instance = this;
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
}