using System;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Process.ProcessInfo.Imp;

namespace DotNetAidLib.Core.Process.ProcessInfo.Core
{
    public abstract class ProcessInfo
    {
        protected string command;
        protected decimal cpu;
        protected bool isRunning;
        protected decimal memory;
        protected string name;
        protected int parentPID;
        protected int pID;
        protected DateTime startTime;
        protected string user;
        protected TimeSpan useTime;

        public ProcessInfo(int processId)
        {
            pID = processId;
        }

        public int PID => pID;
        public int ParentPID => parentPID;
        public string Name => name;
        public string User => user;
        public bool IsRunning => isRunning;
        public string Command => command;
        public decimal CPU => cpu;
        public decimal Memory => memory;
        public DateTime StartTime => startTime;
        public TimeSpan UseTime => useTime;
        public abstract ProcessInfo Refresh();
        public abstract void Send(int signal);


        public static ProcessInfo[] GetProcessesFromName(string processName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsProcessInfo.GetProcessesFromName(processName);
            return PosixProcessInfo.GetProcessesFromName(processName);
        }

        public static ProcessInfo GetProcess(int processId)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsProcessInfo.GetProcess(processId);
            return PosixProcessInfo.GetProcess(processId);
        }

        public void Terminate(bool force)
        {
            if (!force)
                Send(ProcessSignals.SIGTERM);
            else
                Send(ProcessSignals.SIGKILL);
        }

        public override string ToString()
        {
            if (!isRunning)
                return "PID (not running): " + pID;
            return "PID (running): " + pID
                                     + ", PPID: " + parentPID
                                     + ", Name: " + name
                                     + ", User: " + user
                                     + ", CPU: " + cpu
                                     + ", MEM: " + memory
                                     + ", STIME: " + startTime
                                     + ", UTIME: " + useTime
                                     + ", CMD: " + command;
        }

        public static class ProcessSignals
        {
            public const int SIGHUP = 1;
            public const int SIGINT = 2;
            public const int SIGQUIT = 3;
            public const int SIGILL = 4;
            public const int SIGTRAP = 5;
            public const int SIGABRT = 6;
            public const int SIGBUS = 7;
            public const int SIGFPE = 8;
            public const int SIGKILL = 9;
            public const int SIGUSR1 = 10;
            public const int SIGSEGV = 11;
            public const int SIGUSR2 = 12;
            public const int SIGPIPE = 13;
            public const int SIGALRM = 14;
            public const int SIGTERM = 15;
            public const int SIGSTKFLT = 16;
            public const int SIGCHLD = 17;
            public const int SIGCONT = 18;
            public const int SIGSTOP = 19;
            public const int SIGTSTP = 20;
            public const int SIGTTIN = 21;
            public const int SIGTTOU = 22;
            public const int SIGURG = 23;
            public const int SIGXCPU = 24;
            public const int SIGXFSZ = 25;
            public const int SIGVTALRM = 26;
            public const int SIGPROF = 27;
            public const int SIGWINCH = 28;
            public const int SIGIO = 29;
            public const int SIGPWR = 30;
            public const int SIGSYS = 31;
            public const int SIGRTMIN = 34;
            public const int SIGRTMAX = 64;
        }
    }
}