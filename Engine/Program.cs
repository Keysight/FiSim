using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using BinInfo;
using BinInfo.ELF;

using FiSim.FaultModels;

using PlatformSim;
using PlatformSim.HwPeripherals;
using PlatformSim.Simulation;
using PlatformSim.Simulation.Platform.AArch32;

using UnicornManaged.Const;

using Trace = PlatformSim.Trace;

namespace FiSim.Engine {
    internal class Program {
        class MyConfig : Config {
            public bool UseAltData;

            public MyConfig(MyConfig config = null) : base(config) {
            }

            public override Config Clone() => new MyConfig(this) { UseAltData = UseAltData };
        }
        
        public static void Main(string[] args) {
            _initProcess(); 
            
            var projectName = "Example";
            var runMode = RunMode.NormalExecution;
            
            if (args.Length == 2) {
                projectName = args[0];

                switch (args[1].ToLower()) {
                    case "r":
                    case "run":
                        runMode = RunMode.NormalExecution;
                        break;
                    
                    case "v":
                    case "verify":
                        runMode = RunMode.VerifyExecution;
                        break;
                    
                    case "fault":
                        runMode = RunMode.FaultSimTUI;
                        break;
                    
                    case "fault-gui":
                        runMode = RunMode.FaultSimGUI;
                        break;
                    
                    default:
                        throw new Exception("Unknown run mode \""+args[1]+"\" provided!"); 
                }
            }

            string rootPath;

            if (Directory.Exists("Content")) {
                rootPath = Path.GetFullPath("Content");
            }
            else if (Directory.Exists("../../../Content")) {
                rootPath = Path.GetFullPath("../../../Content");
            }
            else {
                throw new Exception(
                    $"Cannot find exercise folder ({Path.GetFullPath("Content")} or {Path.GetFullPath("../../../Content")})");
            }
            
            var projectPath = Path.Combine(rootPath, projectName);
            
            if (!Directory.Exists(Path.Combine(projectPath, "bin"))) {
                throw new Exception("Project path missing: " + projectPath);
            }

            var flashBin = File.ReadAllBytes(Path.Combine(projectPath, "bin/aarch32/bl1.bin"));
            var binInfo = BinInfoFactory.GetBinInfo(Path.Combine(projectPath, "bin/aarch32/bl1.elf"));

            var otpPeripheral = new OtpPeripheral(Path.Combine(projectPath, "bin/otp.bin"));

            if (!File.Exists(Path.Combine(projectPath, "bin/otp.bin"))) {
                byte[] defaultOtpContent = {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x72, 0x67, 0x44, 0xC0,
                    0x80, 0x7D, 0xA5, 0x82, 0xD5, 0xEA, 0xB0, 0xF7, 0xFA, 0x68, 0xD1, 0x8B,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                };
                
                File.WriteAllBytes(Path.Combine(projectPath, "bin/otp.bin"), defaultOtpContent);
            }
            
            var simConfig = new MyConfig {
                Platform = Architecture.AArch32,
                EntryPoint = binInfo.Symbols["_start"].Address,
                StackBase = 0x80100000,
                MaxInstructions = 20000000,
                AddressSpace = new AddressSpace {
                    // OTP
                    { 0x12000000, otpPeripheral },
                    
                    // Next boot stage mem
                    { 0x32000000, new MemoryRegion { Size = 0x1000, Permission = MemoryPermission.RW }  },
                    
                    // Code
                    { 0x80000000, new MemoryRegion { Data = flashBin, Size = 0x20000, Permission = MemoryPermission.RWX } },

                    // Stack
                    { 0x80100000, new MemoryRegion { Size = 0x10000, Permission = MemoryPermission.RW } },
                    
                    // Auth success / failed trigger
                    { 0xAA01000, new HwPeripheral((eng, address, size, value) => { eng.RequestStop(value == 1 ? Result.Completed : Result.Failed); }) },
                },
                BreakPoints = {
                    { binInfo.Symbols["flash_load_img"].Address, eng => {
                        var useAltData = ((MyConfig) eng.Config).UseAltData;

                        if (useAltData) {
                            eng.Write(0x32000000, Encoding.ASCII.GetBytes("!! Pwned boot !!"));
                        }
                        else {
                            eng.Write(0x32000000, Encoding.ASCII.GetBytes("Test Payload!!!!"));
                        }
                    } },
                    
                    // { binInfo.Symbols["memcmp"].Address, eng => {
                    //     var reg_r0 = eng.RegRead(Arm.UC_ARM_REG_R0);
                    //     var reg_r1 = eng.RegRead(Arm.UC_ARM_REG_R1);
                    //     var reg_r2 = eng.RegRead(Arm.UC_ARM_REG_R2);
                    //
                    //     var buf1 = eng.Read(reg_r0, new byte[reg_r2]);
                    //     var buf2 = eng.Read(reg_r1, new byte[reg_r2]);
                    //
                    //     Console.WriteLine(Utils.HexDump(buf1, reg_r0));
                    //     Console.WriteLine(Utils.HexDump(buf2, reg_r1));
                    // } },
                },
                Patches = {
                    { binInfo.Symbols["serial_putc"].Address, AArch32Info.A32_RET },
                },
                
                //OnCodeExecutionTraceEvent = eng => {
                //    Console.WriteLine($"I: {eng.CurrentInstruction.Address:x16} {eng.CurrentInstruction} {Utils.HexDump(eng.CurrentInstruction.Data)}");
                //    Console.WriteLine($"I: {eng.CurrentInstruction.Address:x16} {eng.CurrentInstruction}");
                //}
            };

            try {
                switch (runMode) {
                    case RunMode.NormalExecution:
                        otpPeripheral.PersistentChanges = true;

                        _doNormalExecutionSim(simConfig, binInfo);

                        break;
                    
                    case RunMode.VerifyExecution:
                        otpPeripheral.PersistentChanges = true;
                        
                        _doABVerificationTestSim(simConfig, binInfo);                            

                        break;
                    
                    case RunMode.FaultSimGUI:
                    case RunMode.FaultSimTUI:
                        var glitchRange = new List<TraceRange>();

                        // Only simulate faults in glitchRange
                        // glitchRange.Add(new SymbolTraceRange(binInfo.Symbols["some_func"]));

                        // Simulate faults using the following fault models:
                        var faultModels = new IFaultModel[] {
                            //new CachedNopFetchInstructionModel(), 
                            new TransientNopInstructionModel(),
                            //new CachedSingleBitFlipInstructionModel(),
                            new TransientSingleBitFlipInstructionModel(),
                            //new CachedBusFetchNopInstructionModel()
                        };
                        
                        if (runMode == RunMode.FaultSimGUI)
                            _doGUIFaultSim(simConfig, binInfo, faultModels, glitchRange);
                        else
                            _doTUIFaultSim(simConfig, binInfo, faultModels, glitchRange);

                        break;

                    default:
                        Console.WriteLine("Internal error: not supported run mode");
                        Environment.Exit(-1);
                        break;
                }
            }
            catch (SimulationException ex) when (!Debugger.IsAttached) {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Error: " + ex.Message);
                Console.Error.WriteLine();

                if (ex is PlatformEngineException exception) {
                    exception.Engine.DumpState(Console.Error, binInfo);
                }
            }
            catch (Exception e) when (!Debugger.IsAttached)  {
                Console.Out.WriteLine("Internal error: " + e.Message);
                Console.Out.WriteLine(e);

                Environment.Exit(-1);
            }
        }
        private static void _initProcess() {
            // Make this process low priority
            var thisProc = Process.GetCurrentProcess();
            thisProc.PriorityClass = ProcessPriorityClass.Idle;
        }

