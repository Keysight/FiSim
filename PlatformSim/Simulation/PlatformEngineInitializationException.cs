using System;

namespace PlatformSim.Simulation {
    public class PlatformEngineInitializationException : SimulationException {
        public PlatformEngineInitializationException(IPlatformEngine engine, string message, Exception innerException = null) : base(message, innerException) {
            Engine = engine;
        }
        
        public IPlatformEngine Engine { get; }
    }
}