// How numbers are written in the UI.
public static class Format
{
    // The gold mana symbol. Shows in any text that uses the ManaSymbol sprite asset.
    public const string ManaIcon = "<sprite name=\"Mana\">";

    // 12, 3.5, 1.2K, 45M... short enough to float over an Element.
    public static string Short(double value)
    {
        string[] units = { "", "K", "M", "B", "T", "Qa", "Qi" };
        int unit = 0;
        double size = System.Math.Abs(value);
        while (size >= 1000 && unit < units.Length - 1)
        {
            value /= 1000;
            size /= 1000;
            unit++;
        }

        if (unit == 0) return value.ToString("0.#");
        return value.ToString(size < 10 ? "0.##" : size < 100 ? "0.#" : "0") + units[unit];
    }

    // The whole number with separators (1,250.5), or the short form once it gets too long.
    public static string Number(double value) =>
        System.Math.Abs(value) < 1000000 ? value.ToString("#,0.##") : Short(value);

    // A price with the mana symbol in front of it.
    public static string Price(double value) => ManaIcon + Number(value);
}