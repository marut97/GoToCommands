using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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

            var filePath = _dte.ActiveDocument.FullName;
            bool header = false;
            if (filePath.EndsWith(".h")) 
                header = true;

            var fileName = _dte.ActiveDocument.Name;


            string goToFolder = header ? "src" : "include";
            string currentFolder = !header ? "src" : "include";

            string currentExtension = header ? ".h" : ".cpp";
            string goToExtension = !header ? ".h" : ".cpp";

            fileName = fileName.Replace(currentExtension, goToExtension);

            string relativePath = "\\..\\" + goToFolder;

            //var newPath = filePath.Substring(0, filePath.LastIndexOf(currentFolder) + currentFolder.Length) + relativePath + "\\" + fileName;
            var path = filePath.Substring(filePath.LastIndexOf(currentFolder) + currentFolder.Length, filePath.Length-1- (filePath.LastIndexOf(currentFolder) + currentFolder.Length));
            var newPath = FindHeaderCodeFilePath(filePath.Substring(0, filePath.LastIndexOf(currentFolder) + currentFolder.Length) + relativePath, fileName, path);

            if(File.Exists(newPath))
                _dte.ExecuteCommand("File.OpenFile", newPath);
        }

        private static string FindHeaderCodeFilePath(string basePath, string fileName, string currentFolder)
        {
            var files = Directory.GetFiles(basePath, fileName, SearchOption.AllDirectories).ToList();

            return HandleDuplicates(files, currentFolder);
        }

        private static string HandleDuplicates(List<string> files, string currentFolder)
        {
            if(files.Count > 1)
            {
                List<int> similarityIndex = new List<int>(files.Count);
                for (int i = 0; i < similarityIndex.Count; i++)
                {
                    similarityIndex[i] = 0;
                }

                List<string> folders = currentFolder.Split('/').ToList();
                foreach (var folderName in folders)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        if (files[i].Contains(folderName))
                            similarityIndex[i]++;
                    }
                }

                var maxValueIndex = similarityIndex.IndexOf(similarityIndex.Max());
                return files[maxValueIndex];
            }

            else if (files.Count == 1)
                return files[0];

            return "";
        }
    }
}
