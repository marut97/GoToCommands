using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToCommands.Lib
{
    public static class Utilities
    {
        public static bool IsHeader(String fileName)
        {
            return fileName.EndsWith(".h");
        }

        public static bool IsCode(String fileName)
        {
            return fileName.EndsWith(".cpp");
        }

        public static bool IsTestProject(String projectName)
        {
            return projectName.ToLower().Contains("test");
        }

        public static bool IsInVSProject(string projectName)
        {
            return projectName != "Miscellaneous Files";
        }

		public static String FileName(string projectPath)
		{
			var lastIndex = projectPath.LastIndexOf(".");
			var startIndex = projectPath.LastIndexOf("\\") + 1;
			if (lastIndex > startIndex)
				return projectPath.Substring(startIndex, lastIndex - startIndex);
			return null;
		}

		public static String RemoveExtension(String fileName)
		{
			var index = fileName.LastIndexOf(".");
			return fileName.Substring(0, index > 0 ? index : 0);
		}

		public static string FindHeaderCode(String filePath, String fileName)
		{
			bool header = filePath.EndsWith(".h");

			string goToFolderName = header ? "src" : "include";
			string currentFolderName = !header ? "src" : "include";

			string currentExtension = header ? ".h" : ".cpp";
			string goToExtension = !header ? ".h" : ".cpp";

			var path = filePath.Replace(currentExtension, goToExtension);

			if (File.Exists(path))
				return path;

			fileName = fileName.Replace(currentExtension, goToExtension);

			string relativePath = "\\..\\" + goToFolderName;

			var currentFolderIndex = filePath.LastIndexOf(currentFolderName) + currentFolderName.Length;
			var matchPathLength = filePath.LastIndexOf("\\") - currentFolderIndex;
			matchPathLength = matchPathLength > 0 ? matchPathLength : 0;
			var matchPath = filePath.Substring(currentFolderIndex, matchPathLength);
			path = FindFile(fileName, filePath.Substring(0, currentFolderIndex) + relativePath, matchPath);

			if (File.Exists(path))
				return path;

			return null;
		}

		private static String FindFile(String fileName, String findFolder, String matchPath)
		{
			if (!Directory.Exists(findFolder))
				return null;

			var files = Directory.GetFiles(findFolder, fileName, SearchOption.AllDirectories).ToList();

			return HandleDuplicates(files, matchPath);
		}

		private static String HandleDuplicates(List<String> files, String matchPath)
		{
			if (files.Count > 1)
			{
				List<int> similarityIndex = new List<int>();
				for (int i = 0; i < files.Count; i++)
				{
					similarityIndex.Add(0);
				}

				List<String> folders = matchPath.Split('\\').ToList();
				foreach (var folderName in folders)
				{
					for (int i = 0; i < files.Count; i++)
					{
						if (files[i].Contains(folderName))
							similarityIndex[i]++;
					}
				}

				return BestMatch(similarityIndex, files);
			}

			else if (files.Count == 1)
				return files[0];

			return null;
		}

		private static String BestMatch(List<int> similarityIndex, List<String> files)
		{
			var maxValue = similarityIndex.Max();
			var index = similarityIndex.IndexOf(maxValue);
			var maxIndex = similarityIndex.LastIndexOf(maxValue);
			var file = files[index];
			index++;
			while (index <= maxIndex)
			{
				if (file.Split('\\').Length > files[index].Split('\\').Length)
					file = files[index];

				index++;
			}
			return file;
		}

		public static String BestTest(List<string> allFiles, string classFile)
		{
			int fileNameSize = int.MaxValue;
			String testFile = null;
			foreach (var file in allFiles)
			{
				var testFileName = FileName(file);
				if (testFileName.Contains(classFile) && testFileName.Length < fileNameSize)
				{
					fileNameSize = testFileName.Length;
					testFile = file;
				}
			}
			return testFile;
		}

		public static String BestClass(List<string> allFiles, string testFileName)
		{
			int fileNameSize = 0;
			String testFile = null;
			foreach (var file in allFiles)
			{
				var classFileName = FileName(file);
				if (testFileName.Contains(classFileName) && classFileName.Length > fileNameSize)
				{
					fileNameSize = classFileName.Length;
					testFile = file;
				}
			}
			return testFile;
		}

	}
}
