using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Projects;

namespace VS4Mac.AssetStudio.Helpers
{
    public static class FileHelper
    {
        internal static readonly string[] ValidImageExtensions = { "jpg", "bmp", "png" };

        public static bool IsImageFile(ProjectFile projectFile)
        {
            if (projectFile == null)
                return false;

            var extension = projectFile.FilePath.Extension;

            foreach (var validImageExtension in ValidImageExtensions)
            {
                if (extension.Contains(validImageExtension))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool FolderContainsImage(ProjectFolder projectFolder)
        {
            if (projectFolder == null)
                return false;

            var directoryPath = projectFolder.Path;
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                foreach (var validImageExtension in ValidImageExtensions)
                {
                    if (file.Contains(validImageExtension))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string[] GetFolderImages(string searchFolder)
        {
            List<string> filesFound = new List<string>();

            foreach (var filter in ValidImageExtensions)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, string.Format("*.{0}", filter), SearchOption.AllDirectories)
                .Select(p => p.Remove(0, searchFolder.Length)));
            }

            return filesFound.ToArray();
        }
    }
}