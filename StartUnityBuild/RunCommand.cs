using System.Diagnostics;
using System.Text;

namespace StartUnityBuild;

public class RunCommand
{
    public RunCommand(string prefix, string fileName, string arguments, Action<string, string> readOutput,
        Action<string, int> readExitCode)
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

        var standardOutput =
            new AsyncStreamReader(process.StandardOutput, data => { readOutput(prefix, data); });
        var standardError = new AsyncStreamReader(process.StandardError, data => { readOutput(null!, data); });
        standardOutput.Start();
        standardError.Start();

        process.WaitForExit();
        readExitCode(prefix, process.ExitCode);
    }

    public void Execute()
    {

    }

    /// <summary>
    /// Stream reader for StandardOutput and StandardError stream readers.<br />
    /// Runs an eternal BeginRead loop on the underlying stream bypassing the stream reader as lines.
    /// </summary>
    private class AsyncStreamReader(StreamReader readerToBypass, Action<string> callback)
    {
        private static readonly char[] Separators = ['\r', '\n'];

        private readonly byte[] _buffer = new byte[4096];
        private readonly StringBuilder _builder = new StringBuilder();

        public void Start()
        {
            BeginReadAsync();
        }

        private void BeginReadAsync()
        {
            readerToBypass.BaseStream.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, null);
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            var bytesRead = readerToBypass.BaseStream.EndRead(asyncResult);

            if (bytesRead == 0)
            {
                if (_builder.Length > 0)
                {
                    callback.Invoke(_builder.ToString());
                }
                callback.Invoke(null!);
                return;
            }
            _builder.Append(readerToBypass.CurrentEncoding.GetString(_buffer, 0, bytesRead));
            var bufferText = _builder.ToString();
            if (bufferText.EndsWith('\r') || bufferText.EndsWith('\n'))
            {
                var lines = bufferText.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    callback.Invoke(line);
                }
                _builder.Clear();
            }
            else if (bufferText.Contains('\r') || bufferText.Contains('\n'))
            {
                var lines = bufferText.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < lines.Length - 1; ++i)
                {
                    callback.Invoke(lines[i]);
                }
                _builder.Clear();
                _builder.Append(lines[..^1]);
            }

            //Wait for more data from stream
            BeginReadAsync();
        }
    }
}
