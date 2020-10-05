using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using GoToCommands.Lib;
using Microsoft.VisualStudio.Shell;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using System.Collections.Generic;
using System.IO;

namespace GoToCommands.Commands
{
    internal sealed class GoToTestClass
    {
        public const int _commandId = 4130;

        public static readonly Guid _commandSet = new Guid("1eececa1-e0da-4689-bb36-1cfbef669757");

        private readonly AsyncPackage _package;

        private static DTE _dte;

		private static List<String> _codeProjects;

		private static List<String> _testProjects;

        private GoToTestClass(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var cmdId = new CommandID(_commandSet, _commandId);
            var command = new OleMenuCommand(Execute, cmdId);

			if(_dte != null)
				DteUtilities.FillProjects(_dte);

			//InitializeProjectsList();

            command.BeforeQueryStatus += ButtonStatus;

            commandService.AddCommand(command);
        }

		public static GoToTestClass Instance
        {
            get;
            private set;
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE;
            Instance = new GoToTestClass(package, commandService);
        }

        private void ButtonStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_dte == null || !(sender is OleMenuCommand button))
                return;

            var filePath = _dte.ActiveDocument.FullName;
			if (_dte.ActiveDocument.ProjectItem == null)
			{
				button.Visible = false;
				return;
			}
            var projectName = _dte.ActiveDocument.ProjectItem.ContainingProject.Name;
			if (!Utilities.IsInVSProject(projectName))
			{
				button.Visible = false;
				return;
			}
            button.Visible = Utilities.IsHeader(filePath) || Utilities.IsCode(filePath);
            button.Text = Utilities.IsTestProject(projectName) ? "Go To Class" : "Go To Test";
        }

        private void Execute(object sender, EventArgs e)
        {
			ThreadHelper.ThrowIfNotOnUIThread();
			var projectName = _dte.ActiveDocument.ProjectItem.ContainingProject.Name;
			bool test = Utilities.IsTestProject(projectName);
			var projects = test ? DteUtilities.CodeProjects(projectName) : DteUtilities.TestProjects(projectName);
			var files = new List<String>();
			foreach (var project in projects)
				files.AddRange(DteUtilities.Files(project));
			var fileName = Utilities.RemoveExtension(_dte.ActiveDocument.Name);
			var path = test ? Utilities.BestClass(files, fileName) : Utilities.BestTest(files, fileName);
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
				_dte.ExecuteCommand("File.OpenFile", path);
		}

	}
}
