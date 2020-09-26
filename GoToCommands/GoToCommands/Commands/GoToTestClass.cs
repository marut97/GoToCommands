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

			InitializeProjectsList();

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
			var path = GetFile(projectName, test, _dte.ActiveDocument.Name);
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
				_dte.ExecuteCommand("File.OpenFile", path);
		}


		private void InitializeProjectsList()
		{
			//TODO: Needs to be changed to recursive method to get all projects in the future
			var solution = _dte.Solution;
			var projects = Directory.GetFiles(_dte.Solution.FullName.Substring(0, _dte.Solution.FullName.LastIndexOf("\\")), "*.vcxproj", SearchOption.AllDirectories).ToList();

			_codeProjects = new List<String>();
			_testProjects = new List<String>();

			//TODO: Need to add recursive method to find all project files in project
			foreach (var project in projects)
			{
				if (project.Substring(project.LastIndexOf("\\")).ToLower().Contains("test"))
					_testProjects.Add(project);
				else
					_codeProjects.Add(project);
			}
		}

		private static String GetFile(String projectName, bool test, String fileName)
		{
			return test ? FindClass(projectName, fileName) : FindTest(projectName, fileName);
		}

		private static string FindClass(string projectName, string fileName)
		{
			var testFileName = Utilities.RemoveExtension(fileName);
			List<String> projects = CodeProjects(projectName);
			List<String> allFiles = new List<String>();

			foreach (var project in projects)
			{
				String projectPath = project.Substring(0, project.LastIndexOf("\\"));
				allFiles.AddRange(Directory.GetFiles(projectPath, "*.h", SearchOption.AllDirectories).ToList());
			}
			return Utilities.BestClass(allFiles, testFileName);
		}

		private static List<String> CodeProjects(string testProjectName)
		{
			String matchingProject = null;
			int projectNameSize = 0;
			foreach (var project in _codeProjects)
			{
				var projectName = Utilities.FileName(project);
				if (testProjectName.Contains(projectName) && projectName.Length > projectNameSize)
				{
					matchingProject = project;
					projectNameSize = projectName.Length;
				}
			}

			if (matchingProject == null)
				return _codeProjects;

			return new List<String> { matchingProject };
		}

		private static string FindTest(string projectName, string fileName)
		{
			var classFileName = Utilities.RemoveExtension(fileName);
			List<String> projects = TestProjects(projectName);
			List<String> allFiles = new List<String>();

			foreach (var project in projects)
			{
				String projectPath = project.Substring(0, project.LastIndexOf("\\"));
				 allFiles.AddRange(Directory.GetFiles(projectPath, "*" + classFileName + "*.cpp", SearchOption.AllDirectories).ToList());
			}
			var validFiles = new List<String>();
			foreach (var file in allFiles)
			{
				if (file.EndsWith(".h") || file.EndsWith(".cpp"))
					validFiles.Add(file);
			}
			return Utilities.BestTest(validFiles, classFileName);
		}

		private static List<String> TestProjects(String codeProjectName)
		{
			String matchingProject = null;
			int projectNameSize = int.MaxValue;
			foreach (String project in _testProjects)
			{
				var projectName = Utilities.FileName(project);
				if (projectName.Contains(codeProjectName) && projectName.Length < projectNameSize)
				{
					matchingProject = project;
					projectNameSize = projectName.Length;
				}
			}

			if (matchingProject == null)
				return _testProjects;

			return new List<String>{ matchingProject };
		}

	}
}
