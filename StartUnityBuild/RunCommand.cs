using System.Diagnostics;

namespace StartUnityBuild;

public class RunCommand
{
    public RunCommand(string fileName, string arguments, Action<int, string> readOutput, Action<int> readExitCode)
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutput = new AsyncStreamReader(process.StandardOutput, (sender, data) => { readOutput(1, data); });
        var standardError = new AsyncStreamReader(process.StandardError, (sender, data) => { readOutput(2, data); });
        standardOutput.Start();
        standardError.Start();

        process.WaitForExit();
        readExitCode(process.ExitCode);
    }

    /// <summary>
    /// Stream reader for StandardOutput and StandardError stream readers
    /// Runs an eternal BeginRead loop on the underlaying stream bypassing the stream reader.
    ///
    /// The TextReceived sends data received on the stream in non delimited chunks. Event subscriber can
    /// then split on newline characters etc as desired.
    /// </summary>
    private class AsyncStreamReader
    {
        public delegate void EventHandler<T>(object sender, string Data);

        public event EventHandler<string> DataReceived;

        protected readonly byte[] buffer = new byte[4096];
        private StreamReader reader;

        public bool Active { get; private set; }

        public void Start()
        {
            if (!Active)
            {
                Active = true;
                BeginReadAsync();
            }
        }

        public void Stop()
        {
            Active = false;
        }

        public AsyncStreamReader(StreamReader readerToBypass, EventHandler<string> callback)
        {
            reader = readerToBypass;
            Active = false;
            DataReceived = callback;
        }

        private void BeginReadAsync()
        {
            if (this.Active)
            {
                reader.BaseStream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(ReadCallback), null);
            }
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            var bytesRead = reader.BaseStream.EndRead(asyncResult);

            string data = null;

            //Terminate async processing if callback has no bytes
            if (bytesRead > 0)
            {
                data = reader.CurrentEncoding.GetString(this.buffer, 0, bytesRead);
            }
            else
            {
                //callback without data - stop async
                this.Active = false;
            }

            //Send data to event subscriber - null if no longer active
            if (this.DataReceived != null)
            {
                this.DataReceived.Invoke(this, data);
            }

            //Wait for more data from stream
            this.BeginReadAsync();
        }
    }
}
