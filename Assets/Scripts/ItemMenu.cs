using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The little window beside a selected item on the board: its name and stage, then Upgrade,
// Move (R) and Stash (Z). The buttons call back into the PlacementController, which does the work.
// The tooltip explaining the item sits on its other side.
public class ItemMenu : MonoBehaviour
{
    [SerializeField] PlacementController placement;
    [SerializeField] Camera cam;
    [Tooltip("The window that shows and hides. Must be a child, not this object.")]
    [SerializeField] RectTransform panel;
    [SerializeField] TMP_Text title;
    [SerializeField] Button upgradeButton;
    [SerializeField] Button moveButton;
    [SerializeField] Button stashButton;
    [Tooltip("Keeps the window above the bottom bar (in canvas units).")]
    [SerializeField] float bottomBarHeight = 120f;

    PlacedItem item;
    TMP_Text upgradeText;
    Canvas canvas;

    public bool IsOpen
    {
        get { return panel.gameObject.activeSelf; }
    }

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        upgradeText = upgradeButton.GetComponentInChildren<TMP_Text>();

        upgradeButton.onClick.AddListener(placement.Upgrade);
        moveButton.onClick.AddListener(placement.Move);
        stashButton.onClick.AddListener(placement.Stash);

        panel.gameObject.SetActive(false); // start closed
    }

    public void Show(PlacedItem target)
    {
        item = target;
        panel.gameObject.SetActive(true);
        Refresh();
        Follow();
    }

    public void Hide()
    {
        item = null;
        panel.gameObject.SetActive(false);
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideSelected();
        }
    }

    void LateUpdate()
    {
        if (!IsOpen)
        {
            return;
        }

        if (item == null)
        {
            Hide(); // the item was removed
            return;
        }

        Refresh();
        Follow();
    }

    // Name and stage, and the price of the next stage (greyed out until you can afford it).
    void Refresh()
    {
        title.text = item.StageName;

        // Items with only one stage (conveyors) just get Move and Stash.
        upgradeButton.gameObject.SetActive(item.StageCount > 1);

        if (item.CanUpgrade)
        {
            string price;
            if (GodMode.IsOn)
            {
                price = "Free";
            }
            else
            {
                price = Format.Price(item.UpgradeCost);
            }
            upgradeText.text = "Upgrade\n" + price;
            upgradeButton.interactable = ManaManager.Instance.CanAfford(item.UpgradeCost);
        }
        else
        {
            upgradeText.text = "Fully upgraded";
            upgradeButton.interactable = false;
        }
    }

    // Sit beside the item: on its right, or on its left if there's no room.
    // The tooltip goes on the other side.
    void Follow()
    {
        float scale = canvas.scaleFactor;
        Vector2 size = panel.rect.size * scale;

        // The screen points a little to the right and left of the item.
        Vector3 centre = item.transform.position;
        Vector3 right = cam.WorldToScreenPoint(centre + new Vector3(0.6f, 0f, 0f));
        Vector3 left = cam.WorldToScreenPoint(centre - new Vector3(0.6f, 0f, 0f));

        bool onRight = right.x + size.x <= Screen.width;
        float x;
        if (onRight)
        {
            x = right.x;
        }
        else
        {
            x = left.x - size.x;
        }

        float y = right.y - size.y / 2f;
        y = Mathf.Clamp(y, bottomBarHeight * scale + 4f, Screen.height - size.y - 4f);

        // The panel's pivot is its bottom-left corner.
        panel.position = new Vector3(x, y, 0f);

        if (ItemTooltip.Instance == null)
        {
            return;
        }

        if (onRight)
        {
            ItemTooltip.Instance.ShowSelected(item, left, new Vector2(x + size.x, left.y));
        }
        else
        {
            ItemTooltip.Instance.ShowSelected(item, new Vector2(x, left.y), right);
        }
    }
}