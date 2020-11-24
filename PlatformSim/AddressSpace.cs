using System.Collections.Generic;

namespace PlatformSim {
    public class AddressSpace : Dictionary<ulong, IMemoryRegion>, ICloneable<AddressSpace> {
        public AddressSpace() {
        }
        
        private AddressSpace(AddressSpace addressSpace) : base(addressSpace) {
        }

        public bool IsMapped(ulong address) {
            foreach (var kv in this) {
                var baseAddress = kv.Key;
                var memRegion = kv.Value;

                if (baseAddress <= address && address < (baseAddress + memRegion.Size))
                    return true;
            }

            return false;
        }
        
        public bool IsMapped(ulong address, ulong len) {
            foreach (var kv in this) {
                var baseAddress = kv.Key;
                var memRegion = kv.Value;

                if (baseAddress <= address && address + len <= (baseAddress + memRegion.Size))
                    return true;
            }

            return false;
        }

        public IMemoryRegion GetRegion(ulong address) {
            foreach (var kv in this) {
                var baseAddress = kv.Key;
                var memRegion = kv.Value;

                if (baseAddress <= address && address <= (baseAddress + memRegion.Size))
                    return memRegion;
            }

            throw new KeyNotFoundException($"Address {address:x16} not part of any region");
        }
        
        public ulong GetRegionBase(ulong address) {
            foreach (var kv in this) {
                var baseAddress = kv.Key;
                var memRegion = kv.Value;

                if (baseAddress <= address && address <= (baseAddress + memRegion.Size))
                    return baseAddress;
            }

            throw new KeyNotFoundException($"Address {address:x16} not part of any region");
        }

        public T GetRegion<T>() where T : IMemoryRegion {
            foreach (var kv in this) {
                if (kv.Value.GetType() == typeof(T)) {
                    return (T) kv.Value;
                }
            }

            throw new KeyNotFoundException("Memory region of type " + typeof(T).Name + " not found");
        }

        internal void Merge(AddressSpace value) {
            foreach (var kv in value) {
                if (ContainsKey(kv.Key)) {
                    Remove(kv.Key);
                }

                Add(kv.Key, kv.Value);
            }
        }

        public bool IsInRegion(IMemoryRegion region, ulong address, uint len) {
            foreach (var kv in this) {
                if (kv.Value == region) {
                    var baseAddress = kv.Key;
                    var memRegion = kv.Value;

                    if (baseAddress <= address && address + len <= (baseAddress + memRegion.Size)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public AddressSpace Clone() {
            return new AddressSpace(this);
        }
    }
}