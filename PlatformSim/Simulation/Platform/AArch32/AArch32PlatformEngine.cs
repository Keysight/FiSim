using System;
using System.IO;

using BinInfo;

using PlatformSim.Simulation.Engine;

namespace PlatformSim.Simulation.Platform.AArch32 {
    internal class AArch32PlatformEngine : PlatformEngineBase {
        protected override ulong ADDR_MAGIC_FINISHED { get; } = 0xFFFFFFF0;

        public AArch32PlatformEngine(IExecutionEngine executionEngine, Config config) : base(executionEngine, Simulation.Engine.ArchInfo.AArch32, config) {
        }

        public override Architecture Arch => Architecture.AArch32;

        public override IInstruction CurrentInstruction {
            get {
                var isThumb = TracePC % 2 == 1;

                var insnBytes = new byte[isThumb ? 2 : 4];
                Engine.MemRead(isThumb ? TracePC - 1 : TracePC, insnBytes);

                return new AArch32Instruction(isThumb ? TracePC - 1 : TracePC, insnBytes);
            }
        }

        protected override void _Init() {
            // Set LR
            Engine.RegWrite(Simulation.Engine.ArchInfo.AArch32.LR, ADDR_MAGIC_FINISHED);
        }

        protected override void DumpBackTrace(TextWriter outputWriter, IBinInfo binInfo) {
            var frameNr = 0;

            var stackRegion = Config.AddressSpace.GetRegion(Config.StackBase);

            var currentPc = Engine.RegRead(ArchInfo.PC);
            var currentFramePointerAddress = Config.AddressSpace.IsInRegion(stackRegion, Engine.RegRead(ArchInfo.FP), 4)
                ? Engine.RegRead(ArchInfo.FP)
                : Engine.RegRead(ArchInfo.SP);
            var currentLrAddress = Engine.RegRead(Simulation.Engine.ArchInfo.AArch32.LR);

            var lastFrame = false;
            var didLastFrame = false;

            while (Config.AddressSpace.IsMapped(currentPc) && !didLastFrame) {
                if (!lastFrame) {
                    if (binInfo != null) {
                        outputWriter.WriteLine(
                            $"#{frameNr++} {currentPc:X8} {binInfo.Symbolize(currentPc)} (LR: {currentLrAddress:X8} FP: {currentFramePointerAddress:X8})");
                    }
                    else {
                        outputWriter.WriteLine($"#{frameNr++} {currentPc:X8} (LR: {currentLrAddress:X8} FP: {currentFramePointerAddress:X8})");
                    }

                    if (Config.AddressSpace.IsInRegion(stackRegion, currentFramePointerAddress, 4)) {
                        var storedFpData = new byte[4];
                        var storedLrData = new byte[4];

                        Engine.MemRead(currentFramePointerAddress + 0, storedFpData);
                        Engine.MemRead(currentFramePointerAddress + 4, storedLrData);

                        var storedFp = BitConverter.ToUInt32(storedFpData, 0);
                        var storedLr = BitConverter.ToUInt32(storedLrData, 0);

                        currentPc = currentLrAddress - 4;
                        currentFramePointerAddress = storedFp;
                        currentLrAddress = storedLr;
                    }
                    else {
                        currentPc = currentLrAddress - 4;

                        lastFrame = true;
                    }
                }
                else {
                    if (binInfo != null) {
                        outputWriter.WriteLine($"#{frameNr++} {currentPc:X8} {binInfo.Symbolize(currentPc)}");
                    }
                    else {
                        outputWriter.WriteLine($"#{frameNr++} {currentPc:X8}");
                    }

                    didLastFrame = true;
                }
            }
        }
    }
}