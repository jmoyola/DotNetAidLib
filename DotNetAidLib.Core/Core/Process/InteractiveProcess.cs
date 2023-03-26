using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Proc;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Process
{
    public class ResponseReceivedEventArgs : EventArgs
    {
        public ResponseReceivedEventArgs(string response)
        {
            Response = response;
        }

        public string Response { get; }
    }

    public delegate void ResponseReceivedEventHalndled(object sender, ResponseReceivedEventArgs args);

    public class InteractiveProcess : IDisposable
    {
        public ResponseReceivedEventHalndled ResponseReceivedEvent;

        private InteractiveProcess(System.Diagnostics.Process process)
        {
            Assert.NotNull(process, nameof(process));
            Process = process;
        }

        public bool Running => !Process.HasExited;


        public System.Diagnostics.Process Process { get; }

        public void Dispose()
        {
            Process.Dispose();
        }

        protected void OnResponseReceivedEvent(ResponseReceivedEventArgs args)
        {
            if (ResponseReceivedEvent != null)
                ResponseReceivedEvent(this, args);
        }

        public void Start()
        {
            Process.Start();
        }

        public void SendCommand(string command)
        {
            Process.StandardInput.WriteLine(command);
        }

        public string Expect(int timeoutMs)
        {
            var to = new TimeOutWatchDog(timeoutMs);
            var ret = new StringBuilder();
            var buffer = new char[80];

            do
            {
                if (to.IsTimeOut(false, false))
                    break;

                var rl = Process.StandardOutput.ReadAsync(buffer, 0, buffer.Length);
                while (!rl.IsCompleted)
                    if (to.IsTimeOut(false, false))
                        break;
                if (rl.IsCompleted)
                {
                    var content = new string(buffer, 0, rl.Result);

                    ret.Append(content);
                    OnResponseReceivedEvent(new ResponseReceivedEventArgs(content));
                }
                else
                {
                    break;
                }
            } while (true);

            return ret.ToString();
        }

        public string ExpectLines(int timeoutMs)
        {
            var to = new TimeOutWatchDog(timeoutMs);
            var ret = new StringBuilder();
            string line;

            do
            {
                if (to.IsTimeOut(false, false))
                    break;

                line = null;
                var rl = Process.StandardOutput.ReadLineAsync();
                while (!rl.IsCompleted)
                    if (to.IsTimeOut(false, false))
                        break;
                if (rl.IsCompleted)
                {
                    line = rl.Result;

                    ret.AppendLine(line);
                    OnResponseReceivedEvent(new ResponseReceivedEventArgs(line));
                }
                else
                {
                    break;
                }
            } while (true);

            return ret.ToString();
        }

        public string ExpectLines(string endLinePattern, RegexOptions options = RegexOptions.None)
        {
            var ret = new StringBuilder();
            string line;

            do
            {
                line = Process.StandardOutput.ReadLine(1000);
                ret.AppendLine(line);
                OnResponseReceivedEvent(new ResponseReceivedEventArgs(line));
            } while (!line.RegexIsMatch(endLinePattern, options));

            return ret.ToString();
        }

        public static InteractiveProcess Instance(string fileName, string arguments = null)
        {
            var process = new System.Diagnostics.Process();
            var psi = new ProcessStartInfo();
            process.StartInfo = psi;
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.Arguments = arguments;
            psi.FileName = fileName;

            return new InteractiveProcess(process);
        }
    }
}