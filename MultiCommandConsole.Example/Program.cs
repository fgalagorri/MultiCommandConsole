﻿using System;
using System.Diagnostics;
using Common.Logging;
using MultiCommandConsole.Services;
using MultiCommandConsole.Util;
using ObjectPrinter;

namespace MultiCommandConsole.Example
{
	class Program
    {
        private static readonly IConsoleWriter Writer = ConsoleWriter.Get<Program>();

		static void Main(string[] args)
		{
            if (Array.Exists(args, s => s == "/debug") && Environment.UserInteractive)
            {
                Debugger.Break();
            }

            LogManager.Adapter = Log4NetFactoryAdapter.Load();
		    var logger = LogManager.GetLogger(typeof (Program));
		    logger.InfoFormat("Logging configured");

            Config.ConsoleMode.Enabled = true;
			Config.ConsoleMode.CommandPromptText = "console";
		    Config.ConsoleMode.HistorySize = 5;
		    Config.ConsoleMode.AppName = "example_console";
			Config.ShowViewArgsCommand = true;
		    Config.WriteRunTimeToConsole = true;
		    //Config.DefaultCommand = typeof (RepeatConsoleCommand);
		    Config.Help.GetCategoryDelegate = (s, type) =>
		        {
		            if (s.EndsWith("-service"))
		            {
		                return " - service -";
		            }
                    if (type.Assembly == typeof (Program).Assembly)
                    {
                        return " - example -";
                    }
		            return "";
                };
            Config.Help.GetAddlHelpDelegate = (s, type) => new[] { type.FullName };

            Services.Config.Defaults.CommandLine = "ping /s=google.com /i=10";
            Services.Config.EnableServiceMode();

		    try
		    {
		        new Engine(new[]
		            {
		                typeof(Program).Assembly, 
		                typeof(InstallServiceCommand).Assembly
		            }).Run(args);
		    }
		    catch (Exception e)
		    {
		        Writer.WriteLine(e.Dump());
		    }
		}
	}
}
