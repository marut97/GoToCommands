using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using GoToCommands.Lib;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GoToCommands.Commands
{
    internal sealed class GoToHeaderCode
    {
        public const int _commandId = 4129;

        public static readonly Guid _commandSet = new Guid("1eececa1-e0da-4689-bb36-1cfbef669757");

        private readonly AsyncPackage _package;

        private static DTE _dte;

        private GoToHeaderCode(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));


            var cmdId = new CommandID(_commandSet, _commandId);
            var command = new OleMenuCommand(Execute, cmdId);

            command.BeforeQueryStatus += ButtonStatus;

            commandService.AddCommand(command);
        }

        public static GoToHeaderCode Instance
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
            Instance = new GoToHeaderCode(package, commandService);
        }

        private void ButtonStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_dte == null || !(sender is OleMenuCommand button))
                return;

			CodeFile.set(_dte.ActiveDocument);
            button.Visible = CodeFile.File == CodeFile.FileType.Header || CodeFile.File == CodeFile.FileType.Source;
            button.Text = CodeFile.File == CodeFile.FileType.Source ? "Header File" : "Source File";
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
			if(_dte.ActiveDocument.ProjectItem !=  null && (Utilities.IsInVSProject(_dte.ActiveDocument.ProjectItem.ContainingProject.Name)))
            {
                _dte.ExecuteCommand("EditorContextMenus.CodeWindow.ToggleHeaderCodeFile");
                return;
            }

            var path = Utilities.FindHeaderCode(_dte.ActiveDocument.FullName, _dte.ActiveDocument.Name);
            if (!String.IsNullOrEmpty(path))
                _dte.ExecuteCommand("File.OpenFile", path);
        }
    }
}
