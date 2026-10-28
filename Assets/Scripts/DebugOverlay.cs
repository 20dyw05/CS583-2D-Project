using UnityEngine;
using UnityEngine.InputSystem;

// Developer tool: press F1 to show every Element's value above it.
public class DebugOverlay : MonoBehaviour
{
    [SerializeField] Camera cam;

    bool showValues;
    GUIStyle style;

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.f1Key.wasPressedThisFrame)
        {
            showValues = !showValues;
        }
    }

    void OnGUI()
    {
        if (!showValues) return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = Color.white;
        }

        foreach (Element e in FindObjectsByType<Element>())
        {
            // World position just above the Element -> pixels on screen.
            Vector3 screen = cam.WorldToScreenPoint(e.transform.position + Vector3.up * 0.3f);

            // GUI counts y from the top of the screen; the camera counts from the bottom.
            Rect box = new Rect(screen.x - 40f, Screen.height - screen.y - 10f, 80f, 20f);
            GUI.Label(box, e.Value.ToString("0.##"), style);
        }
    }
}