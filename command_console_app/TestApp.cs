using System;
using System.Threading;
using command_console;

namespace command_console_app
{
	public class TestApp
	{
		private Thread m_thread;

		public TestApp ()
		{
			m_thread = new Thread (DoWork);
			m_thread.Start ();

			CommandConsole.OnCommand += OnCommand;
		}

		private void DoWork()
		{
			while (true) {
				Thread.Sleep (100);
				CommandConsole.WriteLine ("Current time in UTC {0}", DateTime.UtcNow.ToString ());
				CommandConsole.Write ("  Local time {0}", DateTime.Now.ToString());	 
			}
		}

		private void OnCommand(string cmd)
		{
			if (cmd == "quit") {
				CommandConsole.Stop ();
				m_thread.Abort ();
			}
		}
	}
}

