using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace UnityModExtension.Commands.Debug
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RunAndStartDebuggingCmd
    {
        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RunAndStartDebuggingCmd Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("2bfd9814-8bf4-4d5a-828e-1976305175b3");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public string TargetPath
        {
            get
            {
                GeneralOptionPage page = (GeneralOptionPage)package.GetDialogPage(typeof(GeneralOptionPage));
                return page.TargetPath;
            }
        }

        public string TargetArguments
        {
            get
            {
                GeneralOptionPage page = (GeneralOptionPage)package.GetDialogPage(typeof(GeneralOptionPage));
                return page.TargetArguments;
            }
        }

        public int TargetPort
        {
            get
            {
                GeneralOptionPage page = (GeneralOptionPage)package.GetDialogPage(typeof(GeneralOptionPage));
                return page.TargetPort;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunAndStartDebuggingCmd"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private RunAndStartDebuggingCmd(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in RunAndStartDebuggingCmd's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new RunAndStartDebuggingCmd(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object _, EventArgs __)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ToolsForUnity.Init();
            if (ToolsForUnity.Loaded)
            {
                try
                {
                    RunTarget();
                    LaunchDebugger();
                }
                catch (Exception e)
                {
                    UnityModExtension.Debug.Print(e.ToString());
                }
            }
            else
            {

            }
        }

        private void RunTarget()
        {
            var processStartInfo = new ProcessStartInfo(TargetPath)
            {
                //WorkingDirectory = pwd,
                Arguments = TargetArguments,
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
        }

        private void LaunchDebugger()
        {
            var unityProcess = ToolsForUnity.CreateUnityProcess(TargetPort);
            ToolsForUnity.LaunchDebugger(unityProcess);
        }
    }
}
