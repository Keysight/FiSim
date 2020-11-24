namespace PlatformSim {
    public interface IMemoryRegionFilled : IMemoryRegion {
        byte[] Data { get; }
    }
}