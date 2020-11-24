using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading.Tasks;

using PlatformSim;

using Trace = PlatformSim.Trace;

namespace FiSim {
    public delegate bool OnGlitchSimulationCompletedCallback(ulong totalRuns, IPlatformEngine eng, FaultResult faultResult);

    public class FaultSimulator : Simulator {
        public FaultSimulator(Config config) : base(config) {
        }

        public event OnGlitchSimulationCompletedCallback OnGlitchSimulationCompleted;

        public FaultModelResultsList RunSimulation(IFaultModel[] faultModels, Trace traceData) {
            var results = new FaultModelResultsList();

            var expectedRunsAllModels =
                faultModels.Aggregate<IFaultModel, ulong>(0, (current, faultModel) => current + faultModel.CountUniqueFaults(traceData));

            foreach (var faultModel in faultModels) {
                var result = new FaultModelResults { Model = faultModel };

                if (Debugger.IsAttached) {
                    foreach (var faultDefinition in faultModel.CreateFaultEnumerable(traceData)) {
                        using (var simEngine = CreateEngine()) {
                            var faultResult = _runFaultSimulation(faultDefinition, simEngine);

                            result.Glitches.GetOrAdd(faultResult.Result, new ConcurrentBag<FaultResult>()).Add(faultResult);
                            result.Attempts.Add(faultResult);

                            if (OnGlitchSimulationCompleted != null) {
                                var requestToFinish = OnGlitchSimulationCompleted(expectedRunsAllModels, simEngine, faultResult);

                                if (requestToFinish) {
                                    break;
                                }
                            }
                        }
                    }
                }
                else {
                    Parallel.ForEach(faultModel.CreateFaultEnumerable(traceData), (faultDefinition, state) => {
                        if (!state.IsStopped) {
                            using (var simEngine = CreateEngine()) {
                                var faultResult = _runFaultSimulation(faultDefinition, simEngine);

                                result.Glitches.GetOrAdd(faultResult.Result, new ConcurrentBag<FaultResult>()).Add(faultResult);
                                result.Attempts.Add(faultResult);

                                if (OnGlitchSimulationCompleted != null) {
                                    var requestToFinish = OnGlitchSimulationCompleted(expectedRunsAllModels, simEngine, faultResult);

                                    if (requestToFinish) {
                                        state.Stop();
                                    }
                                }
                            }
                        }
                    });
                }

                results.Add(result);
            }

            return results;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        FaultResult _runFaultSimulation(IFaultDefinition faultDefinition, IPlatformEngine sim) {
            sim.Init();

            faultDefinition.InitSimulator(sim);

            var simResult = Result.Exception;
            SimulationException simException = null;

            try {
                simResult = sim.Run();
            }
            catch (SimulationException e) {
                simException = e;
            }

            return new FaultResult { Fault = faultDefinition, Result = simResult, Exception = simException };
        }
    }
}