using System;
using System.Collections.Generic;

namespace command_console
{
	public static class ConsoleFactory
	{
		public enum Type
		{
			Command,
			Proxy
		}

		public static IConsole Console { get; private set; }

		public static IConsole Create(Type console)
		{
			if (Console != null)
				return Console;

			if (console == Type.Command)
				Console = new CommandConsole ();
			else if (console == Type.Proxy)
				Console = new ProxyConsole ();

			return Console; 
		}
	}
}

