using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Every Element's value, always shown just above it (what the F1 debug overlay used to do).
// Labels are reused from frame to frame instead of being created and destroyed.
public class ValueLabels : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] TMP_Text labelPrefab;
    [Tooltip("How far above an Element's centre the label sits, in pixels of the art.")]
    [SerializeField] float height = 11f;

    readonly List<TMP_Text> labels = new List<TMP_Text>();
    readonly List<double> shown = new List<double>();   // the value each label is showing

    void LateUpdate()
    {
        int used = 0;
        foreach (Element element in Element.All)
        {
            if (element.Sinking) continue; // on its way into a Transmutator

            if (used == labels.Count)
            {
                labels.Add(Instantiate(labelPrefab, transform));
                shown.Add(double.NaN);
            }

            TMP_Text label = labels[used];
            if (!label.gameObject.activeSelf) label.gameObject.SetActive(true);
            if (shown[used] != element.Value)
            {
                label.text = Format.Short(element.Value);
                shown[used] = element.Value;
            }

            // Pixels of the art -> world -> screen.
            Vector3 above = element.transform.position + Vector3.up * (height / 50f);
            label.rectTransform.position = cam.WorldToScreenPoint(above);
            used++;
        }

        // Hide the labels nobody needs this frame.
        for (int i = used; i < labels.Count; i++)
        {
            if (labels[i].gameObject.activeSelf) labels[i].gameObject.SetActive(false);
        }
    }
}