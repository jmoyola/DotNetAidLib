using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Proc;
using DotNetAidLib.Core.Streams;


namespace DotNetAidLib.Core.Process
{
    public class ResponseReceivedEventArgs : EventArgs {
        private String response;
        public ResponseReceivedEventArgs(String response) {
            this.response = response;
        }

        public String Response { get { return this.response; } }
    }
    public delegate void ResponseReceivedEventHalndled(Object sender, ResponseReceivedEventArgs args);
    public class InteractiveProcess:IDisposable
    {
        private System.Diagnostics.Process process;
        public ResponseReceivedEventHalndled ResponseReceivedEvent;

        private InteractiveProcess(System.Diagnostics.Process process)
        {
            Assert.NotNull(process, nameof(process));
            this.process = process;
        }

        protected void OnResponseReceivedEvent(ResponseReceivedEventArgs args) {
            if (this.ResponseReceivedEvent != null)
                this.ResponseReceivedEvent(this, args);
        }

        public bool Running { get { return !this.process.HasExited; } }

        public void Start() {
            this.process.Start();
        }


        public System.Diagnostics.Process Process
        {
            get
            {
                return process;
            }
        }

        public void SendCommand(String command) {
            this.process.StandardInput.WriteLine(command);
        }

        public String Expect(int timeoutMs)
        {
            TimeOutWatchDog to = new TimeOutWatchDog(timeoutMs);
            StringBuilder ret = new StringBuilder();
            char[] buffer=new char[80];

            do
            {

                if (to.IsTimeOut(false, false))
                    break;
                    
                Task<int> rl = this.process.StandardOutput.ReadAsync(buffer, 0, buffer.Length);
                while (!rl.IsCompleted)
                {
                    if (to.IsTimeOut(false, false))
                        break;
                }
                if (rl.IsCompleted)
                {
                    String content=new string( buffer, 0, rl.Result);

                    ret.Append(content);
                    this.OnResponseReceivedEvent(new ResponseReceivedEventArgs(content));
                }
                else
                    break;

            } while (true);

            return ret.ToString();

        }

        public String ExpectLines(int timeoutMs) {
            TimeOutWatchDog to = new TimeOutWatchDog(timeoutMs);
            StringBuilder ret = new StringBuilder();
            String line;

            do
            {

                if (to.IsTimeOut(false, false))
                    break;

                line = null;
                Task<String> rl = this.process.StandardOutput.ReadLineAsync();
                while (!rl.IsCompleted) {
                    if (to.IsTimeOut(false, false))
                        break;
                }
                if (rl.IsCompleted)
                {
                    line = rl.Result;

                    ret.AppendLine(line);
                    this.OnResponseReceivedEvent(new ResponseReceivedEventArgs(line));
                }
                else
                    break;

            } while (true);

            return ret.ToString();

        }

        public String ExpectLines(String endLinePattern, RegexOptions options=RegexOptions.None) {
            StringBuilder ret = new StringBuilder();
            String line;

            do
            {
                line = this.process.StandardOutput.ReadLine(1000);
                ret.AppendLine(line);
                this.OnResponseReceivedEvent(new ResponseReceivedEventArgs(line));
            } while (!line.RegexIsMatch(endLinePattern, options));

            return ret.ToString();
        }

        public static InteractiveProcess Instance(String fileName, String arguments=null) {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            process.StartInfo = psi;
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.Arguments = arguments;
            psi.FileName = fileName;

            return new InteractiveProcess(process);
        }

        public void Dispose()
        {
            this.process.Dispose();
        }
    }
}
