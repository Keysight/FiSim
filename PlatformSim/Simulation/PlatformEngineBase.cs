using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;

using BinInfo;

using PlatformSim.Simulation.Engine;

using UnicornManaged;
using UnicornManaged.Const;

namespace PlatformSim.Simulation {
    internal abstract class PlatformEngineBase : IPlatformEngine {
        protected abstract ulong ADDR_MAGIC_FINISHED { get; }

        bool _isInitialized;

        ulong _pc;

        bool _requestRestart;

        bool _requestStop;

        readonly Dictionary<object, object> _state = new Dictionary<object, object>();

        protected PlatformEngineBase(IExecutionEngine engine, ArchInfo archInfo, Config config) {
            Engine = engine;

            ArchInfo = archInfo;

            Config = config;
        }

        public abstract Architecture Arch { get; }

        protected ArchInfo ArchInfo { get; }

        public Config Config { get; }

        public abstract IInstruction CurrentInstruction { get; }

        protected IExecutionEngine Engine { get; }

        protected ulong RegPC {
            get => Engine.RegRead(ArchInfo.PC);
            set {
                Engine.RegWrite(ArchInfo.PC, value);
            }
        }
        
        public ulong TracePC {
            get => IsTracingExecution ? _pc : RegPC;
            set {
                _pc = value;

                if (!IsTracingExecution) {
                    RegPC = value;
                }
            }
        }

        public Result Result { get; set; }

        protected bool IsTracingExecution => Config.OnCodeExecutionTraceEvent != null;

        public void RegisterMemoryHook(ulong baseAddress, IMemoryRegionHook device) {
            Engine.MemMap(baseAddress, device.Size, (int) device.Permission);

            Engine.AddMemReadHook((address, size) => device.OnRead(this, address, size), baseAddress, baseAddress + device.Size);
            Engine.AddMemWriteHook((address, size, value) => device.OnWrite(this, address, size, value), baseAddress, baseAddress + device.Size);
        }

        public void Init() {
            if (_isInitialized)
                throw new NotSupportedException();

            if (Config.Platform != Arch)
                throw new NotSupportedException();

            Result = Result.Undecided;

            // Reset Registers
            foreach (var reg in ArchInfo.GPR) {
                Engine.RegWrite(reg, 0);
            }

            TracePC = Config.EntryPoint;

            // Set SP
            try {
                Engine.RegWrite(ArchInfo.SP, Config.StackBase + Config.AddressSpace.GetRegion(Config.StackBase).Size);
            }
            catch (KeyNotFoundException) {
            } // Unmapped SP, remapped by code?

            _Init();

            _initAddressSpace();

            _applyBreakPoints();

            _applyPatches();

            _initUnmappedMemoryHandler();

            _isInitialized = true;
        }

        protected abstract void _Init();
        
