using System;
using System.Collections.Generic;
using System.Linq;

using PlatformSim;

namespace FiSim.FaultDefinitions {
    public class CachedInstructionFaultDefinition : FaultDefinitionBase {
        public CachedInstructionFaultDefinition(IFaultModel faultModel,
                                                ulong faultAddress,
                                                byte[] originalData,
                                                byte[] faultedData,
                                                List<IInstruction> originalInstructions,
                                                List<IInstruction> faultedInstructions) : base(faultModel, faultAddress, originalData, faultedData,
            originalInstructions, faultedInstructions) {
        }

        public override void InitSimulator(IPlatformEngine sim) {
            if (FaultAddress % 2 == 1)
                throw new NotSupportedException();

            sim.Write(FaultAddress, FaultedData);
        }

        public override string ToString() {
            if (OriginalInstructions.Count == 1) {
                return $"{FaultAddress:X8}: {OriginalInstructions[0]} -> {FaultedInstructions[0]}";
            }
            else {
                var orgInsDesc = "";

                foreach (var instruction in OriginalInstructions.OrderBy(instruction => instruction.Address)) {
                    if (orgInsDesc.Length > 0) {
                        orgInsDesc += ", "+instruction.ToString();
                    }
                    else {
                        orgInsDesc = instruction.ToString();
                    }
                }
                
                var newInsDesc = "";

                foreach (var instruction in FaultedInstructions.OrderBy(instruction => instruction.Address)) {
                    if (newInsDesc.Length > 0) {
                        newInsDesc += ", "+instruction.ToString();
                    }
                    else {
                        newInsDesc = instruction.ToString();
                    }
                }

                orgInsDesc = "{ " + orgInsDesc + " }";
                newInsDesc = "{ " + newInsDesc + " }";
                
                return $"{FaultAddress:X8}: {orgInsDesc} -> {newInsDesc}";
            }
        }
    }
}