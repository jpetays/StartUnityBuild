using System.Diagnostics;
using System.Text;

namespace StartUnityBuild.Commands;

/// <summary>
/// Async task support to execute system commands and wait for their outcome (while listen ing stdout and stderr).
/// </summary>
public static class RunCommand
{
    public static async Task<int> Execute(string prefix, string fileName, string arguments,
        string workingDirectory, Dictionary<string, string> environmentVariables,
        Action<string, string> readOutput, Action<string, string, int> readExitCode)
    {
        if (!Directory.Exists(workingDirectory))
        {
            Form1.AddLine("ERROR", $"working directory not found: {workingDirectory}");
            readExitCode("ERROR", prefix, -1);
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
            readExitCode("ERROR", prefix, -1);
            return -1;
        }
        var linePrefix = $"[{process.Id:x}]";
        var outLines = 0;
        var printOutMessage = Args.Instance.IsTesting;
        var errLines = 0;
        var standardOutput = new AsyncStreamReader(process.StandardOutput,
            line => { CountAndFilter(ref outLines, ref printOutMessage, line); });
        var standardError = new AsyncStreamReader(process.StandardError,
            line => { CountAndFilter(ref errLines, ref printOutMessage, line); });
        standardOutput.Start();
        standardError.Start();

        Thread.Yield();
        await process.WaitForExitAsync();
        // We call blocking version to force flush and/or wait for stdout and stderr streams to be fully closed!
        // ReSharper disable once MethodHasAsyncOverload
        process.WaitForExit();
        if (!standardOutput.IsEndOfStream)
        {
            while (!standardOutput.IsEndOfStream)
            {
                Thread.Yield();
            }
        }
        if (!standardError.IsEndOfStream)
        {
            while (!standardError.IsEndOfStream)
            {
                Thread.Yield();
            }
        }
        if (Args.Instance.IsTesting && (outLines > 0 || errLines > 0))
        {
            Form1.AddLine($".{linePrefix}", $"{prefix} end");
        }
        readExitCode(linePrefix, prefix, process.ExitCode);
        return process.ExitCode;

        void CountAndFilter(ref int counter, ref bool printStartMessage, string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }
            if (printStartMessage)
            {
                printStartMessage = false;
                Form1.AddLine($".{linePrefix}", $"{prefix} start");
            }
            counter += 1;
            readOutput(linePrefix, line);
        }
    }

    /// <summary>
    /// Stream reader for StandardOutput and StandardError stream readers.<br />
    /// Runs an eternal BeginRead loop on the underlying stream bypassing the stream reader as lines.
    /// </summary>
    private class AsyncStreamReader(StreamReader readerToBypass, Action<string> callback)
    {
        private static readonly char[] Separators = ['\r', '\n'];

        private readonly byte[] _buffer = new byte[4096];
        private readonly StringBuilder _builder = new();

        public bool IsEndOfStream;

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
                        callback.Invoke(line);
                    }
                }
                IsEndOfStream = true;
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
                    var line = lines[i];
                    callback.Invoke(line);
                }
                _builder.Clear();
                var lastLine = lines[^1];
                _builder.Append(lastLine);
            }

            //Wait for more data from stream
            BeginReadAsync();
        }
    }
}
