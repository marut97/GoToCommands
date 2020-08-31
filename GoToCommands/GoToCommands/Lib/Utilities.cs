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
		private static readonly string headerExtension = ".h";
		private static readonly string codeExtension = ".cpp";
		private static readonly string headerFolder = "include";
		private static readonly string codeFolder = "src";
		private static readonly string testFolder = "test";
		private static readonly string headerCodeRelativePath = "\\..\\";
		private static readonly string headerTestRelativePath = "\\..\\..\\";

		public static string FindHeader(string codePath, string codeFileName)
		{
			var headerPath = codePath.Replace(codeExtension, headerExtension);
			if (File.Exists(headerPath))
				return headerPath;

			var headerFileName = codeFileName.Replace(codeExtension, headerExtension);

			var relativePath = headerCodeRelativePath + headerFolder;

			var path = codePath.Substring(codePath.LastIndexOf(codeFolder) + codeFolder.Length, codePath.Length - 1 - (codePath.LastIndexOf(codeFolder) + codeFolder.Length));
			headerPath = FindHeaderCodeFilePath(codePath.Substring(0, codePath.LastIndexOf(codeFolder) + codeFolder.Length) + relativePath, headerFileName, path);

			if (File.Exists(headerPath))
				return headerPath;

			return null;
		}

		public static string FindCode(string headerPath, string headerFileName)
		{
			var codePath = headerPath.Replace(headerExtension, codeExtension);
			if (File.Exists(codePath))
				return codePath;

			var codeFileName = headerFileName.Replace(headerExtension, codeExtension);

			var relativePath = headerCodeRelativePath + codeFolder;

			var path = headerPath.Substring(headerPath.LastIndexOf(headerFolder) + headerFolder.Length, headerPath.Length - 1 - (headerPath.LastIndexOf(headerFolder) + headerFolder.Length));
			codePath = FindHeaderCodeFilePath(headerPath.Substring(0, headerPath.LastIndexOf(headerFolder) + headerFolder.Length) + relativePath, codeFileName, path);

			if (File.Exists(codePath))
				return codePath;

			return null;
		}

		public static string FindTest(string filePath, string fileName)
		{
			if (fileName.Contains(".cpp"))
				filePath = filePath.Substring(0, filePath.LastIndexOf(codeFolder)) + headerFolder;

			var testPath = filePath.Substring(0, filePath.LastIndexOf(headerFolder)) + headerFolder + headerTestRelativePath + testFolder;

			if (!Directory.Exists(testPath))
				return null;

			var extensionIndex = fileName.LastIndexOf(".");
			fileName = fileName.Remove(extensionIndex);

			var files = Directory.GetFiles(testPath, "*"+fileName+"*", SearchOption.AllDirectories).ToList().OrderBy( e => (e.Length - e.LastIndexOf("\\")));

			if (files.Count() > 0)
				return files.First();

			return null;
		}

		public static string FindClass(string filePath, string fileName)
		{
			filePath = filePath.Remove(filePath.LastIndexOf(testFolder + "\\"));
			var extensionIndex = fileName.LastIndexOf(".");
			fileName = fileName.Remove(extensionIndex);


			string headerFolderPath = null;

			foreach (var folder in Directory.GetDirectories(filePath).ToList())
			{
				List<string> paths = Directory.GetDirectories(folder, headerFolder).ToList();
				if (paths.Count() > 0)
				{
					headerFolderPath = paths.First();
					break;
				}
			}

			if (headerFolderPath == null)
				return null;

			var files = getAllHeaderFiles(headerFolderPath);

			int currentFileSize = 100;
			string matchedFile = null;
			foreach (var file in files)
			{
				var headerFileName = file.Substring(file.LastIndexOf("\\") + 1);
				headerFileName = headerFileName.Substring(0, headerFileName.LastIndexOf("."));
				if (fileName.Contains(headerFileName) && headerFileName.Count() < currentFileSize)
				{
					currentFileSize = headerFileName.Count();
					matchedFile = file;
				}
			}

			return matchedFile;
		}

		private static List<string> getAllHeaderFiles(string headerFolderPath)
		{
			var files = Directory.GetFiles(headerFolderPath, "*.h", SearchOption.AllDirectories).ToList();
			files.AddRange(Directory.GetFiles(headerFolderPath + headerCodeRelativePath + codeFolder, "*.h", SearchOption.AllDirectories).ToList());

			return files;
		}

		private static string FindHeaderCodeFilePath(string basePath, string fileName, string currentFolder)
		{
			if (!Directory.Exists(basePath))
				return null;

			var files = Directory.GetFiles(basePath, fileName, SearchOption.AllDirectories).ToList();

			return HandleDuplicates(files, currentFolder);
		}

		private static string HandleDuplicates(List<string> files, string currentFolder)
		{
			if (files.Count > 1)
			{
				List<int> similarityIndex = new List<int>();
				for (int i = 0; i < files.Count; i++)
				{
					similarityIndex.Add(0);
				}

				List<string> folders = currentFolder.Split('\\').ToList();
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

		private static string BestMatch(List<int> similarityIndex, List<string> files)
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
	}
}
