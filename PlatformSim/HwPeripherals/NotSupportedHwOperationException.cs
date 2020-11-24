using System.Diagnostics;

namespace PlatformSim.HwPeripherals {
    public class NotSupportedHwOperationException : HwPeripheralException {
        public NotSupportedHwOperationException(IPlatformEngine engine) : base($"Unknown exception in {new StackTrace()}.{new StackTrace().GetFrames()[1].GetMethod().Name} @ {engine.TracePC:X8}: {engine.CurrentInstruction}") {}
        public NotSupportedHwOperationException(IPlatformEngine engine, string message) : base($"{message} @ {engine.TracePC:X8}: {engine.CurrentInstruction}") {}
    }
}