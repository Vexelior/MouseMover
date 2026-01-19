using System.Runtime.InteropServices;

const int DEFAULT_MOVE_INTERVAL_SECONDS = 30;
const int MOUSE_MOVE_OFFSET_PIXELS = 1;
const int MOUSE_MOVE_DELAY_MS = 50;
const int LOOP_CHECK_INTERVAL_MS = 100;

Console.WriteLine("=== MouseMover ===");

int runTime;
bool runForever;
try
{
    (runTime, runForever) = GetRunTimeConfiguration();
}
catch (InvalidOperationException)
{
    return;
}

int interval;
try
{
    interval = GetMoveIntervalConfiguration();
}
catch (InvalidOperationException)
{
    return;
}

Console.WriteLine("Press any key to start the mouse mover...");
Console.ReadKey();
Console.WriteLine();

Console.WriteLine("Mouse mover is now running. Press Ctrl + C to stop the program.");
Console.WriteLine();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    RunMouseMover(runTime, runForever, interval, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine();
    Console.WriteLine("Program stopped by user.");
}

return;

static (int runTime, bool runForever) GetRunTimeConfiguration()
{
    Console.WriteLine("Please enter your desired run time (in minutes) [leave empty to run forever]:");
    string? desiredTime = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(desiredTime))
    {
        Console.WriteLine("The program will run forever (until manually stopped).");
        return (0, true);
    }
    
    if (!int.TryParse(desiredTime, out int runTime))
    {
        Console.WriteLine("Invalid input. Please enter a valid number.");
        throw new InvalidOperationException();
    }
    
    if (runTime <= 0)
    {
        Console.WriteLine("Please enter a positive number for the run time.");
        throw new InvalidOperationException();
    }
    
    Console.WriteLine(FormatDurationMessage(runTime, "The program will run for"));
    return (runTime, false);
}

static int GetMoveIntervalConfiguration()
{
    Console.WriteLine($"Please enter how often you would like the mouse to move (in seconds) [default: {DEFAULT_MOVE_INTERVAL_SECONDS}]:");
    string? moveInterval = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(moveInterval))
    {
        Console.WriteLine($"Using default interval: {DEFAULT_MOVE_INTERVAL_SECONDS} second(s).");
        return DEFAULT_MOVE_INTERVAL_SECONDS;
    }
    
    if (!int.TryParse(moveInterval, out int interval))
    {
        Console.WriteLine("Invalid input. Please enter a valid number.");
        throw new InvalidOperationException();
    }
    
    if (interval <= 0)
    {
        Console.WriteLine("Please enter a positive number for the move interval.");
        throw new InvalidOperationException();
    }
    
    Console.WriteLine(FormatIntervalMessage(interval));
    return interval;
}

static string FormatDurationMessage(int totalMinutes, string prefix)
{
    if (totalMinutes >= 60)
    {
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        return minutes == 0
            ? $"{prefix} {hours} hour(s)."
            : $"{prefix} {hours} hour(s) and {minutes} minute(s).";
    }
    return $"{prefix} {totalMinutes} minute(s).";
}

static string FormatIntervalMessage(int totalSeconds)
{
    if (totalSeconds > 60)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"The mouse will move every {minutes} minute(s) and {seconds} second(s).";
    }
    return $"The mouse will move every {totalSeconds} second(s).";
}

static string FormatElapsedTime(TimeSpan elapsed)
{
    int elapsedHours = (int)elapsed.TotalHours;
    int elapsedMinutes = elapsed.Minutes;
    int elapsedSeconds = elapsed.Seconds;

    if (elapsedHours > 0)
        return $"{elapsedHours}h {elapsedMinutes}m {elapsedSeconds}s";
    if (elapsedMinutes > 0)
        return $"{elapsedMinutes}m {elapsedSeconds}s";
    return $"{elapsedSeconds}s";
}

static string FormatRemainingTime(TimeSpan remaining)
{
    int remainingMinutes = (int)remaining.TotalMinutes;
    int remainingSeconds = remaining.Seconds;

    if (remainingMinutes > 60)
    {
        int hours = remainingMinutes / 60;
        int minutes = remainingMinutes % 60;
        return $"{hours} hour(s) and {minutes} minute(s)";
    }
    if (remainingMinutes > 0)
        return $"{remainingMinutes} minute(s)";
    return $"{remainingSeconds} second(s)";
}

static void RunMouseMover(int runTime, bool runForever, int interval, CancellationToken cancellationToken)
{
    int moveCount = 0;
    DateTime startTime = DateTime.Now;
    DateTime? endTime = runForever ? null : startTime.AddMinutes(runTime);

    while (!cancellationToken.IsCancellationRequested)
    {
        if (!runForever && DateTime.Now >= endTime!.Value)
        {
            break;
        }

        if (!MoveMouse())
        {
            Console.WriteLine();
            Console.WriteLine("Warning: Failed to move mouse cursor. Continuing...");
        }
        
        moveCount++;

        string statusMessage = runForever
            ? $"\rMove Count: {moveCount} | Running for: {FormatElapsedTime(DateTime.Now - startTime)}"
            : $"\rMove Count: {moveCount} | Time remaining: {FormatRemainingTime(endTime!.Value - DateTime.Now)}";
        
        Console.Write(statusMessage);

        DateTime nextMoveTime = DateTime.Now.AddSeconds(interval);
        while (DateTime.Now < nextMoveTime && !cancellationToken.IsCancellationRequested)
        {
            if (!runForever && DateTime.Now >= endTime!.Value)
            {
                break;
            }
            Thread.Sleep(LOOP_CHECK_INTERVAL_MS);
        }
    }

    Console.WriteLine();
    Console.WriteLine("=== Session Complete ===");
    Console.WriteLine($"Total movements: {moveCount}");

    if (runForever)
    {
        TimeSpan totalElapsed = DateTime.Now - startTime;
        Console.WriteLine($"Runtime: {(int)totalElapsed.TotalMinutes} minute(s)");
    }
    else
    {
        Console.WriteLine($"Runtime: {runTime} minute(s)");
    }

    Console.WriteLine("Program has finished. Press any key to exit...");
    Console.ReadKey();
}

static bool MoveMouse()
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        return true;
    }

    if (!GetCursorPos(out POINT currentPos))
    {
        return false;
    }

    SetCursorPos(currentPos.X + MOUSE_MOVE_OFFSET_PIXELS, currentPos.Y);
    Thread.Sleep(MOUSE_MOVE_DELAY_MS);
    SetCursorPos(currentPos.X, currentPos.Y);
    
    return true;
}

[DllImport("user32.dll")]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool SetCursorPos(int x, int y);

[DllImport("user32.dll")]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool GetCursorPos(out POINT lpPoint);

[StructLayout(LayoutKind.Sequential)]
struct POINT
{
    public int X;
    public int Y;
}