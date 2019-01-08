using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using VS4Mac.AssetStudio.Helpers;
using VS4Mac.AssetStudio.Views;

namespace VS4Mac.AssetStudio.Commands
{
    public class OptimizeImagesCommand : CommandHandler
    {
        protected override void Run()
        {
            var projectFolder = IdeApp.ProjectOperations.CurrentSelectedItem as ProjectFolder;

            using (var compressImageDialog = new OptimizeImagesDialog(projectFolder))
            {
                compressImageDialog.Run(Xwt.MessageDialog.RootWindow);
            }
        }

        protected override void Update(CommandInfo info)
        {
            info.Visible =
                IsWorkspaceOpen()
                && ProjectHelper.IsProjectReady()
                && FolderContainsImage();
        }

        bool IsWorkspaceOpen() => IdeApp.Workspace.IsOpen;

        bool FolderContainsImage()
        {
            var projectFolder = IdeApp.ProjectOperations.CurrentSelectedItem as ProjectFolder;
            return FileHelper.FolderContainsImage(projectFolder);
        }
    }
}