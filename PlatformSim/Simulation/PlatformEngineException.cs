using System;

namespace PlatformSim.Simulation {
    public class PlatformEngineException : SimulationException {
        public PlatformEngineException(IPlatformEngine engine, string message, Exception innerException = null) : base(message, innerException) {
            Engine = engine;
        }
        
        public IPlatformEngine Engine { get; }
    }
}