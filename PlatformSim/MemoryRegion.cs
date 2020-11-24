using System;

namespace PlatformSim {
    public class MemoryRegion : IMemoryRegionFilled {
        const ulong PageSize = 4096;

        ulong _size;

        public string Name { get; set; }

        public ulong Size {
            get {
                if (_size == 0) {
                    return ((ulong) Math.Round((Data.Length / (double) PageSize), MidpointRounding.AwayFromZero) + 1) * PageSize;
                }

                return _size;
            }
            set => _size = value;
        }

        public MemoryPermission Permission { get; set; }

        public byte[] Data { get; set; }
    }
}