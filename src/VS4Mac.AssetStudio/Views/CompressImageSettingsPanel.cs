using MonoDevelop.Components;
using MonoDevelop.Ide.Gui.Dialogs;
using VS4Mac.AssetStudio.Controls;

namespace VS4Mac.AssetStudio.Views
{
    public class CompressImageSettingsPanel : OptionsPanel
    {
        CompressImageSettingsWidget _widget;

        public override void ApplyChanges()
        {
            _widget.ApplyChanges();
        }

        public override Control CreatePanelWidget()
        {
            _widget = new CompressImageSettingsWidget();

            return new XwtControl(_widget);
        }
    }
}