using UnityEngine;

public enum CatalystEffect { Add, Multiply }

// One Catalyst's settings, stored as a file in the Project panel.
[CreateAssetMenu(menuName = "Elemental/Catalyst")]
public class CatalystData : ScriptableObject
{
    public string displayName = "New Catalyst";
    public CatalystEffect effect = CatalystEffect.Add;
    public double amount = 1;

    [Tooltip("Only works on Elements worth less than this. 0 = no cap.")]
    public double valueCap = 0;

    [Tooltip("How many times one Element can use it. 0 = unlimited.")]
    public int useLimit = 0;

    [Header("Optional: permanently recolour the Element")]
    public bool tintsElement = false;
    public Color tint = Color.white;
}