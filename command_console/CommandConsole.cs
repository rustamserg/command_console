using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace command_console
{
	public static class CommandConsole
	{
		public delegate void OnCommandHandler (string cmd);
		public static event OnCommandHandler OnCommand;

		public static bool IsAlive { get; set; }
		public static int Width { get; private set; }
		public static int Height { get; private set; }
		public static ConsoleColor CommandColor { get; set; }


		private static readonly int BUF_SIZE = 1024;
		private static readonly int HISTORY_SIZE = 100;
		private static string[] m_buffer = new string[BUF_SIZE];
		private static string[] m_history = new string[HISTORY_SIZE];
		private static int m_historyCursor = HISTORY_SIZE - 1;


		public static void Init(int width, int height, ConsoleColor cmdColor = ConsoleColor.White)
		{
			Width = width;
			Height = height;
			CommandColor = cmdColor;

			Console.SetWindowSize (Width, Height);
			Console.SetBufferSize (Width, Height);
			Console.CursorVisible = false;

			for (int i = 0; i < BUF_SIZE; i++) {
				m_buffer [i] = string.Empty;
			}
			for (int i = 0; i < HISTORY_SIZE; i++) {
				m_history [i] = string.Empty;
			}
		}

		public static void Run()
		{
			Thread inputThread = new Thread (Input);
			inputThread.SetApartmentState(ApartmentState.STA); 
			inputThread.Start ();
			IsAlive = true;

			while (IsAlive) {
				Thread.Sleep (10);
			}

			try {
				inputThread.Abort();
			}
			catch (Exception) {}
		}

		public static void Stop()
		{
			IsAlive = false;
		}

		public static void Write(string line)
		{
			if (!IsAlive)
				return;

			AppendToBuffer (line);
		}

		public static void Write(string format, params object[] args)
		{
			if (!IsAlive)
				return;

			var line = string.Format (format, args);
			AppendToBuffer (line);
		}

		public static void WriteLine(string line)
		{
			if (!IsAlive)
				return;

			AddToBuffer(line);
		}

		public static void WriteLine(string format, params object[] args)
		{
			if (!IsAlive)
				return;

			var line = string.Format (format, args);
			AddToBuffer (line);
		}	
		
		private static void Input()
		{
			while (true) {
				var cmd = string.Empty;
				DrawCmdBuffer (cmd);

				while (true) {
					var keyInfo = Console.ReadKey ();
					if (keyInfo.Key == ConsoleKey.Enter)
						break;

					if (keyInfo.Key == ConsoleKey.Backspace)
						cmd = cmd.Length > 0 ? cmd.Remove (Math.Max (0, cmd.Length - 1)) : cmd;
					else if (keyInfo.Key == ConsoleKey.UpArrow)
						cmd = GetNextHistoryCmd ();
					else if (keyInfo.Key == ConsoleKey.DownArrow)
						cmd = GetPrevHistoryCmd ();
					else if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 && keyInfo.Key == ConsoleKey.V)
						cmd = Clipboard.GetText ();
					else
						cmd += keyInfo.KeyChar.ToString ();

					DrawCmdBuffer (cmd);
				}

				cmd = cmd.Trim ();
				if (string.IsNullOrEmpty (cmd))
					continue;

				ProcessCmd (cmd);

				if (!IsAlive)
					break;
			}
		}
		
		private static void ProcessCmd(string cmd)
		{
			if (OnCommand != null)
				OnCommand (cmd);

			AddToHistoryCmd(cmd);
		}

		private static void DrawCmdBuffer(string cmd)
		{
			lock (m_buffer) {
				Console.SetCursorPosition (0, Height - 1);
				var oldColor = Console.ForegroundColor;
				Console.ForegroundColor = CommandColor;
				Console.Write (cmd.PadRight(Width - 1).Remove(Width - 2));
				Console.ForegroundColor = oldColor;
			}
		}

		private static string GetNextHistoryCmd()
		{
			var cmd = m_history [m_historyCursor];
			if (!string.IsNullOrEmpty(cmd))
				m_historyCursor = Math.Max(0, m_historyCursor - 1);
			return cmd;
		}

		private static string GetPrevHistoryCmd()
		{
			var cmd = m_history [m_historyCursor];
			m_historyCursor = Math.Min (HISTORY_SIZE - 1, m_historyCursor + 1);
			return cmd;
		}

		private static void AddToHistoryCmd(string cmd)
		{
			for (int idx = 0; idx < HISTORY_SIZE - 1; idx++) {
				m_history [idx] = m_history [idx + 1];
			}
			m_history [HISTORY_SIZE - 1] = cmd;
		}

		private static void AppendToBuffer(string line)
		{
			lock (m_buffer) {
				var merged = m_buffer [BUF_SIZE - 1] + line;
				var lines = merged.Replace("\r", "").Split ('\n');

				m_buffer [BUF_SIZE - 1] = lines [0];
				for (int i = 1; i < lines.Length; i++) {
					AddToBuffer (lines [i]);
				}
				DrawBuffer ();
			}
		}

		private static void AddToBuffer(string line)
		{
			lock (m_buffer) {
				var lines = line.Replace("\r", "").Split ('\n');
				for (int i = 0; i < lines.Length; i++) {				
					for (int idx = 0; idx < BUF_SIZE - 1; idx++) {
						m_buffer [idx] = m_buffer [idx + 1];
					}
					m_buffer [BUF_SIZE - 1] = lines [i];
				}
				DrawBuffer ();
			}
		}

		private static void DrawBuffer()
		{
			lock (m_buffer) {
				int bufPos = BUF_SIZE - (Height - 1);

				Console.SetCursorPosition (0, 0);

				for (int pos = bufPos; pos < BUF_SIZE; pos++) {
					Console.WriteLine (m_buffer [pos].PadRight (Width - 1).Remove (Width - 2));
				}
			}
		}
	}
}

