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
        
        [Description("Maximum number of files opened on tabs. If more are open, the last used file (which isn't modified) is closed")]
        public int NumberOfOpenFiles { get; set; }
    }
}