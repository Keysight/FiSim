using System;
using System.Collections.Generic;
using System.Linq;

using PlatformSim;
using PlatformSim.Simulation.Platform.AArch32;
using FiSim.FaultDefinitions;

namespace FiSim.FaultModels {
    public class TransientSingleBitFlipInstructionModel : ModelBase {
        public override IEnumerable<IFaultDefinition> CreateFaultEnumerable(Trace traceData) {
            var glitchedInstructions = new Dictionary<ulong, uint>();

            foreach (var orgInstruction in traceData.InstructionTrace) {
                if (!glitchedInstructions.ContainsKey(orgInstruction.Address)) {
                    glitchedInstructions.Add(orgInstruction.Address, 0);
                }

                if (glitchedInstructions[orgInstruction.Address] < 100 && // Skip if > 100 execs for performance
                    (!orgInstruction.Data.SequenceEqual(AArch32Info.A32_B_SELF) || glitchedInstructions[orgInstruction.Address] == 0)) {
                    glitchedInstructions[orgInstruction.Address]++;

                    for (var i = 0; i < orgInstruction.Data.Length * 8; i++) {
                        var newInstructionData = new byte[orgInstruction.Data.Length];

                        Array.Copy(orgInstruction.Data, 0, newInstructionData, 0, newInstructionData.Length);

                        newInstructionData[i / 8] ^= (byte) (1 << (i % 8));

                        var newInstruction = orgInstruction.Clone();
                        newInstruction.Data = newInstructionData;

                        yield return new TransientInstructionFaultDefinition(this, orgInstruction.Address, orgInstruction.Data, newInstruction.Data, new List<IInstruction> { orgInstruction },
                            new List<IInstruction> { newInstruction }, glitchedInstructions[orgInstruction.Address],
                            (uint) traceData.InstructionHitCount[orgInstruction.Address].Count);
                    }
                }
            }
        }
    }
}