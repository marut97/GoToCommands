using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GoToHeaderCode.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GoToHeaderCode
    {

        private const int _commandId = 0x0100;

        private static readonly Guid _commandSet = new Guid("5a7dac73-7862-4f29-beae-46cea2d3d188");

        private static DTE _dte;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE;

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as IMenuCommandService;
            Assumes.Present(commandService);
            var cmdId = new CommandID(_commandSet, _commandId);

            var cmd = new OleMenuCommand(Execute, cmdId)
            {
                // This will defer visibility control to the VisibilityConstraints section in the .vsct file
                Supported = false
            };

            commandService.AddCommand(cmd);
        }

        private static void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ProjectItem item = _dte.SelectedItems.Item(1).ProjectItem;
        }
    }
}
