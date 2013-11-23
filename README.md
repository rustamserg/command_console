command_console
===============

C# console with command line prompt for Windows console applications. It doesn't use any native code and provides simple replacement for standard Console class.

Can be useful for console applications which print logs into standart output stream and use standard input stream to send control command to an application.

Provide next features:

- Separate console prompt line for command
- Copy commands from clipboard
- Commands history
- Multilines output support

## Initialization ##

Main thread is responsible to initialize a console and start it. Once it runs the execution thread is blocked and waits till the console will be closed by external command handler.

	CommandConsole.Init (80, 40, ConsoleColor.Yellow);
	CommandConsole.Run ();

## Command handler ##

Application code can subscribe to _OnCommand_ event to handle commands from input prompt.

	CommandConsole.OnCommand += OnCommand;

For example we can close the console in the handler:

	private void OnCommand(string cmd)
	{
		if (cmd == "quit") {
			CommandConsole.Stop ();
		}
	}

