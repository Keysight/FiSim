using System;
using System.Collections.Generic;

using PlatformSim;
using FiSim.FaultDefinitions;

namespace FiSim.FaultModels {
    public class CachedSingleBitFlipInstructionModel : ModelBase {
        public override IEnumerable<IFaultDefinition> CreateFaultEnumerable(Trace traceData) {
            var glitchedInstructions = new List<ulong>();

            foreach (var orgInstruction in traceData.InstructionTrace) {
                if (!glitchedInstructions.Contains(orgInstruction.Address)) {
                    for (var i = 0; i < orgInstruction.Data.Length * 8; i++) {
                        var newInstructionData = new byte[orgInstruction.Data.Length];

                        Array.Copy(orgInstruction.Data, 0, newInstructionData, 0, newInstructionData.Length);

                        newInstructionData[i / 8] ^= (byte) (1 << (i % 8));

                        var newInstruction = orgInstruction.Clone();
                        newInstruction.Data = newInstructionData;

                        yield return new CachedInstructionFaultDefinition(this, orgInstruction.Address, orgInstruction.Data, newInstruction.Data, new List<IInstruction> { orgInstruction },
                            new List<IInstruction> { newInstruction });
                    }

                    glitchedInstructions.Add(orgInstruction.Address);
                }
            }
        }
    }
}