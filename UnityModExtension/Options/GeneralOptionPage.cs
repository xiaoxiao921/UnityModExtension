using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace UnityModExtension.Commands.Debug
{
    public class GeneralOptionPage : DialogPage
    {
        private string _targetPath = @"C:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2.exe";
        private string _targetArguments = "";
        private int _targetPort = 55555;

        [Category("Target")]
        [DisplayName("Path")]
        [Description("Full path to the target to run when using the Run And Start Debugging Command")]
        public string TargetPath
        {
            get { return _targetPath; }
            set { _targetPath = value; }
        }

        [Category("Target")]
        [DisplayName("Arguments")]
        [Description("Extra arguments when the target run")]
        public string TargetArguments
        {
            get { return _targetArguments; }
            set { _targetArguments = value; }
        }

        [Category("Target")]
        [DisplayName("Port")]
        [Description("Port used by the remote debugger")]
        public int TargetPort
        {
            get { return _targetPort; }
            set { _targetPort = value; }
        }
    }
}
