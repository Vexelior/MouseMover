# MouseMover

[![License](https://img.shields.io/github/license/Vexelior/MouseMover)](LICENSE) [![GitHub stars](https://img.shields.io/github/stars/Vexelior/MouseMover?style=social)](https://github.com/Vexelior/MouseMover/stargazers) [![.NET](https://img.shields.io/badge/dotnet-net10.0-blue)](https://dotnet.microsoft.com/) [![Build](https://img.shields.io/github/actions/workflow/status/Vexelior/MouseMover/ci.yml?branch=main)](https://github.com/Vexelior/MouseMover/actions)

MouseMover is a small, single-file .NET console utility that periodically nudges the mouse cursor to prevent the OS from considering the system idle. It's useful for keeping a workstation or a virtual machine active during long-running tasks where you don't want the screen saver, lock, or sleep behavior to trigger.

## Key features

- Simple interactive console application (no external configuration files required).
- Periodically moves the mouse a tiny offset and returns it to its original position.
- Configurable runtime (minutes) or run indefinitely until manually stopped.
- Configurable move interval (seconds).
- Windows-specific cursor movement; on non-Windows platforms the program will gracefully skip moving the cursor.

## How it works (brief)

The program prompts you for two values:

- Desired run time (in minutes) — leave empty to run forever.
- Move interval (in seconds) — how often to nudge the mouse.

When running on Windows the program calls the native Win32 APIs `GetCursorPos` and `SetCursorPos` to move the cursor by a single pixel and then immediately move it back (a short delay of a few dozen milliseconds is used to make the move effective). On non-Windows platforms the program will simply continue but not attempt cursor movement.

The implementation lives in `App/Program.cs` and the project file is `App/App.csproj` (targets `net10.0`).

## Requirements

- .NET SDK capable of building the project (the project targets `net10.0`; check `App/App.csproj` for the exact target).
- Windows to actually move the cursor. On Linux/macOS the program will run but won't perform OS-level cursor movement.

## Build and run

Open a terminal at the repository root (this project layout places the app in the `App/` subfolder).

Build the project:

```bash
dotnet build ./App -c Release
```

Run the app (interactive):

```bash
dotnet run --project ./App
```

Notes:

- The program will prompt for a run duration in minutes. Provide a positive integer or press Enter to run indefinitely.
- It will then prompt for the move interval in seconds. Press Enter to accept the default (30 seconds).
- Press any key to start the session and press Ctrl+C to cancel/stop early.

## Configuration defaults and behavior

The following default values are defined in `App/Program.cs`:

- Default move interval: 30 seconds
- Mouse move offset: 1 pixel
- Mouse move delay: 50 ms

Edge cases handled by the app:

- Non-numeric or non-positive input for duration or interval will be rejected and the program will ask you to restart with valid input.
- When running on non-Windows platforms the program will continue but skip native cursor calls.

## Example session

1. Start the app: `dotnet run --project ./App`
2. Enter `120` when prompted for run time (the app will run for 120 minutes), or press Enter to run forever.
3. Enter `60` when prompted for the interval to move every 60 seconds, or press Enter to accept the default.
4. Press any key to begin. The console displays move count and time remaining.

## Troubleshooting

- If the cursor does not move on Windows, ensure the console is running with sufficient privileges (should not normally be necessary). Also check other accessibility or security settings that may prevent synthetic cursor movement.
- If `dotnet` commands fail, ensure you have an appropriate .NET SDK installed and that your PATH includes the `dotnet` executable.

## Contributing

Contributions are welcome. Keep changes focused and small. If you plan to add features (for example, command-line arguments, a background service installer, or broader OS support), open an issue or a pull request describing the change first.

Suggested low-risk improvements:

- Add optional CLI flags to configure run time and interval (non-interactive mode).
- Add unit tests for helper formatting functions.
- Add platform detection and optional support for macOS/Linux cursor movement using appropriate APIs.

## License

This repository includes a `LICENSE` file at the project root. Review that file for the project's license terms.

## Files of interest

- `App/Program.cs` — main application logic and interactive prompts.
- `App/App.csproj` — project file (target framework and build settings).
