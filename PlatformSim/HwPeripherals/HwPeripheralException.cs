using System;

namespace PlatformSim.HwPeripherals {
    public class HwPeripheralException : SimulationException {
        public HwPeripheralException(string message, Exception innerException = null) : base(message, innerException) { }
    }
}