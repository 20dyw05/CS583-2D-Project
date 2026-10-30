using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// God Mode, for demos and recording videos. While it's on, everything is free: the shop, upgrades
// and Rebirth cost nothing, and every item in the game can be placed from the inventory without
// running out. Click the God Mode button in the bottom bar (or press G) to switch it on and off.
//
// Items placed while God Mode is on didn't come out of the inventory, so stashing them later doesn't
// add them to it, and upgrades bought for free give no mana back.
public class GodMode : MonoBehaviour
{
    public static GodMode Instance { get; private set; }

    [SerializeField] Button button;
    [SerializeField] TMP_Text label;
    [Tooltip("The button's text colour while God Mode is on.")]
    [SerializeField] Color onColor = new Color(1f, 0.85f, 0.35f, 1f);

    bool isOn;
    Color offColor;

    // Fired whenever God Mode is switched on or off. Menus listen to this to redraw their prices.
    public event System.Action Changed;

    // Is God Mode on right now? Everything that costs mana or uses up items checks this.
    public static bool IsOn
    {
        get { return Instance != null && Instance.isOn; }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        offColor = label.color;
        button.onClick.AddListener(Toggle);
        RefreshButton();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.gKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    // Switch God Mode on if it's off, and off if it's on.
    public void Toggle()
    {
        isOn = !isOn;
        RefreshButton();

        if (Changed != null)
        {
            Changed();
        }
    }

    void RefreshButton()
    {
        if (isOn)
        {
            label.text = "God Mode: On";
            label.color = onColor;
        }
        else
        {
            label.text = "God Mode (G)";
            label.color = offColor;
        }
    }
}