using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

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
        public static void ExecuteTimeout(Action action, int timeoutMs) {
            ExecuteTimeout(action, TimeSpan.FromMilliseconds(timeoutMs));
        }

        public static void ExecuteTimeout(Action action, TimeSpan timeout)
        {
            Thread taskThread = null;

            DateTime d = DateTime.Now;
            Task.Factory.StartNew(() => {
                taskThread = Thread.CurrentThread;
                action.Invoke();
            });

            while (taskThread.IsAlive && (DateTime.Now.Subtract(d) < timeout))
                Thread.Sleep(1);

            if (taskThread.IsAlive)
            {
                taskThread.Abort();
                throw new System.TimeoutException();
            }

        }

        public static System.Diagnostics.Process FindProcessById(long id)
		{
			System.Diagnostics.Process ret = null;

			System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcesses();

			foreach (System.Diagnostics.Process proc in procs) {
				if ((id == proc.Id)) {
					ret = proc;
				}

				if (((ret != null))) {
					break; // TODO: might not be correct. Was : Exit For
				}
			}

			return ret;
		}

		public static System.Diagnostics.Process FindProcessByName(string name)
		{
			System.Diagnostics.Process ret = null;

			System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcesses();

			foreach (System.Diagnostics.Process proc in procs) {
				if (name == proc.ProcessName) {
					ret = proc;
				}

				if (ret != null) {
					break; // TODO: might not be correct. Was : Exit For
				}
			}

			return ret;
		}

		public static System.Diagnostics.Process FindProcessByFileName(string fileName)
		{
			System.Diagnostics.Process ret = null;

			System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcesses();

			foreach (System.Diagnostics.Process proc in procs) {
				if ((fileName == GetProcessFileName(proc))) {
					ret = proc;
				}

				if (((ret != null))) {
					break; // TODO: might not be correct. Was : Exit For
				}
			}

			return ret;
		}

		public static System.Diagnostics.Process FindProcessByFilePath(string filePath)
		{
			System.Diagnostics.Process ret = null;

			System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcesses();

			foreach (System.Diagnostics.Process proc in procs) {
				if ((filePath == GetProcessFilePath(proc))) {
					ret = proc;
				}

				if (((ret != null))) {
					break; // TODO: might not be correct. Was : Exit For
				}
			}

			return ret;
		}

		public static System.Diagnostics.Process ExecuteProcessIfIsRunningProcessId(int runningProcessId, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			while (FindProcessById(runningProcessId) == null) {
				System.Threading.Thread.Sleep(1);
			}
			return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
		}

		public static System.Diagnostics.Process ExecuteProcessIfIsRunningProcessName(string runningProcessName, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			while (FindProcessByName(runningProcessName) == null) {
				System.Threading.Thread.Sleep(1);
			}
			return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
		}

		public static System.Diagnostics.Process ExecuteProcessIfIsRunningProcessFileName(string runningProcessFileName, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			while (FindProcessByFileName(runningProcessFileName) == null) {
				System.Threading.Thread.Sleep(1);
			}
			return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
		}

		public static System.Diagnostics.Process ExecuteProcessIfIsRunningProcessFilePath(string runningProcessFilePath, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			while (FindProcessByFilePath(runningProcessFilePath) == null) {
				System.Threading.Thread.Sleep(1);
			}
			return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
		}

		public static System.Diagnostics.Process ExecuteProcessAssertByRunningProcessId(int runningProcessId, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			if (((FindProcessById(runningProcessId) != null))) {
				return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
			} else {
				throw new Exception("Running Process Id '" + runningProcessId + "' is not running.");
			}
		}

		public static System.Diagnostics.Process ExecuteProcessAssertByRunningProcessName(string runningProcessName, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			if (((FindProcessByName(runningProcessName) != null))) {
				return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
			} else {
				throw new Exception("Running Process Name '" + runningProcessName + "' is not running.");
			}
		}

		public static System.Diagnostics.Process ExecuteProcessAssertByRunningProcessFileName(string runningProcessFileName, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			if (((FindProcessByFileName(runningProcessFileName) != null))) {
				return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
			} else {
				throw new Exception("Running Process File Name '" + runningProcessFileName + "' is not running.");
			}
		}

		public static System.Diagnostics.Process ExecuteProcessAssertByRunningProcessFilePath(string runningProcessFilePath, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			if (((FindProcessByFilePath(runningProcessFilePath) != null))) {
				return ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
			} else {
				throw new Exception("Running Process File Path '" + runningProcessFilePath + "' is not running.");
			}
		}

		public static System.Diagnostics.Process CreateProcess(string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			FileInfo fi = new FileInfo(executablePath);
			return CreateProcess(fi.Directory.FullName, executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
		}

		public static System.Diagnostics.Process CreateProcess(string workingDirectory, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			ProcessStartInfo psi = null;
			if ((args == null)) {
				psi = new ProcessStartInfo(executablePath);
			} else {
				psi = new ProcessStartInfo(executablePath, args);
			}

			psi.WorkingDirectory = workingDirectory;
			psi.RedirectStandardInput = RedirectStandardInput;
			psi.RedirectStandardOutput = RedirectStandardOutput;

			psi.UseShellExecute = UseShellExecute;
			psi.CreateNoWindow = CreateNoWindows;

			System.Diagnostics.Process ret = new System.Diagnostics.Process();
			ret.StartInfo = psi;

			return ret;
		}

		public static void ExecuteProcesses(System.Diagnostics.Process[] processes, int msDelay)
		{
			foreach (System.Diagnostics.Process proc in processes) {
				System.Threading.Thread.Sleep(msDelay);
				proc.Start();
			}
		}

		public static System.Diagnostics.Process ExecuteProcess(string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			System.Diagnostics.Process ret = CreateProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
			ret.Start();
			return ret;
		}

		public static string GetProcessFilePath(System.Diagnostics.Process proc)
		{
			try {
				return proc.MainModule.FileName;
			} catch {
				return null;
			}
		}

		public static string GetProcessFileName(System.Diagnostics.Process proc)
		{
			try {
				FileInfo fi = new FileInfo(proc.MainModule.FileName);
				return fi.Name;
			} catch {
				return null;
			}
		}

        public static bool IsNetApplication(String executablePath)
        {
            try
            {
                System.Reflection.Assembly.LoadFrom(executablePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Type GetMainClassType(String executablePath)
        {
            try
            {
                System.Reflection.Assembly fileAssembly = System.Reflection.Assembly.LoadFrom(executablePath);
                System.Reflection.MethodInfo mainMethod = fileAssembly.EntryPoint;
                if (mainMethod != null)
                    return mainMethod.DeclaringType;

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static ApplicationType GetApplicationType(String applicationPath)
        {
            ApplicationType ret = ApplicationType.NativeAppication;
            String executableFile = applicationPath.Split(' ')[0];
            if (ProcessHelper.IsNetApplication(executableFile))
            {
                ret = ApplicationType.DotNetAppication;
                if (typeof(ServiceBase).IsAssignableFrom(ProcessHelper.GetMainClassType(executableFile)))
                    ret = ApplicationType.DotNetService;
            }
            return ret;
        }
    }
}

