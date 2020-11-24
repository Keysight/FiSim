using System;
using System.Collections.Generic;
using System.Linq;

using PlatformSim.Simulation;
using PlatformSim.Simulation.Engine;
using PlatformSim.Simulation.Platform.AArch32;
using PlatformSim.Simulation.Platform.AArch64;

namespace PlatformSim {
    public class Simulator {
        public Simulator(Config config) {
            Config = config.Clone();
        }

        public Config Config { get; }

        public Result RunSimulation() {
            return CreateEngine().Run();
        }

        public (Result, Trace) TraceSimulation(IEnumerable<TraceRange> traceRanges = null) {
            var traceData = new Trace();
            
            var engine = CreateEngine();

            var orgOnCodeExecutionTraceEvent = engine.Config.OnCodeExecutionTraceEvent;
            
            var hasTraceFilter = traceRanges != null && traceRanges.Any();
            var tracedAddresses = new HashSet<ulong>();

            if (hasTraceFilter) {
                foreach (var traceRange in traceRanges) {
                    for (var addr = traceRange.Start; addr <= traceRange.End; addr += 2) {
                        tracedAddresses.Add(addr);
                    }
                }
            }

            engine.Config.OnCodeExecutionTraceEvent = eng => {
                var instruction = eng.CurrentInstruction;
                
                if (hasTraceFilter && !tracedAddresses.Contains(instruction.Address)) {
                    // Not on trace list
                    return;
                }

                traceData.InstructionTrace.Add(instruction);

                if (!traceData.InstructionHitCount.ContainsKey(instruction.Address))
                    traceData.InstructionHitCount.Add(instruction.Address, new List<ulong>());

                traceData.InstructionHitCount[instruction.Address].Add(traceData.AmountInstuctionsExecuted);
                
                orgOnCodeExecutionTraceEvent?.Invoke(eng);
            };

            var result = engine.Run();

            return (result, traceData);
        }

        protected IPlatformEngine CreateEngine() {
            PlatformEngineBase sim;
            
            switch (Config.Platform) {
                case Architecture.AArch32:
                    sim = new AArch32PlatformEngine(new UnicornEngine(Architecture.AArch32), Config.Clone());
                    break;
                case Architecture.AArch64:
                    sim = new AArch64PlatformEngine(new UnicornEngine(Architecture.AArch64), Config.Clone());
                    break;
                default:
                    throw new NotSupportedException();
            }

            return sim;
        }
    }
}