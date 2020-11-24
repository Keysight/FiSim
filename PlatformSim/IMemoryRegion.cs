namespace PlatformSim {
    public interface IMemoryRegion {
        ulong Size { get; }
        
        MemoryPermission Permission { get; }
    }
}