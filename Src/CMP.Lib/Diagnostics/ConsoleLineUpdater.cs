namespace CMP.Lib.Diagnostics;

public sealed class ConsoleLineUpdater
{
    private int _lastLength;

    public void Update(string text)
    {
        // Move to start of line
        Console.Write('\r');

        // Write text
        Console.Write(text);

        // Clear leftover characters from previous write
        int padding = _lastLength - text.Length;
        if (padding > 0)
        {
            Console.Write(new string(' ', padding));
        }

        _lastLength = text.Length;
    }

    public void Clear()
    {
        // Move to start of line
        Console.Write('\r');

        // Clear leftover characters from previous write
        int padding = _lastLength - 0;
        if (padding > 0)
        {
            Console.Write(new string(' ', padding));
        }

        // Move to start of line
        Console.Write('\r');
    }

    public void Finish()
    {
        Console.WriteLine();
        _lastLength = 0;
    }
}
