//------------------------------------------------------------------------------
// <copyright file="GoToTest.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using GoToCommands.Lib;

namespace GoToCommands.Commands
{
	internal sealed class GoToTestClass
	{
		public const int CommandId = 4131;

		public static readonly Guid CommandSet = new Guid("18a02811-3efc-4164-ba29-3bf12ab847ad");

		private readonly Package _package;

		private static DTE _dte;

		private static bool _testFolder;

		private GoToTestClass(Package package)
		{
			_package = package ?? throw new ArgumentNullException(nameof(package));
			OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			_dte = ServiceProvider.GetService(typeof(DTE)) as DTE;

			var cmdID = new CommandID(CommandSet, CommandId);
			var command = new OleMenuCommand(Execute, cmdID);

			command.BeforeQueryStatus += MyQueryStatus;

			commandService.AddCommand(command);
		}

		public static GoToTestClass Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider
		{
			get { return _package; }
		}

		public static void Initialize(Package package)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			Instance = new GoToTestClass(package);
		}

		private void MyQueryStatus(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var button = sender as OleMenuCommand;
			_testFolder = _dte.ActiveDocument.FullName.Contains("\\test\\");
			button.Text =  _testFolder ? "Go To Class" : "Go To Test";
		}

		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			string path = _testFolder ? Utilities.FindClass(_dte.ActiveDocument.FullName, _dte.ActiveDocument.Name) : Utilities.FindTest(_dte.ActiveDocument.FullName, _dte.ActiveDocument.Name);
			if (!string.IsNullOrEmpty(path))
				_dte.ExecuteCommand("File.OpenFile", path);
		}
	}
}
