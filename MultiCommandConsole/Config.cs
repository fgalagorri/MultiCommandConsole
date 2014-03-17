using System;
using MultiCommandConsole.Commands;
using MultiCommandConsole.Util;

namespace MultiCommandConsole
{
	public static class Config
	{
	    private static Type _defaultCommand = typeof(HelpCommand);

	    /// <summary>
        /// When not specified or set to null, the help command becomes the default command.
        /// </summary>
        public static Type DefaultCommand
	    {
	        get { return _defaultCommand; }
	        set { _defaultCommand = value ?? typeof(HelpCommand); }
	    }

	    /// <summary>
		/// By default, commands and arg sets are created by Activator.  
		/// Use this delegate to override the default behavior. 
		/// </summary>
		public static Func<Type, object> ResolveTypeDelegate { get; set; }

		/// <summary>
		/// Indicates a command is about to run.  
		/// <hint>This is where you should refresh any DI Container lifecycles</hint>.
		/// </summary>
		public static Action<string[], Action<string[]>> RunCommand { get; set; }

		/// <summary>
		/// Indicates a command has finished running.  
		/// <hint>This is where you should cleanup any DI Container lifecycles</hint>.
		/// </summary>
		public static Action EndRunCommand { get; set; }

		/// <summary>The console formatter used to format messages for the console window</summary>
		public static ConsoleFormatter ConsoleFormatter { get; set; }


        public static class ConsoleMode
        {
            /// <summary>
            /// If true, an additional command will be displayed to allow the user to enter interactive console mode
            /// </summary>
            public static bool Enabled { get; set; }

            /// <summary>
            /// The prompt text displayed before the prompt
            /// </summary>
            public static string CommandPromptText { get; set; }

            /// <summary>The name of the application.  Will be used to store console history.</summary>
            public static string AppName { get; set; }

            /// <summary>
            /// The number of entries to keep in history
            /// </summary>
            public static int? HistorySize { get; set; }
        }

		/// <summary>
		/// If true, an additional command will be displayed to allow the user to see how the text they 
		/// enter is converted into arguments.  Useful when arguments need to escape quotes.
		/// </summary>
		public static bool ShowVierArgsCommand { get; set; }

		/// <summary>
		/// In a standard dos console, Escape clears the current line. 
		/// Setting EscapeIsAltKey to true will cause Escape to trigger the Alt modifier 
		/// instead of clearing the current line.  This is similar to some *nix consoles.
		/// </summary>
		public static bool EscapeIsAltKey { get; set; }

		static Config()
		{
			ConsoleFormatter = new ConsoleFormatter();
			ShowVierArgsCommand = false;
		    ResolveTypeDelegate = Activator.CreateInstance;

            ConsoleMode.CommandPromptText = "$";
		    ConsoleMode.HistorySize = 50;
		}
	}
}