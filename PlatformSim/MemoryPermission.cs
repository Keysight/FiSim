using System;

using UnicornManaged.Const;

// TODO: Unify this so Unicorn becomes one of many instead of leading
namespace PlatformSim {
    [Flags]
    public enum MemoryPermission {
        R = Common.UC_PROT_READ,
        W = Common.UC_PROT_WRITE,
        X = Common.UC_PROT_EXEC,
        
        RW = Common.UC_PROT_READ | Common.UC_PROT_WRITE,
        RX = Common.UC_PROT_READ | Common.UC_PROT_EXEC,
        RWX = Common.UC_PROT_ALL
    }
}