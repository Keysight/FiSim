using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

using BinInfo.Utils;

namespace BinInfo.MachO {
    public class MachOSourceLineResolver : ISourceLineResolver {
        readonly MachOBinInfo _bin;

        public MachOSourceLineResolver(MachOBinInfo bin) {
            _bin = bin;
        }

        readonly Dictionary<ulong, ISourceLineInfo> _sourceLineInfo = new Dictionary<ulong, ISourceLineInfo>();

        public ISourceLineInfo this[ulong address] {
            get {
                if (!_sourceLineInfo.ContainsKey(address)) {
                    _loadSourceInfo(address);
                }

                return _sourceLineInfo[address];
            }
        }

        void _loadSourceInfo(ulong address) {
            var process = new Process {
                StartInfo = {
                    FileName               = "atos",
                    Arguments              = $"-o \"{CommandLineEncoder.EncodeArgText(_bin.Path)}\" {address:X16}",
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    CreateNoWindow         = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new InvalidOperationException();

            while (!process.StandardOutput.EndOfStream) {
                var line = process.StandardOutput.ReadLine().Trim();

                var match = Regex.Match(line, @"^([a-zA-Z0-9_]+) \((.*)\) (\((.*)\)|\+ [0-9]+)$");

                var funcName = match.Groups[1].Value;
                var binName = match.Groups[2].Value;
                var fileName = match.Groups[4].Value;

                if (fileName.Contains(":")) {
                    match = Regex.Match(fileName, @"^(.+):([0-9]+)$");

                    var filePath   = match.Groups[1].Value;
                    var lineNumber = uint.Parse(match.Groups[2].Value);
                    
                    _sourceLineInfo.Add(address, new SourceLineInfo { FilePath = filePath, LineNumber = lineNumber, FunctionName = funcName });
                }
                else {
                    _sourceLineInfo.Add(address, new SourceLineInfo { FunctionName = funcName });
                }
            }
        }
    }
}