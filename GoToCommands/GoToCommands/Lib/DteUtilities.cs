using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToCommands.Lib
{
	public static class DteUtilities
	{
		public static List<Project> _testProjects = new List<Project>();

		public static List<Project> _codeProjects = new List<Project>();

		static bool _projectsFilled = false;

		public static void FillProjects(DTE dte)
		{
			if (_projectsFilled)
				return;

			Projects projects = dte.Solution.Projects;
			var item = projects.GetEnumerator();
			while (item.MoveNext())
			{
				var project = item.Current as Project;
				if (project == null)
				{
					continue;
				}

				if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
				{
					GetSolutionFolderProjects(project);
				}
				else
				{
					AddProject(project);
				}
			}
			_projectsFilled = true;
		}

		private static void AddProject(Project project)
		{
			if (project.FullName.EndsWith("vcxproj"))
			{
				if (project.Name.ToLower().Contains("test"))
					_testProjects.Add(project);
				else
					_codeProjects.Add(project);
			}
		}

		private static void GetSolutionFolderProjects(Project solutionFolder)
		{
			for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
			{
				var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
				if (subProject == null)
				{
					continue;
				}

				// If this is another solution folder, do a recursive call, otherwise add
				if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
				{
					GetSolutionFolderProjects(subProject);
				}
				else
				{
					AddProject(subProject);
				}
			}
		}

		public static List<String> Files(Project project, bool codeFiles = true)
		{
			var files = new List<String>();
			foreach (ProjectItem item in project.ProjectItems)
			{
				for (short i = 0; i < item.FileCount; i++)
				{
					var fileName = item.FileNames[i];
					if ((codeFiles && Utilities.IsCode(fileName)) || Utilities.IsHeader(fileName))
						files.Add(fileName);
				}
			}
			return files;
		}

		public static List<Project> TestProjects(String codeProjectName)
		{
			Project matchingProject = null;
			int projectNameSize = int.MaxValue;
			foreach (var project in DteUtilities._testProjects)
			{
				var projectName = project.Name;
				if (projectName.Contains(codeProjectName) && projectName.Length < projectNameSize)
				{
					matchingProject = project;
					projectNameSize = projectName.Length;
				}
			}

			if (matchingProject == null)
				return DteUtilities._testProjects;

			return new List<Project> { matchingProject };
		}

		public static List<Project> CodeProjects(string testProjectName)
		{
			Project matchingProject = null;
			int projectNameSize = 0;
			foreach (var project in DteUtilities._codeProjects)
			{
				var projectName = project.Name;
				if (testProjectName.Contains(projectName) && projectName.Length > projectNameSize)
				{
					matchingProject = project;
					projectNameSize = projectName.Length;
				}
			}

			if (matchingProject == null)
				return DteUtilities._codeProjects;

			return new List<Project> { matchingProject };
		}
	}
}
