using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using TimeoutException = System.TimeoutException;

namespace DotNetAidLib.Core.Proc
{
    public enum ApplicationType
    {
        NativeAppication,
        DotNetAppication,
        DotNetService
    }

    public class ProcessHelper
    {
        public static void ExecuteTimeout(Action action, int timeoutMs)
        {
            ExecuteTimeout(action, TimeSpan.FromMilliseconds(timeoutMs));
        }

        public static void ExecuteTimeout(Action action, TimeSpan timeout)
        {
            Thread taskThread = null;

            var d = DateTime.Now;
            Task.Factory.StartNew(() =>
            {
                taskThread = Thread.CurrentThread;
                action.Invoke();
            });

            while (taskThread.IsAlive && DateTime.Now.Subtract(d) < timeout)
                Thread.Sleep(1);

            if (taskThread.IsAlive)
            {
                taskThread.Abort();
                throw new TimeoutException();
            }
        }

        public static System.Diagnostics.Process FindProcessById(long id)
        {
            System.Diagnostics.Process ret = null;

            var procs = System.Diagnostics.Process.GetProcesses();

            foreach (var proc in procs)
            {
                if (id == proc.Id) ret = proc;

                if (ret != null) break; // TODO: might not be correct. Was : Exit For
            }

            return ret;
        }

        public static System.Diagnostics.Process FindProcessByName(string name)
        {
            System.Diagnostics.Process ret = null;

            var procs = System.Diagnostics.Process.GetProcesses();

            foreach (var proc in procs)
            {
                if (name == proc.ProcessName) ret = proc;

                if (ret != null) break; // TODO: might not be correct. Was : Exit For
            }

            return ret;
        }

        public static System.Diagnostics.Process FindProcessByFileName(string fileName)
        {
            System.Diagnostics.Process ret = null;

            var procs = System.Diagnostics.Process.GetProcesses();

            foreach (var proc in procs)
            {
                if (fileName == GetProcessFileName(proc)) ret = proc;

                if (ret != null) break; // TODO: might not be correct. Was : Exit For
            }

            return ret;
        }

        public static System.Diagnostics.Process FindProcessByFilePath(string filePath)
        {
            System.Diagnostics.Process ret = null;

            var procs = System.Diagnostics.Process.GetProcesses();

            foreach (var proc in procs)
            {
                if (filePath == GetProcessFilePath(proc)) ret = proc;

                if (ret != null) break; // TODO: might not be correct. Was : Exit For
            }

            return ret;
        }

        public static System.Diagnostics.Process ExecuteProcessIfIsRunningProcessId(int runningProcessId,
            string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput,
            bool UseShellExecute, bool CreateNoWindows)
        {
            while (FindProcessById(runningProcessId) == null) Thread.Sleep(1);
            return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute,
                CreateNoWindows);
        }

