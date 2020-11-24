using System;
using System.IO;

using BinInfo;

using PlatformSim.Simulation.Engine;

namespace PlatformSim.Simulation.Platform.AArch64 {
    internal class AArch64PlatformEngine : PlatformEngineBase {
        protected override ulong ADDR_MAGIC_FINISHED { get; } = 0xFFFFFFFFFFFFFFFA;

        public AArch64PlatformEngine(IExecutionEngine executionEngine, Config config) : base(executionEngine, Simulation.Engine.ArchInfo.AArch64, config) {}

        public override Architecture Arch => Architecture.AArch64;
        
        public override IInstruction CurrentInstruction {
            get {
                var isThumb = TracePC % 2 == 1;

                var insnBytes = new byte[isThumb ? 2 : 4];
                Engine.MemRead(isThumb ? TracePC - 1 : TracePC, insnBytes);

                return new AArch64Instruction(isThumb ? TracePC - 1 : TracePC, insnBytes);
            }
        }
        
        protected override void _Init() {
            // Set LR
            Engine.RegWrite(Simulation.Engine.ArchInfo.AArch64.LR, ADDR_MAGIC_FINISHED); // R30
        }

        protected override void DumpBackTrace(TextWriter outputWriter, IBinInfo binInfo) {
            var frameNr = 0;

            var currentPc = Engine.RegRead(ArchInfo.PC);
            var currentFramePointerAddress = Engine.RegRead(ArchInfo.FP);
            var currentLrAddress = Engine.RegRead(Simulation.Engine.ArchInfo.AArch64.LR);

            var lastFrame = false;
            var didLastFrame = false;
            
            while (Config.AddressSpace.IsMapped(currentLrAddress - 4) && !didLastFrame) {
                if (!lastFrame) {
                    outputWriter.WriteLine($"#{frameNr++} {currentPc:X16} {binInfo.Symbolize(currentPc)} (LR: {currentLrAddress:X16} FP: {currentFramePointerAddress:X16})");
                    
                    if (Config.AddressSpace.IsInRegion(Config.AddressSpace.GetRegion(Config.StackBase), currentFramePointerAddress, 8)) {
                        var storedFpData = new byte[8];
                        var storedLrData = new byte[8];

                        Engine.MemRead(currentFramePointerAddress + 0, storedFpData);
                        Engine.MemRead(currentFramePointerAddress + 8, storedLrData);

                        var storedFp = BitConverter.ToUInt64(storedFpData, 0);
                        var storedLr = BitConverter.ToUInt64(storedLrData, 0);

                        currentPc                  = currentLrAddress - 4;
                        currentFramePointerAddress = storedFp;
                        currentLrAddress           = storedLr;
                    }
                    else {
                        currentPc = currentLrAddress - 4;
                        
                        lastFrame = true;
                    }
                }
                else {
                    outputWriter.WriteLine($"#{frameNr++} {currentPc:X16} {binInfo.Symbolize(currentPc)}");
                    
                    didLastFrame = true;
                }
            }
        }
    }
}