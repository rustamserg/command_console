using System;
using command_console;

namespace command_console_app
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var cs = ConsoleFactory.Get (ConsoleFactory.Type.Command);
			cs.Init (ConsoleColor.Yellow);

			TestApp test = new TestApp (cs);
	
			cs.Run ();
		}
	}
}
