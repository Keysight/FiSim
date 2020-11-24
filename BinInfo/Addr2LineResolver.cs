using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using BinInfo.ELF;
using BinInfo.Utils;

namespace BinInfo {
    public static class Addr2LineResolver {
        public static Dictionary<ulong, ISourceLineInfo> LoadSourceInfo(ELFBinInfo bin, IEnumerable<ulong> addresses, string toolPath, string toolPrefix, string toolName = "addr2line") {
            var addressBuff = new StringBuilder();

            foreach (var address in addresses) {
                addressBuff.Append($" {address:X16}");
            }

            var addr2linePath = Path.GetFullPath(Path.Combine(toolPath, toolPrefix + toolName));

            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                addr2linePath = addr2linePath + ".exe";
            }

            if (!File.Exists(addr2linePath)) {
                throw new Exception("addr2line missing: " + addr2linePath);
            }
            
            var process = new Process {
                StartInfo = {
                    FileName               = addr2linePath,
                    Arguments              = $"-a -p -f -e \"{CommandLineEncoder.EncodeArgText(bin.Path)}\" {addressBuff}",
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

            var sourceLineInfo = new Dictionary<ulong, ISourceLineInfo>();

            while (!process.StandardOutput.EndOfStream) {
                var line = process.StandardOutput.ReadLine().Trim();
                
                if (Regex.IsMatch(line, @"^0x([0-9a-fA-F]{8,16}): .*")) {
                    var addrMatch = Regex.Match(line, @"^0x([0-9a-fA-F]{8,16}): .*");
                    
                    var address = ulong.Parse(addrMatch.Groups[1].Value, NumberStyles.AllowHexSpecifier);

                    if (Regex.IsMatch(line, @"^0x([0-9a-fA-F]{8,16}): ([a-zA-Z0-9_]+) at (.*)$")) {
                        var nameMatch = Regex.Match(line, @"^0x([0-9a-fA-F]{8,16}): ([a-zA-Z0-9_]+) at (.*)$");
                        
                        var funcName = nameMatch.Groups[2].Value;
                        var fileName = nameMatch.Groups[3].Value;

                        if (fileName.Contains(":")) {                            
                            nameMatch = Regex.Match(fileName, @"^(.+):(\?|[0-9]+)(.*)$");
                            
                            var filePath = nameMatch.Groups[1].Value;
                            
                            filePath = new FileInfo(filePath).FullName;

                            if (filePath.StartsWith(Directory.GetCurrentDirectory())) {
                                filePath = filePath.Substring(Directory.GetCurrentDirectory().Length + 1);
                            }
                            
                            if (nameMatch.Groups[2].Value.Length > 0 && nameMatch.Groups[2].Value != "?") {
                                var lineNumber = uint.Parse(nameMatch.Groups[2].Value);
                                
                                sourceLineInfo.Add(address, new SourceLineInfo {FilePath = filePath, LineNumber = lineNumber, FunctionName = funcName});
                            }
                            else {
                                sourceLineInfo.Add(address, new SourceLineInfo {FilePath = filePath, FunctionName = funcName});
                            }
                        }
                        else {
                            sourceLineInfo.Add(address, new SourceLineInfo {FilePath = fileName});
                        }
                    }
                    else {
                        throw new KeyNotFoundException($"Cannot resolve {address:X16}");
                    }
                }
            }

            return sourceLineInfo;
        }
    }
}
