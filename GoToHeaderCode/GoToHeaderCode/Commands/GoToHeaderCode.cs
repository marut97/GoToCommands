using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GoToHeaderCode.Commands
{

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

            var filePath = _dte.ActiveDocument.FullName;
            bool header = filePath.EndsWith(".h");

            var fileName = _dte.ActiveDocument.Name;


            string goToFolder = header ? "src" : "include";
            string currentFolder = !header ? "src" : "include";

            string currentExtension = header ? ".h" : ".cpp";
            string goToExtension = !header ? ".h" : ".cpp";

            var newPath = filePath.Replace(currentExtension, goToExtension);

            if (OpenFile(newPath))
                return;

            fileName = fileName.Replace(currentExtension, goToExtension);

            string relativePath = "\\..\\" + goToFolder;

            var path = filePath.Substring(filePath.LastIndexOf(currentFolder) + currentFolder.Length, filePath.Length - 1 - (filePath.LastIndexOf(currentFolder) + currentFolder.Length));
            newPath = FindHeaderCodeFilePath(filePath.Substring(0, filePath.LastIndexOf(currentFolder) + currentFolder.Length) + relativePath, fileName, path);

            if (OpenFile(newPath))
                return;
        }

        public static bool OpenFile(string path)
        {
            if (File.Exists(path))
            {
                _dte.ExecuteCommand("File.OpenFile", path);
                return true;
            }
            return false;
        }

        private static string FindHeaderCodeFilePath(string basePath, string fileName, string currentFolder)
        {
            if (!Directory.Exists(basePath))
                return "";

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

            return "";
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
