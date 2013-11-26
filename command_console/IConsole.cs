using System;

namespace command_console
{
	public delegate void OnCommandHandler (string cmd);

	public interface IConsole
	{
		event OnCommandHandler OnCommand;

		bool IsAlive { get; }
		int Width { get; }
		int Height { get; }
		ConsoleColor CommandColor { get; set; }

		void Init(ConsoleColor cmdColor = ConsoleColor.White);
		void Init(int width, int height, ConsoleColor cmdColor = ConsoleColor.White);
		void Run(bool isBlocking = false);
		void Stop();

		void Write (string line);
		void Write (string format, params object[] args);
		void WriteLine (string line);
		void WriteLine (string format, params object[] args);
	}
}

