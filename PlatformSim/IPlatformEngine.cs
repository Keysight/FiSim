using System;
using System.IO;

using BinInfo;

namespace PlatformSim {
    public interface IPlatformEngine : IDisposable {
        Architecture Arch { get; }
        
        Config Config { get; }
        
        IInstruction CurrentInstruction { get; }
        
        ulong TracePC { get; set; }
        
        void DumpState(TextWriter outputWriter);
        void DumpState(TextWriter outputWriter, IBinInfo binInfo);

        void Init();
        
        Result Run();
        
        void RequestStop();
        void RequestStop(Result result);
        
        void RequestRestart();
        void RequestRestart(ulong address);

        byte[] Read(ulong address, byte[] data);
        void Write(ulong address, byte[] data);
        bool Compare(ulong address, byte[] expectedData);
        
        ulong RegRead(int regId);
        void RegWrite(int regId, ulong value);

        T GetState<T>(object obj, Action<T> initCb = null) where T : new();

        void SetBreakPoint(ulong address, Action<IPlatformEngine> callback);
    }
}