        public void SetBreakPoint(ulong address, Action<IPlatformEngine> callback) {
            Engine.AddCodeHook((addr, size) => callback(this), address, address);
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public virtual Result Run() {
            if (!_isInitialized)
                Init();

            if (Result != Result.Undecided)
                throw new NotSupportedException();

            do {
                _requestRestart = false;
                _requestStop = false;

                try {
                    var startPc = TracePC;

                    // Handle exceptions, can throw RequestSimulationRestartException
                    Engine.EmuStart(startPc, Config.AddressSpace.GetRegion(startPc).Size - (startPc - Config.AddressSpace.GetRegionBase(startPc)),
                        Config.Timeout, Config.MaxInstructions);

                    if (Result == Result.Undecided) // Apparently we executed the max amount of instructions
                        Result = Result.Timeout;

                    break;
                }
                catch (EmulationRestartException e) {
                    Engine.EmuStop();
                    
                    _requestRestart = true;

                    if (e.Address != ADDR_MAGIC_FINISHED) {
                        RegPC = e.Address;

                        if (IsTracingExecution) {
                            TracePC = e.Address;
                        }
                    }
                }
                catch (UnicornEngineException e) {
                    if (e.ErrorNo == Common.UC_ERR_EXCEPTION) {
                        Config.OnInvalidInstructionEvent?.Invoke(this);

                        if (Result == Result.Undecided && !_requestRestart && !_requestStop) {
                            Result = Result.Exception;

                            throw new PlatformEngineException(this, $"Invalid instruction 0x{RegPC:X8} {CurrentInstruction}");
                        }
                    }
                    else {
                        // Use a fetch to the magic address to detect we have finished
                        if (e.ErrorNo != Common.UC_ERR_FETCH_UNMAPPED || RegPC != ADDR_MAGIC_FINISHED) {
                            if (Result == Result.Undecided && !_requestRestart && !_requestStop) {
                                Result = Result.Exception;

                                if (!Config.AddressSpace.IsMapped(RegPC)) {
                                    throw new PlatformEngineException(this, e.Message, e);
                                }

                                try {
                                    throw new PlatformEngineException(this, $"{e.Message} @ {RegPC:X8}: {CurrentInstruction}", e);
                                }
                                catch (UnicornEngineException) {
                                    throw new PlatformEngineException(this, $"{e.Message} @ {RegPC:X8}: <internal error>", e);
                                }
                            }
                        }

                        // We reached ADDR_MAGIC_FINISHED
                        Result = Result.Completed;
                    }
                }
                catch (AccessViolationException e) {
                    // Bug in qemu?
                    Result = Result.Exception;

                    Console.Error.WriteLine("Internal Error");

                    throw new PlatformEngineException(this, e.Message, e);
                }
                catch (Exception e) {
                    throw new PlatformEngineException(this, e.Message, e);
                }
            } while ((!_requestStop && Result == Result.Undecided) || _requestRestart);

            return Result;
        }

        private class EmulationRestartException : Exception {
            internal ulong Address { get; }

            internal EmulationRestartException(ulong address) {
                Address = address;
            }
        }

        public void RequestRestart() {
            if (!_isInitialized)
                throw new NotSupportedException();

            throw new EmulationRestartException(ADDR_MAGIC_FINISHED);
        }

        public void RequestRestart(ulong address) {
            throw new EmulationRestartException(address);
        }

        public void RequestStop() {
            if (!_isInitialized)
                throw new NotSupportedException();

            _requestStop = true;

            Engine.EmuStop();
        }

        public void RequestStop(Result result) {
            RequestStop();

            Result = result;
        }

        public T GetState<T>(object obj, Action<T> initCb = null) where T : new() {
            if (!_state.ContainsKey(obj)) {
                var newStateObj = new T();

                initCb?.Invoke(newStateObj);

                _state.Add(obj, newStateObj);
            }

            return (T) _state[obj];
        }

        public ulong RegRead(int regId) {
            if (!_isInitialized)
                throw new NotSupportedException();

            return Engine.RegRead(regId);
        }

        public void RegWrite(int regId, ulong value) {
            if (!_isInitialized)
                throw new NotSupportedException();

            Engine.RegWrite(regId, value);
        }

        public byte[] Read(ulong address, byte[] data) {
            if (!_isInitialized)
                throw new NotSupportedException();

            Engine.MemRead(address, data);

            return data;
        }

        public void Write(ulong address, byte[] data) {
            if (!_isInitialized)
                throw new NotSupportedException();

            Engine.MemWrite(address, data);
        }

        public bool Compare(ulong address, byte[] expectedData) {
            var currentData = new byte[expectedData.Length];

            Engine.MemRead(address, currentData);

            return !expectedData.Where((t, i) => currentData[i] != t).Any();
        }

        public virtual void DumpState(TextWriter outputWriter) {
            DumpState(outputWriter, null);
        }

        public void DumpState(TextWriter outputWriter, IBinInfo binInfo) {
            uint i = 0;
            foreach (var reg in ArchInfo.GPR) {
                if ((i % 4) > 0) {
                    outputWriter.Write(" ");
                }

                switch (ArchInfo.NativeWordSize) {
                    case 32:
                        outputWriter.Write($"R{i:d2}: {Engine.RegRead(reg):X8}");
                        break;
                    case 64:
                        outputWriter.Write($"R{i:d2}: {Engine.RegRead(reg):X16}");
                        break;
                }

                if ((i % 4) == 3) {
                    outputWriter.WriteLine();
                }

                i++;
            }

            if ((i % 4) > 0) {
                outputWriter.WriteLine();
            }

            if (binInfo != null) {
                var sourceInfo = "<unknown>";

                try {
                    sourceInfo = binInfo.SourceLine[Engine.RegRead(ArchInfo.PC)].ToString();
                }
                catch (KeyNotFoundException) {
                }

                switch (ArchInfo.NativeWordSize) {
                    case 32:
                        outputWriter.WriteLine($"SP:  {Engine.RegRead(ArchInfo.SP):X8} PC:  {Engine.RegRead(ArchInfo.PC):X8} {sourceInfo}");
                        break;
                    case 64:
                        outputWriter.WriteLine($"SP:  {Engine.RegRead(ArchInfo.SP):X16} PC:  {Engine.RegRead(ArchInfo.PC):X16} {sourceInfo}");
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            else {
                switch (ArchInfo.NativeWordSize) {
                    case 32:
                        outputWriter.WriteLine($"SP:  {Engine.RegRead(ArchInfo.SP):X8} PC:  {Engine.RegRead(ArchInfo.PC):X8}");
                        break;
                    case 64:
                        outputWriter.WriteLine($"SP:  {Engine.RegRead(ArchInfo.SP):X16} PC:  {Engine.RegRead(ArchInfo.PC):X16}");
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            // Stack dump
            var currentStackPointer = Engine.RegRead(ArchInfo.SP);
            var alignedStackPointer = currentStackPointer & 0xFFFFFFFFFFFFFFF0ul;

            if (Config.AddressSpace.IsMapped(currentStackPointer)) {
                outputWriter.WriteLine();
                outputWriter.WriteLine("Stack:");

                for (i = 0; i < 10; i++) {
                    var stackData = new byte[1 * 0x10];

                    if (Config.AddressSpace.IsMapped(alignedStackPointer - (4 * 0x10) + ((ulong) i * 0x10), 0x10)) {
                        try {
                            Engine.MemRead(alignedStackPointer - (4 * 0x10) + ((ulong) i * 0x10), stackData);

                            outputWriter.WriteLine(Utils.HexDump(stackData, alignedStackPointer - (4 * 0x10) + ((ulong) i * 0x10)).TrimEnd());
                        }
                        catch (Exception) {
                            outputWriter.WriteLine($"{alignedStackPointer - (4 * 0x10) + ((ulong) i * 0x10):x16} <error>");

                            break;
                        }
                    }
                }
            }

            // Backtrace
            if (Config.AddressSpace.IsMapped(currentStackPointer) && binInfo != null) {
                outputWriter.WriteLine();
                outputWriter.WriteLine("Backtrace:");

                DumpBackTrace(outputWriter, binInfo);
            }
        }

        protected abstract void DumpBackTrace(TextWriter outputWriter, IBinInfo binInfo);
        
        void _initAddressSpace() {
            foreach (var kv in Config.AddressSpace) {
                var baseAddress = kv.Key;
                var memMapping = kv.Value;

                switch (memMapping) {
                    case IMemoryRegionHook _:
                        RegisterMemoryHook(baseAddress, (IMemoryRegionHook) memMapping);
                        break;

                    case IMemoryRegionFilled _:
                        Engine.MemMap(baseAddress, memMapping.Size, (int) memMapping.Permission);

                        if (((IMemoryRegionFilled) memMapping).Data != null) {
                            if ((ulong) ((IMemoryRegionFilled) memMapping).Data.Length > memMapping.Size) {
                                throw new PlatformEngineInitializationException(this, $"Not enough space for data @ {baseAddress:X16}");
                            }

                            Engine.MemWrite(baseAddress, ((IMemoryRegionFilled) memMapping).Data);
                        }

                        if (IsTracingExecution) {
                            if ((memMapping.Permission & MemoryPermission.X) == MemoryPermission.X) {
                                Engine.AddCodeHook((address, size) => {
                                        TracePC = size == 2 ? address + 1 : address; // Thumb mode?

                                        try {
                                            Config.OnCodeExecutionTraceEvent.Invoke(this);
                                        }
                                        catch (Exception e) {
                                            throw new PlatformEngineException(this, "Internal error in OnCodeExecutionTraceEvent handler", e);
                                        }

                                        // We cannot use the normal hooks for this
                                        if (Config.BreakPoints.ContainsKey(address)) {
                                            Config.BreakPoints[address](this);
                                        }
                                    }, baseAddress, baseAddress + memMapping.Size);
                            }
                        }

                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        void _applyBreakPoints() {
            if (!IsTracingExecution) { // Tracing handler will take care of the breakpoints, to prevent too much hooks
                foreach (var kv in Config.BreakPoints) {
                    SetBreakPoint(kv.Key, kv.Value);
                }
            }
        }

        void _applyPatches() {
            foreach (var kv in Config.Patches) {
                Engine.MemWrite(kv.Key, kv.Value);
            }
        }

        void _initUnmappedMemoryHandler() {
            // Handler for accessing unmapped memory
            Engine.AddEventMemHook((eventType, address, size, value) => {
                    if (Config.OnUnmappedOrInvalidMemoryAccessEvent != null) {
                        if (Config.OnUnmappedOrInvalidMemoryAccessEvent.Invoke(this, eventType, address, size, value))
                            return true; // handled
                    }

                    // Unhandled, so throw an exception
                    switch (eventType) {
                        case UnicornEngine.MEM_WRITE_UNMAPPED:
                            throw new PlatformEngineException(this, $"Unmapped WRITE at 0x{address:X8} (data size = {size:X}, value = 0x{value:X})");
                        case UnicornEngine.MEM_READ_UNMAPPED:
                            throw new PlatformEngineException(this, $"Unmapped READ at 0x{address:X8} (data size = {size:X})");
                        case UnicornEngine.MEM_FETCH_UNMAPPED:
                            if (address == ADDR_MAGIC_FINISHED) {
                                RequestStop(Config.FinishedResult);
                                return true;
                            }

                            throw new PlatformEngineException(this, $"Unmapped FETCH at 0x{address:X8}");
                        default:
                            throw new PlatformEngineException(this, $"Unexpected memory event: {eventType} at 0x{address:X8} (data size = {size:X})");
                    }
                }, Common.UC_HOOK_MEM_READ_UNMAPPED | Common.UC_HOOK_MEM_WRITE_UNMAPPED | Common.UC_HOOK_MEM_FETCH_UNMAPPED | Common.UC_HOOK_MEM_READ_INVALID | Common.UC_HOOK_MEM_WRITE_INVALID | Common.UC_HOOK_MEM_FETCH_INVALID);
        }

        protected void ResetAddressSpace() {
            if (!_isInitialized)
                throw new NotSupportedException();

            foreach (var kv in Config.AddressSpace) {
                var baseAddress = kv.Key;
                var memMapping = kv.Value;

                if (memMapping is IMemoryRegionFilled) {
                    if ((memMapping.Permission & MemoryPermission.W) == MemoryPermission.W) {
                        if (((IMemoryRegionFilled) memMapping).Data == null || (ulong) ((IMemoryRegionFilled) memMapping).Data.Length < memMapping.Size) {
                            // Zero out
                            Engine.MemWrite(baseAddress, new byte[memMapping.Size]);
                        }

                        if (((IMemoryRegionFilled) memMapping).Data != null) {
                            Engine.MemWrite(baseAddress, ((IMemoryRegionFilled) memMapping).Data);
                        }
                    }
                }
            }
        }

        public void Dispose() {
            Engine?.Dispose();
        }
    }
}