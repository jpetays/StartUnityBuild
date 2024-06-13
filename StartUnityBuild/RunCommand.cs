using System.Diagnostics;
using System.Text;

namespace StartUnityBuild;

public static class RunCommand
{
    public static int ExecuteBlocking(string prefix, string fileName, string arguments,
        string workingDirectory, Dictionary<string, string>? environmentVariables,
        Action<string, string> readOutput, Action<string, int> readExitCode)
    {
        if (!Directory.Exists(workingDirectory))
        {
            Form1.AddLine("ERROR", $"working directory not found: {workingDirectory}");
            readExitCode(prefix, -1);
            return -1;
        }
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
        };
        if (environmentVariables != null)
        {
            foreach (var pair in environmentVariables)
            {
                startInfo.EnvironmentVariables.Add(pair.Key, pair.Value);
            }
        }
        // Required for redirection
        startInfo.UseShellExecute = false;

        var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            Form1.AddLine("ERROR", "unable start process");
            readExitCode(prefix, -1);
            return -1;
        }
        prefix = $"{prefix}[{process.Id:x}]";
        var errorPrefix = $"ERROR[{process.Id:x}]";
        var standardOutput = new AsyncStreamReader(
            process.StandardOutput, data => { readOutput(prefix, data); }, process.StandardInput);
        var standardError = new AsyncStreamReader(
            process.StandardError, data => { readOutput(errorPrefix!, data); }, null);
        standardOutput.Start();
        standardError.Start();

        Form1.AddLine(".cmd", $"{prefix} started");
        Form1.AddLine(".cmd", $"{prefix} wait");
        Thread.Yield();
        process.WaitForExit();
        Form1.AddLine(".cmd", $"{prefix} ended");
        readExitCode(prefix, process.ExitCode);
        Form1.AddLine(".cmd", $"{prefix} done");
        return process.ExitCode;
    }

    public static void ExecuteAsync(string prefix, string fileName, string arguments, string workingDirectory,
        Action<string, string> readOutput, Action<string, int> readExitCode, Action finished)
    {
        if (!Directory.Exists(workingDirectory))
        {
            Form1.AddLine("ERROR", $"working directory not found: {workingDirectory}");
            readExitCode(prefix, -1);
            finished.Invoke();
            return;
        }
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            CreateNoWindow = true,
            // Required for redirection
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
        };

        var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            Form1.AddLine("ERROR", "unable start process");
            readExitCode(prefix, -1);
            finished.Invoke();
            return;
        }
        prefix = $"{prefix}[{process.Id:x}]";
        var errorPrefix = $"ERROR[{process.Id:x}]";
        var standardOutput = new AsyncStreamReader(
            process.StandardOutput, data => { readOutput(prefix, data); }, process.StandardInput);
        var standardError = new AsyncStreamReader(
            process.StandardError, data => { readOutput(errorPrefix, data); }, null);
        standardOutput.Start();
        standardError.Start();

        Task.Run(() =>
        {
            Form1.AddLine(".cmd", $"{prefix} wait");
            // Let first output (if nay) to arrive first.
            Thread.Sleep(100);
            process.WaitForExit();
            Form1.AddLine(".cmd", $"{prefix} ended");
            readExitCode(prefix, process.ExitCode);
            finished.Invoke();
            Form1.AddLine(".cmd", $"{prefix} done");
        });
        Form1.AddLine(".cmd", $"{prefix} started");
    }

    /// <summary>
    /// Stream reader for StandardOutput and StandardError stream readers.<br />
    /// Runs an eternal BeginRead loop on the underlying stream bypassing the stream reader as lines.
    /// </summary>
    private class AsyncStreamReader(StreamReader readerToBypass, Action<string> callback, StreamWriter? writer)
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
                    var lastBufferText = _builder.ToString();
                    var lines = lastBufferText.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < lines.Length - 1; ++i)
                    {
                        var line = lines[i];
                        if (!HandledAnyKey(line))
                        {
                            callback.Invoke(line);
                        }
                    }
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
                    if (!HandledAnyKey(line))
                    {
                        callback.Invoke(line);
                    }
                }
                _builder.Clear();
            }
            else if (bufferText.Contains('\r') || bufferText.Contains('\n'))
            {
                var lines = bufferText.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < lines.Length - 1; ++i)
                {
                    var line = lines[i];
                    if (!HandledAnyKey(line))
                    {
                        callback.Invoke(line);
                    }
                }
                _builder.Clear();
                var lastLine = lines[^1];
                if (!HandledAnyKey(lastLine))
                {
                    _builder.Append(lastLine);
                }
            }

            //Wait for more data from stream
            BeginReadAsync();
            return;

            bool HandledAnyKey(string text)
            {
                if (!text.StartsWith("Press any key to continue"))
                {
                    return false;
                }
                writer?.Write("Y");
                return true;
            }
        }
    }
}