        private static void _doNormalExecutionSim(MyConfig simConfig, IBinInfo binInfo) {
            // enable serial
            simConfig.BreakPoints.Add(binInfo.Symbols["serial_putc"].Address, eng => {
                Console.Write((char) eng.RegRead(Arm.UC_ARM_REG_R0));
            });
            
            var sim = new Simulator(simConfig);
            
            var simResult = sim.RunSimulation();

            Console.WriteLine("Result: " + simResult);
        }
        
        private static void _doABVerificationTestSim(MyConfig simConfig, IBinInfo binInfo) {
            try {
                Console.Out.WriteLine("Verify functional behavior...");

                // *** GOOD SIGN *** //
                var sim = new Simulator(simConfig);
                
                sim.Config.BreakPoints.Add(binInfo.Symbols["serial_putc"].Address, eng => {
                    Console.Out.Write((char) eng.RegRead(Arm.UC_ARM_REG_R0));
                });

                var simResult = sim.RunSimulation();

                if (simResult != Result.Completed) {
                    Console.Out.WriteLine("Incorrect behavior ("+simResult+") with signed payload!");
                    Environment.Exit(-1);
                }
                
                // *** Incorrect SIGN *** //
                simConfig.UseAltData = true;

                ((OtpPeripheral) simConfig.AddressSpace[0x12000000]).PersistentChanges = false;

                sim = new Simulator(simConfig);
                
                sim.Config.BreakPoints.Add(binInfo.Symbols["serial_putc"].Address, eng => {
                    Console.Out.Write((char) eng.RegRead(Arm.UC_ARM_REG_R0));
                });

                simResult = sim.RunSimulation();

                if (simResult != Result.Failed && simResult != Result.Timeout) {
                    Console.Out.WriteLine($"Incorrect behavior ({simResult.ToString()}) with incorrectly signed payload!");
                    Environment.Exit(-1);
                }

                Console.Out.WriteLine("Verification finished. Bootloader is verified to work as intended.");
                Environment.Exit(0);
            }
            catch (SimulationException ex) {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine();
                    
                if (ex is PlatformEngineException exception) {
                    exception.Engine.DumpState(Console.Out, binInfo);
                }

                Environment.Exit(-1);
            }
        }
        
