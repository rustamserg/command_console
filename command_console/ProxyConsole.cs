using System;

namespace command_console
{
	public class ProxyConsole : IConsole
	{
		public event OnCommandHandler OnCommand;

		public bool IsAlive { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public ConsoleColor CommandColor { get; set; }

		public void Init(ConsoleColor cmdColor)
		{
			Width = Console.WindowWidth;
			Height = Console.WindowHeight;
			Init(Width, Height, cmdColor);
		}

		public void Init(int width, int height, ConsoleColor cmdColor)
		{
			Width = width;
			Height = height;
			CommandColor = cmdColor;

			Console.SetWindowSize (Width, Height);
			Console.SetBufferSize (Width, Height);
		}

		public void Run(bool isBlocking)
		{
			IsAlive = true;

			while (IsAlive) {
				string cmd = Console.ReadLine ();
				if (OnCommand != null)
					OnCommand (cmd);
			}
		}

		public void Stop()
		{
			IsAlive = false;
		}

		public void Write (string line)
		{
			if (!IsAlive)
				return;

			Console.Write (line);
		}

		public void Write (string format, params object[] args)
		{
			if (!IsAlive)
				return;

			Console.Write (format, args);
		}

		public void WriteLine (string line)
		{
			if (!IsAlive)
				return;

			Console.WriteLine (line);
		}

		public void WriteLine (string format, params object[] args)
		{
			if (!IsAlive)
				return;

			Console.WriteLine (format, args);
		}
	}
}

