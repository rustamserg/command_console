using System;
using System.Threading;
using command_console;

namespace command_console_app
{
	public class TestApp
	{
		private readonly Thread m_thread;
		private readonly IConsole m_console;

		public TestApp (IConsole console)
		{
			m_thread = new Thread (DoWork);
			m_thread.Start ();

			m_console = console;
			m_console.OnCommand += OnCommand;
		}

		private void DoWork()
		{
			while (true) {
				Thread.Sleep (2000);

				m_console.Write ("Current time in UTC {0}", DateTime.UtcNow.ToString ());
				m_console.WriteLine ("  Local time {0}", DateTime.Now.ToString());	 
			}
		}

		private void OnCommand(string cmd)
		{
			if (cmd == "quit") {
				m_thread.Abort ();
				m_console.Stop ();
			}
		}
	}
}

