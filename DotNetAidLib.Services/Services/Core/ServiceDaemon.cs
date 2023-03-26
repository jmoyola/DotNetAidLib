using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Cmd;

namespace DotNetAidLib.Services.Core
{
    public abstract class ServiceDaemon
    {
        public ServiceDaemon(string id)
        {
            ID = id;
        }

        public string ID { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string ConfigPath { get; }

        public abstract bool IsServiceInstalled { get; }

        public abstract string ExecutablePath { get; }

        public abstract ServiceControllerStatus Status { get; }
        public abstract IEnumerable<ServiceDaemon> ServicesDepended { get; }

        public virtual string StatusDescription => null;

        public abstract void Enable();

        public abstract void Disable();

        public abstract void Mask();

        public abstract void Unmask();

        public abstract void Start(int timeoutMs);

        public abstract void Stop(int timeoutMs);

        public void Restart(int timeoutMs)
        {
            Stop(timeoutMs);
            Start(timeoutMs);
        }

        public abstract void Pause(int timeoutMs);

        public abstract void Resume(int timeoutMs);

        public override string ToString()
        {
            return ID;
        }

        protected static string ResolvExecutablePath(string execPath)
        {
            var ret = execPath;

            if (ret.RegexIsMatch(@"mono-?service",
                    RegexOptions.IgnoreCase)) // Si es un servicio mono, devolvemos la verdadera linea de comando
            {
                var cmd = new CmdParameters(execPath);
                ret = cmd.Values.ToList().LastOrDefault();
            }

            return ret;
        }
    }
}