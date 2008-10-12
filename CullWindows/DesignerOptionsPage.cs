using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace BrunoMLopes.CullWindows {

    [Guid(GuidList.guidCullWindowsDesignerOptionsPage)]
    public class DesignerOptionsPage : DialogPage, IOptionsProvider {

        public const string CategoryName = "Cull Windows";
        public const string PageName = "Options";

        public override void ResetSettings() {
            base.ResetSettings();
            NumberOfOpenFiles = 10;
        }

        private int _numberOfOpenFiles = 10;

        [DisplayName("Number of open files")]
        [DefaultValue(10)]
        [Description("Maximum number of concurrent files opened on tabs. If more are open, the last used file (which isn't modified) is closed")]
        public int NumberOfOpenFiles {
            get { return _numberOfOpenFiles; }
            set { _numberOfOpenFiles = value; }
        }
    }
}