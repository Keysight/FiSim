using System;

namespace PlatformSim {
    public class SimulationException : Exception {
        public SimulationException(string message, Exception innerException = null) : base(message, innerException) {
        }
    }
}