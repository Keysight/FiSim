using System;

namespace PlatformSim {
    public interface IExecutionEngine : IDisposable {
        void EmuStart(ulong address, ulong size, ulong timeout, ulong maxInstructions);
        void EmuStop();
        
        void MemMap(ulong address, ulong size, int perm);
        
        void MemRead(ulong address, byte[] data);
        void MemWrite(ulong address, byte[] data);
        
        ulong RegRead(int reg);
        void RegWrite(int reg, ulong value);
        
        void AddCodeHook(Action<ulong, uint> hook, ulong startAddress, ulong endAddress);
        void AddMemReadHook(Action<ulong, uint> hook, ulong startAddress, ulong endAddress);
        void AddMemWriteHook(Action<ulong, uint, ulong> hook, ulong startAddress, ulong endAddress);
        void AddEventMemHook(Func<int, ulong, uint, ulong, bool> hook, int eventTypes);
    }
}