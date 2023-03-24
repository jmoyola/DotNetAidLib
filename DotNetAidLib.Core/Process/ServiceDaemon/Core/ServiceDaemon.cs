using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using DotNetAidLib.Core.Cmd;

namespace DotNetAidLib.Core.Process.ServiceDaemon.Core
{
    public abstract class ServiceDaemon
    {
        private string id;

        public ServiceDaemon(String id){
            this.id = id;
        }

        public String ID
        {
            get
            {
                return id;
            }
        }
        public abstract String Name { get; }

        public abstract String Description { get; }

        public abstract string ConfigPath { get; }

        public abstract void Enable();

        public abstract void Disable();

        public abstract void Mask();

        public abstract void Unmask();

        public abstract void Start(int timeoutMs);

        public abstract void Stop(int timeoutMs);

        public void Restart(int timeoutMs)
        {
            this.Stop(timeoutMs);
            this.Start(timeoutMs);
        }

        public abstract void Pause(int timeoutMs);

        public abstract void Resume(int timeoutMs);

        public abstract bool IsServiceInstalled { get; }

        public abstract String ExecutablePath { get; }

        public abstract ServiceControllerStatus Status { get; }
        public abstract IEnumerable<ServiceDaemon> ServicesDepended { get; }

        public virtual String StatusDescription {
            get{
                return null;
            }
        }

        public override string ToString()
        {
            return this.ID;
        }

        protected static String ResolvExecutablePath(String execPath){
            String ret = execPath;

            if (ret.RegexIsMatch(@"mono-?service", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) // Si es un servicio mono, devolvemos la verdadera linea de comando
            {
                CmdParameters cmd = new CmdParameters(execPath);
                ret =cmd.Values.ToList().LastOrDefault();
            }

            return ret;
        }
    }
}
