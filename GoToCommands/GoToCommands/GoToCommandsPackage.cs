//------------------------------------------------------------------------------
// <copyright file="GoToCommandsPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using GoToCommands.Commands;
using Task = System.Threading.Tasks.Task;
using System.Threading;

namespace GoToCommands
{

	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(GoToCommandsPackage.PackageGuidString)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideUIContextRule(UiConstraintGuidString, // Must match the GUID in the .vsct file
		name: "UI Context",
		expression: "header | code", // This will make the button only show for .cpp and .h files
		termNames: new[] { "header", "code" },
		termValues: new[] { "HierSingleSelectionName:.h$", "HierSingleSelectionName:.cpp$" })]
	[ProvideAutoLoad(UiConstraintGuidString, PackageAutoLoadFlags.BackgroundLoad)]
	public sealed class GoToCommandsPackage : Package
	{
		public const string PackageGuidString = "aeae4f09-dcb3-4a6d-950c-668bbef87f2b";
		public const string UiConstraintGuidString = "6B4C995A-47FD-4461-90A2-2048B531EBE1";

		public GoToCommandsPackage()
		{
		}

		#region Package Members

		protected override void Initialize()
		{
			ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			base.Initialize();
			GoToHeaderCode.Initialize(this);
		    GoToTestClass.Initialize(this);
		}

		#endregion
	}
}
