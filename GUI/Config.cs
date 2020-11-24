using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using static System.Reflection.Assembly;

namespace FiSim.GUI {
    public static class Config {
        public static RunMode Mode {
            get {
                if (ProjectName == "Example")
                    return RunMode.Demo;
                
                if (File.Exists(Path.Combine(ContentPath, ".minimal"))) {
                    return RunMode.Minimal;                    
                }
                
                return RunMode.Full;
            }
        }

        public static string WindowTitle {
            get {
                switch (Mode) {
                    case RunMode.Demo:
                        return "Riscure FiSim (demo edition)";
                    
                    case RunMode.Minimal:
                        return "Riscure BootSim (training edition)";
                    
                    case RunMode.Full:
                        return "Riscure FiSim (training edition)";

                    default:
                        throw new Exception();
                }
            }
        }

        public static string EngineExe {
            get {
                if (File.Exists("Engine.exe")) {
                    return Path.GetFullPath("Engine.exe");
                }
                
                if (File.Exists("FiSim.Engine.exe")) {
                    return Path.GetFullPath("FiSim.Engine.exe");
                }
                
                MessageBox.Show("Simulation engine cannot be found; please contact inforequest@riscure.com", "Riscure FiSim");
                Environment.Exit(0);
                
                throw new Exception();
            }
        }

        public static string ContentRootPath {
            get {
                if (Directory.Exists("Content")) {
                    return Path.GetFullPath("Content");
                }
                
                if (Directory.Exists("..\\..\\..\\Content")) {
                    return Path.GetFullPath("..\\..\\..\\Content");
                }
                
                MessageBox.Show("Content directory cannot be found; please contact inforequest@riscure.com", "Riscure FiSim");
                Environment.Exit(0);
                
                throw new Exception();
            }
        }

        public static string ProjectName {
            get {
                var assemblyPath = GetExecutingAssembly().Location;
                var assemblyName = Path.GetFullPath(assemblyPath).Split('\\').Last().Replace(".exe", "");

                if (Directory.Exists(Path.Combine(ContentRootPath, assemblyName))) {
                    return assemblyName;
                }

                var args = Environment.GetCommandLineArgs();
                if (args.Length == 2 && Directory.Exists(Path.Combine(ContentRootPath, args[1]))) {
                    return args[1];
                }
                
                if (Directory.Exists(Path.Combine(ContentRootPath, "Example"))) {
                    return "Example";
                }
                
                MessageBox.Show("Exercise directory cannot be found; please contact inforequest@riscure.com", "Riscure FiSim");
                Environment.Exit(0);
                
                throw new Exception();
            }
        }

        public static string ContentPath => Path.Combine(ContentRootPath, ProjectName);

        public static string ToolchainRootPath => Path.Combine(ContentRootPath, "..", "toolchains");
        
        public static string CompilerToolchain => "arm-eabi";
        public static string CompilerToolchainPath => Path.Combine(ToolchainRootPath, CompilerToolchain);
        
        public static string BL1ELFPath => Path.Combine(ContentPath, "bin","aarch32", "bl1.elf");

        public static readonly Dictionary<string, string> DefaultOpenedFiles = new Dictionary<string, string> {
            {"main.c", "src\\main.c"}
        };
        
        public static readonly List<string> HiddenFiles = new List<string> {
            "bin\\aarch32\\obj",
            "bin\\aarch64\\obj",
            "src\\aarch32",
            "src\\aarch64",
            "Makefile.AArch64"
        };

        public static string CompilerPath => Path.Combine(ToolchainRootPath, CompilerToolchain, $"bin\\{CompilerToolchain}-gcc.exe");

        public static string MakeCommand => Path.Combine(ToolchainRootPath, "make", "make.exe");
        public static readonly string MakeArgument = "-f Makefile.AArch32 all";
        public static readonly string CleanArgument = "-f Makefile.AArch32 clean";
    }
}