         private static void _doGUIFaultSim(MyConfig simConfig, IBinInfo binInfo, IFaultModel[] faultModels, IEnumerable<TraceRange> glitchRange) {
             Console.WriteLine("Starting simulation... This will take several minutes.");
            
             // Good / Bad simulation
             _doFaultSimTrace(simConfig, binInfo, glitchRange, out var correctSignTraceData, out var wrongSignTraceData);
            
             Console.Out.Write($"{correctSignTraceData.AmountInstuctionsExecuted}/{correctSignTraceData.AmountUniqueInstuctionsExecuted} " + 
                               $"{wrongSignTraceData.AmountInstuctionsExecuted}/{wrongSignTraceData.AmountUniqueInstuctionsExecuted}");

            var totalRuns = 0ul;
            foreach (var faultModel in faultModels) {
                totalRuns += faultModel.CountUniqueFaults(wrongSignTraceData);
            }
            
            Console.Out.WriteLine(" " + faultModels.Length + "/" +totalRuns);
            Console.Error.WriteLine(totalRuns);
            
            simConfig.UseAltData = true;
            
            var faultSim = new FaultSimulator(simConfig);

            faultSim.OnGlitchSimulationCompleted += (runs, eng, result) => {
                Console.Error.WriteLine($"{result.Fault.FaultModel.Name}::{result.Result}::{result.Fault.FaultAddress:x8}::"+
                                        $"{result.Fault.ToString()}::{(result.Result == Result.Exception?result.Exception.Message:"")}");

                return false;
            };

            faultSim.RunSimulation(faultModels, wrongSignTraceData);
            
            Environment.Exit(0);
        }
         
         private static void _doTUIFaultSim(MyConfig simConfig, IBinInfo binInfo, IFaultModel[] faultModels, IEnumerable<TraceRange> glitchRange) {
            Console.WriteLine("Starting simulation... This will take several minutes.");
            
            // Good / Bad simulation
            _doFaultSimTrace(simConfig, binInfo, glitchRange, out var correctSignTraceData, out var wrongSignTraceData);
            
            simConfig.UseAltData = true;
            
            var faultSim = new FaultSimulator(simConfig);

            faultSim.OnGlitchSimulationCompleted += (runs, eng, result) => {
                if (result.Result == Result.Completed) {
                    Console.WriteLine($"{result.Fault.FaultModel.Name} {result.Fault.ToString()} {binInfo.Symbolize(result.Fault.FaultAddress)}");
                }

                return false;    
            };

            faultSim.RunSimulation(faultModels, wrongSignTraceData);
            
            Environment.Exit(0);
        }

         private static void _doFaultSimTrace(MyConfig simConfig, IBinInfo binInfo, IEnumerable<TraceRange> glitchRange, 
                                              out Trace correctSignTraceDataOut, out Trace wrongSignTraceDataOut) {
             Trace correctSignTraceData = null;
             Trace wrongSignTraceData = null;
             
             try {
                var normalFipSim = new Simulator(simConfig);
                
                simConfig.UseAltData = true;
                var altFipSim = new Simulator(simConfig);

                var task1 = Task.Run(() => {
                    Console.WriteLine("Start correct sign trace");
                    
                    Result correctSignResult;
                    (correctSignResult, correctSignTraceData) = normalFipSim.TraceSimulation();

                    if (correctSignResult != Result.Completed) {
                        Console.Out.WriteLine("Simulation did not complete; result: " + correctSignResult);
                        Environment.Exit(-1);
                    }
                    
                    Console.WriteLine("Finished correct sign trace");
                });
                
                var task2 = Task.Run(() => {
                    Console.WriteLine("Start wrong sign trace");
                    
                    Result wrongSignResult;
                    (wrongSignResult, wrongSignTraceData) = altFipSim.TraceSimulation(glitchRange);

                    if (wrongSignResult != Result.Failed && wrongSignResult != Result.Timeout) {
                        Console.Out.WriteLine("Simulation did not fail; result: " + wrongSignResult);
                        Environment.Exit(-1);
                    }
                    
                    Console.WriteLine("Finished wrong sign trace");
                });

                task1.Wait();
                task2.Wait();
             }
            catch (SimulationException ex) {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine();

                if (ex is PlatformEngineException exception) {
                    exception.Engine.DumpState(Console.Out, binInfo);
                }

                Environment.Exit(-1);
            }

            correctSignTraceDataOut = correctSignTraceData;
            wrongSignTraceDataOut = wrongSignTraceData;
        }
    }
}