using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace command_console
{
	public class CommandConsole : IConsole
	{
		public event OnCommandHandler OnCommand;

		public bool IsAlive { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public ConsoleColor CommandColor { get; set; }

		class ConsoleLine {
			public ConsoleColor Color;
			public string Line;

			public ConsoleLine() :this(Console.ForegroundColor, string.Empty) {} 
			public ConsoleLine(string ln) : this(Console.ForegroundColor, ln) {}

			public ConsoleLine(ConsoleColor clr, string ln)
			{
				Color = clr;
				Line = ln;
			}

			public static readonly ConsoleLine Empty = new ConsoleLine();
		}

		private static readonly int BUF_SIZE = 1024;
		private static readonly int HISTORY_SIZE = 100;
		private ConsoleLine[] m_buffer = new ConsoleLine[BUF_SIZE];
		private string[] m_history = new string[HISTORY_SIZE];
		private int m_historyCursor = HISTORY_SIZE - 1;
		private Thread m_inputThread;
		private AutoResetEvent m_blockEvent;
		private bool m_isNewLine = false;
		private int m_bufPos = BUF_SIZE;

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
			Console.CursorVisible = false;

			for (int i = 0; i < BUF_SIZE; i++) {
				m_buffer [i] = ConsoleLine.Empty;
			}
			for (int i = 0; i < HISTORY_SIZE; i++) {
				m_history [i] = string.Empty;
			}
		}

		public void Run(bool isBlocking)
		{
			m_inputThread = new Thread (Input);
			m_inputThread.SetApartmentState(ApartmentState.STA); 
			m_inputThread.Start ();
			IsAlive = true;

			if (isBlocking) {
				m_blockEvent = new AutoResetEvent (false);
				m_blockEvent.WaitOne ();
			}
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

		public void Write(string line)
		{
			if (!IsAlive)
				return;

			AppendToBuffer (new ConsoleLine(line));
		}

		public void Write(string format, params object[] args)
		{
			if (!IsAlive)
				return;

			var line = string.Format (format, args);
			AppendToBuffer (new ConsoleLine(line));
		}

		public void WriteLine(string line)
		{
			if (!IsAlive)
				return;

			AddToBuffer(new ConsoleLine(line));
		}

		public void WriteLine(string format, params object[] args)
		{
			if (!IsAlive)
				return;

			var line = string.Format (format, args);
			AddToBuffer (new ConsoleLine(line));
		}

		public void Write (string line, ConsoleColor lineColor)
		{
			if (!IsAlive)
				return;

			AppendToBuffer (new ConsoleLine (lineColor, line));
		}

		public void Write (string format, ConsoleColor lineColor, params object[] args)
		{
			if (!IsAlive)
				return;

			var line = string.Format (format, args);
			AppendToBuffer (new ConsoleLine (lineColor, line));
		}

		public void WriteLine (string line, ConsoleColor lineColor)
		{
			if (!IsAlive)
				return;

			AddToBuffer (new ConsoleLine (lineColor, line));
		}

		public void WriteLine (string format, ConsoleColor lineColor, params object[] args)
		{
			if (!IsAlive)
				return;

			var line = string.Format (format, args);
			AddToBuffer (new ConsoleLine (lineColor, line));
		}

		private void Input()
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
					else if (keyInfo.Key == ConsoleKey.PageUp)
						ScrollBufferUp ();
					else if (keyInfo.Key == ConsoleKey.PageDown)
						ScrollBufferDown ();
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
		
		private void ProcessCmd(string cmd)
		{
			if (OnCommand != null)
				OnCommand (cmd);

			AddToHistoryCmd(cmd);
		}

		private void DrawCmdBuffer(string cmd)
		{
			lock (m_buffer) {
				Console.SetCursorPosition (0, Height - 1);
				ApplyColorAspect (CommandColor, () => Console.Write (cmd.PadRight (Width - 1).Remove (Width - 2)));
			}
		}

		private string GetNextHistoryCmd()
		{
			var cmd = m_history [m_historyCursor];
			if (!string.IsNullOrEmpty(cmd))
				m_historyCursor = Math.Max(0, m_historyCursor - 1);
			return cmd;
		}

		private string GetPrevHistoryCmd()
		{
			var cmd = m_history [m_historyCursor];
			m_historyCursor = Math.Min (HISTORY_SIZE - 1, m_historyCursor + 1);
			return cmd;
		}

		private void AddToHistoryCmd(string cmd)
		{
			if (m_history.Any (e => e == cmd))
				return;

			for (int idx = 0; idx < HISTORY_SIZE - 1; idx++) {
				m_history [idx] = m_history [idx + 1];
			}
			m_history [HISTORY_SIZE - 1] = cmd;
		}

		private void AppendToBuffer(ConsoleLine line)
		{
			lock (m_buffer) {
				if (m_isNewLine) {
					PushToBuffer (ConsoleLine.Empty);
					m_isNewLine = false;
				}

				var merged = m_buffer [BUF_SIZE - 1].Line + line.Line;
				var lines = SplitLine(merged);

				m_buffer [BUF_SIZE - 1] = new ConsoleLine(line.Color, lines [0]);
				m_isNewLine = lines.Length > 1;

				for (int i = 1; i < lines.Length; i++) {
					PushToBuffer (new ConsoleLine(line.Color, lines [i]));
				}
				DrawBuffer ();
			}
		}

		private void AddToBuffer(ConsoleLine line)
		{
			lock (m_buffer) {
				if (m_isNewLine) {
					PushToBuffer (ConsoleLine.Empty);
					m_isNewLine = false;
				}

				var merged = m_buffer [BUF_SIZE - 1].Line + line.Line;
				var lines = SplitLine(merged);

				m_buffer [BUF_SIZE - 1] = new ConsoleLine(line.Color, lines [0]);
				m_isNewLine = true;

				for (int i = 1; i < lines.Length; i++) {
					PushToBuffer (new ConsoleLine(line.Color, lines [i]));
				}
				DrawBuffer ();
			}
		}

		private string[] SplitLine(string line)
		{
			var lines = new List<string> ();
			var rawlines = line.Replace("\r", "").Split ('\n');

			foreach (var rl in rawlines) {
				int idx = 0;
				while (true) {
					var ln = rl.Substring(idx, Math.Min(Width - 1, rl.Length - idx));
					if (ln != string.Empty) {
						lines.Add (ln);
						idx += ln.Length;
					} else
						break; 
				}
			}
			return lines.ToArray ();
		}

		private void PushToBuffer(ConsoleLine line)
		{
			for (int idx = 0; idx < BUF_SIZE - 1; idx++) {
				m_buffer [idx] = m_buffer [idx + 1];
			}
			m_buffer [BUF_SIZE - 1] = line;
		}


		private void ScrollBufferUp()
		{
			m_bufPos = Math.Max (Height - 1, m_bufPos - (Height - 1));
			DrawBuffer ();
		}

		private void ScrollBufferDown()
		{
			m_bufPos = Math.Min (BUF_SIZE, m_bufPos + (Height - 1));
			DrawBuffer ();
		}

		private void DrawBuffer()
		{
			lock (m_buffer) {
				int bufOffset = m_bufPos - (Height - 1);

				Console.SetCursorPosition (0, 0);

				for (int idx = 0; idx < (Height - 1); idx++) {
					var line = m_buffer [bufOffset + idx];
					ApplyColorAspect (line.Color, () => Console.WriteLine (line.Line.PadRight(Width - 1)));
				}
			}
		}

		private void ApplyColorAspect(ConsoleColor clr, Action action)
		{
			var curClr = Console.ForegroundColor;
			Console.ForegroundColor = clr;
			action ();
			Console.ForegroundColor = curClr;
		}
	}
}

