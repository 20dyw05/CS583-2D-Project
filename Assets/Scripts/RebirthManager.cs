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

    [Header("Balance")]
    [SerializeField] double firstCost = 5000;
    [Tooltip("Each Rebirth costs this many times more than the last.")]
    [SerializeField] double costGrowth = 5;
    [Tooltip("Extra income per Rebirth. 0.5 = +50% each time.")]
    [SerializeField] double bonusPerRebirth = 0.5;

    public int Rebirths { get; private set; }
    public double Cost => firstCost * System.Math.Pow(costGrowth, Rebirths);
    public double IncomeMultiplier => 1 + bonusPerRebirth * Rebirths;

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
        if(ManaManager.Instance.Mana < Cost) {
            return;
        }

        Rebirths+=1;
        grid.ClearAll();

        foreach (Element element in FindObjectsByType<Element>())
        {
            element.Dissolve();
        }

        ManaManager.Instance.ResetToStart();
        Update();
        RefreshText();
    }

    void RefreshText()
    {
        buttonText.text = "Rebirth\n" + Cost.ToString("0") + " mana";
        statusText.text = "Rebirths: " + Rebirths + "    Income x" + IncomeMultiplier.ToString("0.##");
    }
}