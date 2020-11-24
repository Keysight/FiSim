namespace PlatformSim {
    public interface IMemoryRegionHook : IMemoryRegion {
        void OnRead(IPlatformEngine engine, ulong address, uint size);
        void OnWrite(IPlatformEngine engine, ulong address, uint size, ulong value);
    }
}