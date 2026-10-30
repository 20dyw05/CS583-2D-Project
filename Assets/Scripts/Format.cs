// How numbers are written in the UI.
public static class Format
{
    // The gold mana symbol. Shows in any text that uses the ManaSymbol sprite asset.
    public const string ManaIcon = "<sprite name=\"Mana\">";

    // Short form, small enough to float over an Element: 12, 3.5, 1.2K, 45M...
    public static string Short(double value)
    {
        string[] units = { "", "K", "M", "B", "T", "Qa", "Qi" };

        // Divide by 1000 until the number is under 1000, moving up a unit each time.
        int unit = 0;
        double size = System.Math.Abs(value);
        while (size >= 1000 && unit < units.Length - 1)
        {
            value /= 1000;
            size /= 1000;
            unit++;
        }

        if (unit == 0)
        {
            return value.ToString("0.#");
        }

        // Fewer decimals for bigger numbers, so it's always about three digits: 1.25K, 12.5K, 125K.
        string pattern;
        if (size < 10)
        {
            pattern = "0.##";
        }
        else if (size < 100)
        {
            pattern = "0.#";
        }
        else
        {
            pattern = "0";
        }
        return value.ToString(pattern) + units[unit];
    }

    // The whole number with separators (1,250.5), or the short form once it gets too long.
    public static string Number(double value)
    {
        if (System.Math.Abs(value) < 1000000)
        {
            return value.ToString("#,0.##");
        }
        return Short(value);
    }

    // A price with the mana symbol in front of it.
    public static string Price(double value)
    {
        return ManaIcon + Number(value);
    }
}