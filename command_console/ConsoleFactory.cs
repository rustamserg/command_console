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

		private static Dictionary<Type, IConsole> m_consoles = new Dictionary<Type, IConsole>();
		
		public static IConsole Get(Type console)
		{
			IConsole cs;
			if (m_consoles.TryGetValue (console, out cs))
				return cs;

			if (console == Type.Command)
				cs = new CommandConsole ();
			else if (console == Type.Proxy)
				cs = new ProxyConsole ();

			m_consoles [console] = cs;
			return cs; 
		}
	}
}

