using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Process.ProcessInfo.Imp;

namespace DotNetAidLib.Core.Process.ProcessInfo.Core
{
    public abstract class ProcessInfo
    {
        protected int pID;
        protected int parentPID;
        protected String name;
        protected String user;
        protected bool isRunning;
        protected String command;
        protected decimal cpu;
        protected decimal memory;
        protected DateTime startTime;
        protected TimeSpan useTime;

        public ProcessInfo(int processId)
        {
            this.pID = processId;
        }

        public int PID { get { return this.pID; } }
        public int ParentPID { get { return this.parentPID; } }
        public String Name { get { return this.name; } }
        public String User { get { return this.user; } }
        public bool IsRunning { get { return this.isRunning; } }
        public String Command { get { return this.command; } }
        public decimal CPU { get { return this.cpu; } }
        public decimal Memory { get { return this.memory; } }
        public DateTime StartTime { get { return this.startTime; } }
        public TimeSpan UseTime { get { return this.useTime; } }
        public abstract ProcessInfo Refresh();
        public abstract void Send(int signal);


        public static ProcessInfo[] GetProcessesFromName(String processName){
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsProcessInfo.GetProcessesFromName(processName);
            else
                return PosixProcessInfo.GetProcessesFromName(processName);
        }

        public static ProcessInfo GetProcess(int processId)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsProcessInfo.GetProcess(processId);
            else
                return PosixProcessInfo.GetProcess(processId);
        }

        public void Terminate(bool force) {
            if(!force)
                this.Send(ProcessSignals.SIGTERM);
            else
                this.Send(ProcessSignals.SIGKILL);
        }

        public override string ToString()
        {
            if (!this.isRunning)
                return "PID (not running): " + this.pID;
            else
                return "PID (running): " + this.pID
                + ", PPID: " + this.parentPID
                + ", Name: " + this.name
                + ", User: " + this.user
                + ", CPU: " + this.cpu
                + ", MEM: " + this.memory
                + ", STIME: " + this.startTime
                + ", UTIME: " + this.useTime
                + ", CMD: " + this.command;
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
