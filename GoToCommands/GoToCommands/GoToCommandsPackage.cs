using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using GoToCommands.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace GoToCommands
{

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GoToCommandsPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideService(typeof(IMenuCommandService), IsAsyncQueryable = true)]
    [ProvideAutoLoad(UiConstraintGuidString, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideUIContextRule(UiConstraintGuidString, // Must match the GUID in the .vsct file
        name: "UI Context",
        expression: "header | code", // This will make the button only show for .cpp and .h files
        termNames: new[] { "header", "code" },
        termValues: new[] { "HierSingleSelectionName:.h$", "HierSingleSelectionName:.cpp$" })]
    public sealed class GoToCommandsPackage : AsyncPackage
    {
        public const string PackageGuidString = "20bc4cc8-d316-4f4a-9f46-ec8f3fe76d4d";
        public const string UiConstraintGuidString = "6B4C995A-47FD-4461-90A2-2048B531EBE1";

        public GoToCommandsPackage()
        {
        }

        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await GoToHeaderCode.InitializeAsync(this);
            await GoToTestClass.InitializeAsync(this);
            //await GoToImplementation.InitializeAsync(this);
        }

        #endregion
    }
}
