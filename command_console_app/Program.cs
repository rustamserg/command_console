using System;
using command_console;

namespace command_console_app
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			CommandConsole.Init (80, 40, ConsoleColor.Yellow);

			TestApp test = new TestApp ();
	
			CommandConsole.Run ();
		}
	}
}
