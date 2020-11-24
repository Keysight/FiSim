using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using BinInfo.Utils;

namespace BinInfo {
    public static class NMSymbolResolver {
        public static Dictionary<string, List<ISymbolInfo>> LoadSymbolInfo(IBinInfo binPath, string toolPath = "", string toolPrefix = "", string toolName = "nm") {
            var nmPath = Path.GetFullPath(Path.Combine(toolPath, toolPrefix + toolName));
            
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                nmPath = nmPath + ".exe";
            }
            
            Console.WriteLine(binPath.Path);

            if (!File.Exists(nmPath)) {
                throw new Exception("nm missing: " + nmPath);
            }
            
            var process = new Process {
                StartInfo = {
                    FileName               = nmPath,
                    Arguments              = $"-S \"{CommandLineEncoder.EncodeArgText(binPath.Path)}\"",
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    CreateNoWindow         = true
                }
            };

            process.Start();

            var symbolInfo = new Dictionary<string, List<ISymbolInfo>>();

            while (!process.StandardOutput.EndOfStream) {
                var line = process.StandardOutput.ReadLine()?.Trim();
                
                var matchWithSize = Regex.Match(line, @"^([0-9a-fA-F]+) ([0-9a-fA-F]+) ([a-zA-Z]) ([a-zA-Z0-9_]+)$");
                var matchWithoutSize = Regex.Match(line, @"^([0-9a-fA-F]+) ([a-zA-Z]) ([a-zA-Z0-9_]+)$");

                if (matchWithSize.Success) {
                    var address  = ulong.Parse(matchWithSize.Groups[1].Value, NumberStyles.HexNumber);
                    var size  = ulong.Parse(matchWithSize.Groups[2].Value, NumberStyles.HexNumber);
                    var funcName = matchWithSize.Groups[4].Value;
                    
                    if (!symbolInfo.ContainsKey(funcName)) {
                        symbolInfo.Add(funcName, new List<ISymbolInfo>());
                    }

                    symbolInfo[funcName].Add(new SymbolInfo {Address = address, Size = size, Name = funcName});
                }
                else if (matchWithoutSize.Success) {
                    var address = ulong.Parse(matchWithoutSize.Groups[1].Value, NumberStyles.HexNumber);
                    var funcName = matchWithoutSize.Groups[3].Value;
                    
                    if (!symbolInfo.ContainsKey(funcName)) {
                        symbolInfo.Add(funcName, new List<ISymbolInfo>());
                    }

                    symbolInfo[funcName].Add(new SymbolInfo { Address = address, Size = 0, Name = funcName });
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new InvalidOperationException();

            return symbolInfo;
        }
    }
}