        public static System.Diagnostics.Process ExecuteProcessIfIsRunningProcessName(string runningProcessName,
            string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput,
            bool UseShellExecute, bool CreateNoWindows)
        {
            while (FindProcessByName(runningProcessName) == null) Thread.Sleep(1);
            return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute,
                CreateNoWindows);
        }

        public static System.Diagnostics.Process ExecuteProcessIfIsRunningProcessFileName(string runningProcessFileName,
            string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput,
            bool UseShellExecute, bool CreateNoWindows)
        {
            while (FindProcessByFileName(runningProcessFileName) == null) Thread.Sleep(1);
            return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute,
                CreateNoWindows);
        }

        public static System.Diagnostics.Process ExecuteProcessIfIsRunningProcessFilePath(string runningProcessFilePath,
            string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput,
            bool UseShellExecute, bool CreateNoWindows)
        {
            while (FindProcessByFilePath(runningProcessFilePath) == null) Thread.Sleep(1);
            return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute,
                CreateNoWindows);
        }

        public static System.Diagnostics.Process ExecuteProcessAssertByRunningProcessId(int runningProcessId,
            string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput,
            bool UseShellExecute, bool CreateNoWindows)
        {
            if (FindProcessById(runningProcessId) != null)
                return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput,
                    UseShellExecute, CreateNoWindows);
            throw new Exception("Running Process Id '" + runningProcessId + "' is not running.");
        }

        public static System.Diagnostics.Process ExecuteProcessAssertByRunningProcessName(string runningProcessName,
            string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput,
            bool UseShellExecute, bool CreateNoWindows)
        {
            if (FindProcessByName(runningProcessName) != null)
                return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput,
                    UseShellExecute, CreateNoWindows);
            throw new Exception("Running Process Name '" + runningProcessName + "' is not running.");
        }

        public static System.Diagnostics.Process ExecuteProcessAssertByRunningProcessFileName(
            string runningProcessFileName, string executablePath, string args, bool RedirectStandardInput,
            bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
        {
            if (FindProcessByFileName(runningProcessFileName) != null)
                return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput,
                    UseShellExecute, CreateNoWindows);
            throw new Exception("Running Process File Name '" + runningProcessFileName + "' is not running.");
        }

        public static System.Diagnostics.Process ExecuteProcessAssertByRunningProcessFilePath(
            string runningProcessFilePath, string executablePath, string args, bool RedirectStandardInput,
            bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
        {
            if (FindProcessByFilePath(runningProcessFilePath) != null)
                return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput,
                    UseShellExecute, CreateNoWindows);
            throw new Exception("Running Process File Path '" + runningProcessFilePath + "' is not running.");
        }

        public static System.Diagnostics.Process CreateProcess(string executablePath, string args,
            bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
        {
            var fi = new FileInfo(executablePath);
            return CreateProcess(fi.Directory.FullName, executablePath, args, RedirectStandardInput,
                RedirectStandardOutput, UseShellExecute, CreateNoWindows);
        }

        public static System.Diagnostics.Process CreateProcess(string workingDirectory, string executablePath,
            string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute,
            bool CreateNoWindows)
        {
            ProcessStartInfo psi = null;
            if (args == null)
                psi = new ProcessStartInfo(executablePath);
            else
                psi = new ProcessStartInfo(executablePath, args);

            psi.WorkingDirectory = workingDirectory;
            psi.RedirectStandardInput = RedirectStandardInput;
            psi.RedirectStandardOutput = RedirectStandardOutput;

            psi.UseShellExecute = UseShellExecute;
            psi.CreateNoWindow = CreateNoWindows;

            var ret = new System.Diagnostics.Process();
            ret.StartInfo = psi;

            return ret;
        }

        public static void ExecuteProcesses(System.Diagnostics.Process[] processes, int msDelay)
        {
            foreach (var proc in processes)
            {
                Thread.Sleep(msDelay);
                proc.Start();
            }
        }

        public static System.Diagnostics.Process ExecuteProcess(string executablePath, string args,
            bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
        {
            var ret = CreateProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput,
                UseShellExecute, CreateNoWindows);
            ret.Start();
            return ret;
        }

        public static string GetProcessFilePath(System.Diagnostics.Process proc)
        {
            try
            {
                return proc.MainModule.FileName;
            }
            catch
            {
                return null;
            }
        }

        public static string GetProcessFileName(System.Diagnostics.Process proc)
        {
            try
            {
                var fi = new FileInfo(proc.MainModule.FileName);
                return fi.Name;
            }
            catch
            {
                return null;
            }
        }

        public static bool IsNetApplication(string executablePath)
        {
            try
            {
                Assembly.LoadFrom(executablePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Type GetMainClassType(string executablePath)
        {
            try
            {
                var fileAssembly = Assembly.LoadFrom(executablePath);
                var mainMethod = fileAssembly.EntryPoint;
                if (mainMethod != null)
                    return mainMethod.DeclaringType;

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static ApplicationType GetApplicationType(string applicationPath)
        {
            var ret = ApplicationType.NativeAppication;
            var executableFile = applicationPath.Split(' ')[0];
            if (IsNetApplication(executableFile))
            {
                ret = ApplicationType.DotNetAppication;
                if (typeof(ServiceBase).IsAssignableFrom(GetMainClassType(executableFile)))
                    ret = ApplicationType.DotNetService;
            }

            return ret;
        }
    }
}