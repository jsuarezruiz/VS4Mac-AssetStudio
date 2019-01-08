using MonoDevelop.Components;
using MonoDevelop.Ide.Gui.Dialogs;
using VS4Mac.AssetStudio.Controls;

namespace VS4Mac.AssetStudio.Views
{
    public class OptimizeImageSettingsPanel : OptionsPanel
    {
        OptimizeImageSettingsWidget _widget;

        public override void ApplyChanges()
        {
            _widget.ApplyChanges();
        }

        public override Control CreatePanelWidget()
        {
            _widget = new OptimizeImageSettingsWidget();

            return new XwtControl(_widget);
        }
    }
}