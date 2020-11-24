using System;

namespace PlatformSim.HwPeripherals {
    public class InvalidHwOperationException : HwPeripheralException {
        public InvalidHwOperationException(IPlatformEngine engine, string message, Exception innerException = null) : 
            base($"{message} @ {engine.TracePC:X8}: {engine.CurrentInstruction}", innerException) {}
    }
}