using System;
using System.Collections.Generic;
using System.Linq;

using PlatformSim;

namespace FiSim.FaultDefinitions {
    public class TransientInstructionFaultDefinition : FaultDefinitionBase {
        public uint BreakpointHitCount { get; }

        public uint NormalExecutionCount { get; }

        public TransientInstructionFaultDefinition(IFaultModel faultModel,
                                                   ulong faultAddress,
                                                   byte[] originalData,
                                                   byte[] faultedData,
                                                   List<IInstruction> originalInstructions,
                                                   List<IInstruction> faultedInstructions,
                                                   uint breakpointHitCount,
                                                   uint normalExecutionCount) : base(faultModel, faultAddress, originalData, faultedData, originalInstructions,
            faultedInstructions) {
            BreakpointHitCount = breakpointHitCount;

            NormalExecutionCount = normalExecutionCount;
        }

        public override void InitSimulator(IPlatformEngine sim) {
            if (FaultAddress % 2 == 1)
                throw new NotSupportedException();

            uint hitCount = 0;

            sim.SetBreakPoint(FaultAddress, engine => {
                hitCount++;
                
                if (hitCount == BreakpointHitCount) { // We hit the glitched instruction
                    engine.Write(FaultAddress, FaultedData);

                    engine.RequestRestart(FaultAddress);
                }
                else if (hitCount == BreakpointHitCount + 1) { // Execute glitched instruction
                }
                else if (hitCount == BreakpointHitCount + 2) { // Write back original instruction
                    engine.Write(FaultAddress, OriginalData);

                    engine.RequestRestart(FaultAddress);
                }
            });
        }

        public override int Compare(IFaultDefinition faultDefinition) {
            if (FaultAddress == faultDefinition.FaultAddress && faultDefinition is TransientInstructionFaultDefinition executedInstructionFaultDefinition) {
                if (BreakpointHitCount == executedInstructionFaultDefinition.BreakpointHitCount) {
                    return 0;
                }
                else if (BreakpointHitCount < executedInstructionFaultDefinition.BreakpointHitCount) {
                    return -1;
                }
                else if (BreakpointHitCount > executedInstructionFaultDefinition.BreakpointHitCount) {
                    return 1;
                }
                else {
                    throw new InvalidOperationException();
                }
            }

            return base.Compare(faultDefinition);
        }

        public override string ToString() {
            if (OriginalInstructions.Count == 1) {
                if (BreakpointHitCount == NormalExecutionCount)
                    return $"{FaultAddress:X8}: {OriginalInstructions[0]} -> {FaultedInstructions[0]}";
                else
                    return $"{FaultAddress:X8}({BreakpointHitCount}/{NormalExecutionCount}): {OriginalInstructions[0]} -> {FaultedInstructions[0]}";
            }
            else {
                var orgInsDesc = "";

                foreach (var instruction in OriginalInstructions.OrderBy(instruction => instruction.Address)) {
                    if (orgInsDesc.Length > 0) {
                        orgInsDesc += ", " + instruction.ToString();
                    }
                    else {
                        orgInsDesc = instruction.ToString();
                    }
                }

                var newInsDesc = "";

                foreach (var instruction in FaultedInstructions.OrderBy(instruction => instruction.Address)) {
                    if (newInsDesc.Length > 0) {
                        newInsDesc += ", " + instruction.ToString();
                    }
                    else {
                        newInsDesc = instruction.ToString();
                    }
                }

                orgInsDesc = "{ " + orgInsDesc + " }";
                newInsDesc = "{ " + newInsDesc + " }";
                
                if (BreakpointHitCount == NormalExecutionCount)
                    return $"{FaultAddress:X8}: {orgInsDesc} -> {newInsDesc}";
                else
                    return $"{FaultAddress:X8}({BreakpointHitCount}/{NormalExecutionCount}): {orgInsDesc} -> {newInsDesc}";
            }
        }
    }
}