using System.Security.Cryptography;

namespace ArkhamDisplay;

internal static class Utils
{
    public static string GetSHA1Hash(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return "";
        }

        return GetSHA1Hash(File.ReadAllBytes(filePath));
    }

    public static string GetSHA1Hash(List<string> data)
    {
        string final = string.Concat(data);
        return GetSHA1Hash(Encoding.UTF8.GetBytes(final));
    }

    public static string GetSHA1Hash(byte[] data) =>
        BitConverter.ToString(SHA1.HashData(data));

    public static string ListToNewlinedString(List<string> list)
    {
        string final = "";

        foreach (string s in list)
        {
            final += "\n" + s;
        }

        return final;
    }

    public static Rect DetermineFinalWindowRectPosition(Rect originalRect, double minWidth = 1, double minHeight = 1)
    {
        double screenLeft = SystemParameters.VirtualScreenLeft;
        double screenRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
        double screenTop = SystemParameters.VirtualScreenTop;
        double screenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

        if (originalRect.Width < minWidth || originalRect.Height < minHeight)
        {
            //Input is not valid, return default rect with min sizes
            return new Rect(0.0, 0.0, minWidth, minHeight);
        }

        Rect finalRect;

        finalRect.Width = originalRect.Width;
        finalRect.Height = originalRect.Height;

        if (originalRect.X < screenLeft)
        {
            finalRect.X = screenLeft;
        }
        else if (originalRect.X + originalRect.Width > screenRight)
        {
            finalRect.X = screenRight - originalRect.Width;
        }
        else
        {
            finalRect.X = originalRect.X;
        }

        if (originalRect.Y < screenTop)
        {
            finalRect.Y = screenTop;
        }
        else if (originalRect.Y + originalRect.Height > screenBottom)
        {
            finalRect.Y = screenBottom - originalRect.Height;
        }
        else
        {
            finalRect.Y = originalRect.Y;
        }

        return finalRect;
    }
}