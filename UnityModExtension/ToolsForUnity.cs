using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace UnityModExtension
{
    public static class ToolsForUnity
    {
        private const BindingFlags All = (BindingFlags)(-1);

        private const string ToolsForUnityDllName = "SyntaxTree.VisualStudio.Unity";
        private const string ToolsForUnityMessagingDllName = "SyntaxTree.VisualStudio.Unity.Messaging";

        private const string ToolsForUnityDebuggerNamespaceName = "SyntaxTree.VisualStudio.Unity.Debugger";

        public static bool Loaded { get; private set; }

        public static Type UnityPackageType { get; set; }
        public static Type UnityConnectorType { get; set; }

        public static Type UnityDebuggerEngineFactoryType { get; set; }
        public static Type UnitySolutionListenerType { get; set; }

        public static Type UnityProcessType { get; set; }
        public static Type UnityPlatformType { get; set; }

        internal static void Init()
        {
            if (Loaded)
            {
                return;
            }

            LoadToolsForUnityPackage();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var ass in assemblies)
            {
                if (ass.GetName().Name == ToolsForUnityDllName)
                {
                    UnityPackageType = ass.GetType(ToolsForUnityDllName + ".UnityPackage");
                    UnityConnectorType = ass.GetType(ToolsForUnityDllName + ".UnityConnector");

                    UnityDebuggerEngineFactoryType = ass.GetType(ToolsForUnityDebuggerNamespaceName + ".DebuggerEngineFactory");

                    if (!assemblies.Any(a => a.GetName().Name == ToolsForUnityMessagingDllName))
                    {
                        Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(UnityPackageType.Assembly.Location), ToolsForUnityMessagingDllName + ".dll"));
                    }
                }
                else if (ass.GetName().Name == ToolsForUnityMessagingDllName)
                {
                    UnityProcessType = ass.GetType(ToolsForUnityMessagingDllName + ".UnityProcess");
                    UnityPlatformType = ass.GetType(ToolsForUnityMessagingDllName + ".Platform");
                }
            }

            if (UnityPackageType != null &&
                UnityConnectorType != null &&
                UnityDebuggerEngineFactoryType != null &&
                UnityProcessType != null &&
                UnityPlatformType != null)
            {
                Loaded = true;
            }
            else
            {
                Debug.Print($"Failed initializing {nameof(ToolsForUnity)}\n" +
                    $"{nameof(UnityPackageType)} : {UnityPackageType != null}\n" +
                    $"{nameof(UnityConnectorType)} : {UnityConnectorType != null}\n" +
                    $"{nameof(UnityDebuggerEngineFactoryType)} : {UnityDebuggerEngineFactoryType != null}\n" +
                    $"{nameof(UnityProcessType)} : {UnityProcessType != null}\n" +
                    $"{nameof(UnityPlatformType)} : {UnityPlatformType != null}");
            }
        }

        private static void LoadToolsForUnityPackage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var vsShell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(IVsShell));
            if (vsShell != null)
            {
                var unityGuid = new Guid("B6546C9C-E5FE-4095-8D39-C080D9BD6A85");
                if (vsShell.IsPackageLoaded(unityGuid, out var vsPackage) != Microsoft.VisualStudio.VSConstants.S_OK)
                {
                    vsShell.LoadPackage(unityGuid, out vsPackage);
                }
            }
        }

        public static IPAddress[] IPAddresses()
        {
            return (IPAddress[])UnityPlatformType.GetMethod("IPAddresses", All).Invoke(null, new object[] { });
        }

        public static object CreateUnityProcess(int port)
        {
            var endPoint = new IPEndPoint(IPAddresses()[0], port);

            var ctorMethod = UnityProcessType.GetConstructor(All, null, Type.EmptyTypes, null);

            var unityProcess = ctorMethod.Invoke(null);
            UnityProcessType.GetProperty("Type").SetValue(unityProcess, 0x10);
            UnityProcessType.GetProperty("DiscoveryType").SetValue(unityProcess, 0x1);
            UnityProcessType.GetProperty("Address").SetValue(unityProcess, endPoint.Address.ToString());
            UnityProcessType.GetProperty("DebuggerPort").SetValue(unityProcess, endPoint.Port);

            return unityProcess;
        }

        private static object UnityConnectorInstance;

        public static void LaunchDebugger(object unityProcess)
        {
            var unityPackageInstance = UnityPackageType.GetProperty("CurrentInstance", All).GetValue(null);

            var unityConnectorCtor = UnityConnectorType.GetConstructor(new[] { typeof(IServiceProvider) });

            if (UnityConnectorInstance == null)
                UnityConnectorInstance = unityConnectorCtor.Invoke(new object[] { null });
            if (UnityConnectorInstance != null)
            {
                var connectToTargetProcessMethod = UnityConnectorType.GetMethod("ConnectToTargetProcess", All);
                connectToTargetProcessMethod.Invoke(UnityConnectorInstance, new[] { unityProcess });
            }

            UnityDebuggerEngineFactoryType.
                GetMethod("LaunchDebugger", All).
                Invoke(null, new object[] { unityPackageInstance, unityProcess });
        }
    }
}
