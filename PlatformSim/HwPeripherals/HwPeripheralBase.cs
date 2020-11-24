namespace PlatformSim.HwPeripherals {
    public abstract class HwPeripheralBase : IPeripheral {
        public virtual ulong Size { get; } = 0x1000;

        public virtual MemoryPermission Permission { get; } = MemoryPermission.RW;
        
        public virtual void OnRead(IPlatformEngine engine, ulong address, uint size) => throw new NotSupportedHwOperationException(engine, $"OnRead({address:X16}, {size}) not implemented");
        public virtual void OnWrite(IPlatformEngine engine, ulong address, uint size, ulong value) => throw new NotSupportedHwOperationException(engine, $"OnWrite({address:X16}, {size}, {value}) not implemented");
    }
}