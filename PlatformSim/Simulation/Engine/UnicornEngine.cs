using System;

using UnicornManaged;
using UnicornManaged.Const;

// ReSharper disable all

namespace PlatformSim.Simulation.Engine {
    public class UnicornEngine : Unicorn, IExecutionEngine {
        public static int HOOK_MEM_READ_UNMAPPED= Common.UC_HOOK_MEM_READ_UNMAPPED;
        public static int HOOK_MEM_WRITE_UNMAPPED= Common.UC_HOOK_MEM_WRITE_UNMAPPED;

        public const int MEM_READ_UNMAPPED = Common.UC_MEM_READ_UNMAPPED;
        public const int MEM_WRITE_UNMAPPED = Common.UC_MEM_WRITE_UNMAPPED;
        public const int MEM_FETCH_UNMAPPED = Common.UC_MEM_FETCH_UNMAPPED;
        
        public const int MEM_READ_INVALID = Common.UC_HOOK_MEM_READ_INVALID;
        public const int MEM_WRITE_INVALID = Common.UC_HOOK_MEM_WRITE_INVALID;
        public const int MEM_FETCH_INVALID = Common.UC_HOOK_MEM_FETCH_INVALID;

        public UnicornEngine(Architecture arch) : base(
                                                       ((arch == Architecture.AArch32) ? Common.UC_ARCH_ARM : ((arch == Architecture.AArch64) ? Common.UC_ARCH_ARM64 : throw new NotSupportedException())),
                                                       ((arch == Architecture.AArch32) ? Common.UC_MODE_ARM : ((arch == Architecture.AArch64) ? Common.UC_MODE_ARM : throw new NotSupportedException()))
                                                       ) {}
        
        public void AddCodeHook(Action<ulong, uint> hook, ulong startAddress, ulong endAddress) {
            base.AddCodeHook((unicorn, addr, size, userData) => hook.Invoke(addr, size), null, startAddress, endAddress);
        }

        public void AddMemReadHook(Action<ulong, uint> hook, ulong startAddress, ulong endAddress) {
            base.AddMemReadHook((Unicorn engine, ulong address, uint size, object userData) => hook.Invoke(address, size),
                                null,
                                startAddress,
                                endAddress);
        }

        public void AddMemWriteHook(Action<ulong, uint, ulong> hook, ulong startAddress, ulong endAddress) {
            base.AddMemWriteHook((Unicorn engine, ulong address, uint size, ulong value, object userData) => hook.Invoke(address, size, value),
                                 null,
                                 startAddress,
                                 endAddress);
        }

        public void AddEventMemHook(Func<int, ulong, uint, ulong, bool> hook, int eventTypes) {
            base.AddEventMemHook((unicorn, eventType, address, size, value, ud) => {
                                     return hook.Invoke(eventType, address, size, value);
                                 }, eventTypes);
        }
    }
}