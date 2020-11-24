using System.Collections.Generic;

using PlatformSim;
using FiSim.FaultDefinitions;

namespace FiSim.FaultModels {
    public class CachedNopFetchInstructionModel : ModelBase {
        public override IEnumerable<IFaultDefinition> CreateFaultEnumerable(Trace traceData) {
            var glitchedInstructions = new List<ulong>();

            foreach (var orgInstruction in traceData.InstructionTrace) {
                if (!glitchedInstructions.Contains(orgInstruction.Address)) {
                    var newInstruction = orgInstruction.Clone();
                    newInstruction.Data = new byte[orgInstruction.Data.Length];

                    yield return new CachedInstructionFaultDefinition(this, orgInstruction.Address, orgInstruction.Data, newInstruction.Data,
                        new List<IInstruction> { orgInstruction }, new List<IInstruction> { newInstruction });

                    glitchedInstructions.Add(orgInstruction.Address);
                }
            }
        }
    }
}