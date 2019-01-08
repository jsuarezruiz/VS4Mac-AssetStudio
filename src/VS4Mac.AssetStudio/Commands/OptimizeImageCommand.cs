using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using VS4Mac.AssetStudio.Helpers;
using VS4Mac.AssetStudio.Views;

namespace VS4Mac.AssetStudio.Commands
{
    public class OptimizeImageCommand : CommandHandler
    {
        protected override void Run()
        {
            var projectFile = IdeApp.ProjectOperations.CurrentSelectedItem as ProjectFile;

            using (var compressImageDialog = new OptimizeImageDialog(projectFile))
            {
                compressImageDialog.Run(Xwt.MessageDialog.RootWindow);
            }
        }

        protected override void Update(CommandInfo info)
        {
            info.Visible =
                IsWorkspaceOpen()
                && ProjectHelper.IsProjectReady()
                && SelectedItemIsImage();
        }

        bool IsWorkspaceOpen() => IdeApp.Workspace.IsOpen;

        bool SelectedItemIsImage()
        {
            var projectFile = IdeApp.ProjectOperations.CurrentSelectedItem as ProjectFile;
            return FileHelper.IsImageFile(projectFile);
        }
    }
}