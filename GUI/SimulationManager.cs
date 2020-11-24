using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using BinInfo;
using FiSim;
using PlatformSim;

namespace FiSim.GUI {
    internal class SimulationManager {
        internal SimulationManager() {
            if (File.Exists(Config.BL1ELFPath)) {
                _loadAssemblyToSourceInfo(TextWriter.Null, Config.BL1ELFPath, 0x80000000, 0x80000000);
            }
        }

        internal bool Clean(TextWriter outputWriter) {
            if (!File.Exists( Config.CompilerPath)) {
                outputWriter.WriteLine("Toolchain missing!");

                return false;
            }
            
            var process = new Process();
            process.StartInfo.Environment["Path"] = Path.Combine(Config.CompilerToolchainPath, "bin") + ";" + process.StartInfo.Environment["Path"];
            process.StartInfo.Environment["Path"] = Path.Combine(Config.ToolchainRootPath, "coreutils", "bin") + ";" + process.StartInfo.Environment["Path"];
            process.StartInfo.Environment["Path"] = Path.Combine(Config.ToolchainRootPath, "Python27") + ";" + process.StartInfo.Environment["Path"];
            process.StartInfo.WorkingDirectory = Config.ContentPath;
            process.StartInfo.FileName = Config.MakeCommand;
            process.StartInfo.Arguments = Config.CleanArgument;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
                    
            outputWriter.WriteLine("Path: " + process.StartInfo.Environment["Path"]);
            outputWriter.WriteLine("CMD: " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);

            try {
                process.Start();
                
                var standardOutputTask = Task.Run(() => {
                    while (!process.StandardOutput.EndOfStream) {
                        outputWriter.WriteLine(process.StandardOutput.ReadLine());
                    }
                });
            
                var standardErrorTask = Task.Run(() => {
                    while (!process.StandardError.EndOfStream) {
                        outputWriter.WriteLine(process.StandardError.ReadLine());
                    }
                });
                
                process.WaitForExit();

                standardOutputTask.Wait();
                standardErrorTask.Wait();

                if (process.ExitCode != 0)
                    return false;
            }
            catch (Exception e) {
                outputWriter.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        internal bool CompileBinary(TextWriter outputWriter) {
            if (!File.Exists( Config.CompilerPath)) {
                outputWriter.WriteLine("Toolchain missing!");

                return false;
            }
            
            var process = new Process();
            process.StartInfo.Environment["Path"] = Path.Combine(Config.CompilerToolchainPath, "bin") + ";" + process.StartInfo.Environment["Path"];
            process.StartInfo.Environment["Path"] = Path.Combine(Config.ToolchainRootPath, "coreutils", "bin") + ";" + process.StartInfo.Environment["Path"];
            process.StartInfo.Environment["Path"] = Path.Combine(Config.ToolchainRootPath, "Python27") + ";" + process.StartInfo.Environment["Path"];
            process.StartInfo.WorkingDirectory = Config.ContentPath;
            process.StartInfo.FileName = Config.MakeCommand;
            process.StartInfo.Arguments = Config.MakeArgument;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
                    
            outputWriter.WriteLine("Path: " + process.StartInfo.Environment["Path"]);
            outputWriter.WriteLine("CMD: " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);

            try {
                process.Start();
                
                var standardOutputTask = Task.Run(() => {
                    while (!process.StandardOutput.EndOfStream) {
                        outputWriter.WriteLine(process.StandardOutput.ReadLine());
                    }
                });
            
                var standardErrorTask = Task.Run(() => {
                    while (!process.StandardError.EndOfStream) {
                        outputWriter.WriteLine(process.StandardError.ReadLine());
                    }
                });
                
                process.WaitForExit();

                standardOutputTask.Wait();
                standardErrorTask.Wait();

                if (process.ExitCode != 0)
                    return false;
                
                // Make source to binary map
                if (!_loadAssemblyToSourceInfo(outputWriter, Config.BL1ELFPath, 0x80000000, 0x80000000)) {
                    return false;
                }
            }
            catch (Exception e) {
                outputWriter.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        private IBinInfo _binInfo;
        
        internal ISourceLineInfo LookupSourceLocation(ulong address) => _binInfo.SourceLine[address];
        
        private bool _loadAssemblyToSourceInfo(TextWriter outputWriter, string binaryPath, ulong loadingVa, ulong binaryVa) {
            try {
                _binInfo = BinInfoFactory.GetBinInfo(binaryPath);
            }
            catch (Exception e) {
                outputWriter.WriteLine(e);

                return false;
            }

            return true;
        }

        internal bool RunSimulation(TextWriter outputWriter) {
            if (!File.Exists(Config.EngineExe)) {
                throw new Exception("File not found: " + Config.EngineExe);
            }

            var process = new Process {
                StartInfo = {
                    FileName = Config.EngineExe,
                    Arguments = Config.ProjectName + " R",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var startTime = DateTime.Now;

            try {
                process.Start();

                while (!process.HasExited || (!process.StandardOutput.EndOfStream) || !process.StandardError.EndOfStream) {
                    while (!process.StandardOutput.EndOfStream) {
                        var line = process.StandardOutput.ReadLine();

                        Debugger.Log(0, "", "O:" + line + Environment.NewLine);

                        outputWriter.Write(line + Environment.NewLine);
                    }

                    while (!process.StandardError.EndOfStream) {
                        var line = process.StandardError.ReadLine();

                        Debugger.Log(0, "", "E:" + line + Environment.NewLine);

                        outputWriter.Write(line + Environment.NewLine);
                    }

                    if ((DateTime.Now - startTime).TotalSeconds >= 5) { // Stuck?!
                        Debugger.Log(0, "", $"Stuck" + Environment.NewLine);

                        process.Kill();
                        process.Close();

                        return false;
                    }
                }

                var hadError = process.ExitCode != 0;

                process.Close();
                process.Dispose();

                return !hadError;
            }
            catch (Exception e) {
                outputWriter.WriteLine(e);

                return false;
            }
        }
        
        internal bool VerifyBinary(TextWriter outputWriter) {
            if (!File.Exists(Config.EngineExe)) {
                throw new Exception("File not found: " + Config.EngineExe);
            }

            var process = new Process {
                StartInfo = {
                    FileName = Config.EngineExe,
                    Arguments = Config.ProjectName + " V",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var startTime = DateTime.Now;

            try {
                process.Start();

                while (!process.HasExited || (!process.StandardOutput.EndOfStream) || !process.StandardError.EndOfStream) {
                    while (!process.StandardOutput.EndOfStream) {
                        var line = process.StandardOutput.ReadLine();

                        Debugger.Log(0, "", "O:" + line + Environment.NewLine);

                        outputWriter.Write(line + Environment.NewLine);
                    }

                    while (!process.StandardError.EndOfStream) {
                        var line = process.StandardError.ReadLine();

                        Debugger.Log(0, "", "E:" + line + Environment.NewLine);

                        outputWriter.Write(line + Environment.NewLine);
                    }

                    if ((DateTime.Now - startTime).TotalSeconds >= 10) { // WTF! Stuck?!
                        Debugger.Log(0, "", $"Stuck" + Environment.NewLine);

                        try {
                            process.Kill();
                        }
                        catch (Win32Exception) { }

                        process.Close();

                        return false;
                    }
                }
                
                var hadError = process.ExitCode != 0;

                process.Close();
                process.Dispose();

                return !hadError;
            }
            catch (Exception e) {
                outputWriter.WriteLine(e);

                return false;
            }
        }
        
        
        internal void RunFISimulation(OutputConsoleWriter outputWriter, OnGlitchSimulationCompletedCallback onGlitchSimulationCompletedCallback) {
            if (!File.Exists(Config.EngineExe)) {
                throw new Exception("File not found: " + Config.EngineExe);
            }

            var process = new Process {
                StartInfo = {
                    FileName = Config.EngineExe,
                    Arguments = Config.ProjectName + " fault-gui",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            Debugger.Log(0, "", $"Run: {process.StartInfo.FileName} {process.StartInfo.Arguments}" + Environment.NewLine);

            try {
                var requestedToStop = false;

                process.OutputDataReceived += (sender, args) => {
                    try {
                        if (args.Data != null) {
                            outputWriter.Write(args.Data + Environment.NewLine);
                        }
                    }
                    catch (ObjectDisposedException) {
                    }
                };

                process.Start();
                process.BeginOutputReadLine();

                var watchThread = new Thread(() => {
                    try {
                        while (!process.HasExited) {
                            if (requestedToStop) {
                                Debugger.Log(0, "", $"Stopped" + Environment.NewLine);

                                process.Kill();
                                process.Close();

                                return;
                            }

                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception) {
                    }
                });

                watchThread.IsBackground = true;
                watchThread.Start();

                var firstLine = true;
                ulong totalRuns = 0;

                var namedFaultModels = new Dictionary<string, IFaultModel>();

                while (!process.HasExited || !process.StandardError.EndOfStream) {
                    while (!process.StandardError.EndOfStream) {
                        var line = process.StandardError.ReadLine();

                        if (firstLine) {
                            try {
                                totalRuns = ulong.Parse(line);
                            }
                            catch (Exception e) {
                                outputWriter.WriteLine(e.ToString() + @" - Failed to parse " + line);
                            }

                            firstLine = false;
                        }
                        else if (Regex.IsMatch(line, "([a-zA-Z]*)::([a-zA-Z]*)::([0-9a-zA-Z]{8})::.*::.*")) {
                            var parts = line.Split(new[] { "::" }, StringSplitOptions.None);

                            var modelName = parts[0];
                            var result = (Result) Enum.Parse(typeof(Result), parts[1]);
                            var address = ulong.Parse(parts[2], NumberStyles.AllowHexSpecifier);

                            if (!namedFaultModels.ContainsKey(modelName)) {
                                namedFaultModels.Add(modelName, new NamedFaultModel(modelName));
                            }
                            
                            var faultModel = namedFaultModels[modelName];

                            requestedToStop = onGlitchSimulationCompletedCallback(totalRuns,
                                null,
                                new FaultResult {
                                    Result = result, 
                                    Fault = new InstructionFaultDefinition(faultModel, null, null, parts[3], address)
                                });

                            if (requestedToStop) {
                                Debugger.Log(0, "", $"Stopped" + Environment.NewLine);

                                process.Kill();
                                process.Close();

                                return;
                            }
                        }
                        else if (line.StartsWith("InstructionFetchNopModel") || line.StartsWith("InstructionFetchSingleBitCorruptionModel")) {
                            // ??????
                        }
                        else {
                            Debugger.Log(0, "", $"Unexpected output " + line + Environment.NewLine);

                            outputWriter.Write(line + Environment.NewLine);
                        }
                    }
                }

                Debugger.Log(0, "", $"Exit code: {process.ExitCode}" + Environment.NewLine);

                watchThread.Abort();

                process.Close();
                process.Dispose();
            }
            catch (Exception e) when (!Debugger.IsAttached) {
                outputWriter.WriteLine(e);
            }
        }
    }
}