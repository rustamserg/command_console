using System;
using System.Threading;


namespace command_console
{
	public class ProxyConsole : IConsole
	{
		public event OnCommandHandler OnCommand;

		public bool IsAlive { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public ConsoleColor CommandColor { get; set; }

		private Thread m_inputThread;
		private AutoResetEvent m_blockEvent;


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
			m_inputThread = new Thread (Input);
			m_inputThread.Start ();
			IsAlive = true;

			if (isBlocking) {
				m_blockEvent = new AutoResetEvent (false);
				m_blockEvent.WaitOne ();
			}

			IsAlive = true;
		}

		public void Stop()
		{
			IsAlive = false;

			if (m_blockEvent != null)
				m_blockEvent.Set ();

			try {
				if (m_inputThread.IsAlive)
					m_inputThread.Abort();
			}
			catch (Exception) {}
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

		private void Input()
		{
			while (IsAlive) {
				string cmd = Console.ReadLine ();
				if (OnCommand != null)
					OnCommand (cmd);
			}
		}
	}
}

