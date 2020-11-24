using System.Collections.Generic;

using PlatformSim;
using FiSim.FaultDefinitions;

namespace FiSim.FaultModels {
    public class CachedBusFetchNopInstructionModel : ModelBase {
        private const uint BUS_FETCH_SIZE = 128 / 8;
        
        public override IEnumerable<IFaultDefinition> CreateFaultEnumerable(Trace traceData) {
            var glitchedInstructions = new List<ulong>();

            foreach (var orgInstruction in traceData.InstructionTrace) {
                var address = orgInstruction.Address & ~(BUS_FETCH_SIZE - 1);
                
                if (!glitchedInstructions.Contains(address)) {
                    var orgInstructionList = new List<IInstruction>();
                    var newInstructionList = new List<IInstruction>();

                    for (ulong offset = 0; offset < BUS_FETCH_SIZE;) {
                        IInstruction instruction;
                        
                        try {
                            instruction = traceData[address + offset].Clone();
                            orgInstructionList.Add(instruction);
                        }
                        catch (KeyNotFoundException) {
                            instruction = orgInstruction.Clone();
                            instruction.Address = address + offset;
                            instruction.Data = new byte[orgInstruction.Data.Length];
                            instruction.Mnemonic = "???";
                            instruction.Operand = "";
                            orgInstructionList.Add(instruction);
                        }
                        
                        var newInstruction = orgInstruction.Clone();
                        newInstruction.Address = address + offset;
                        newInstruction.Data = new byte[orgInstruction.Data.Length];
                        newInstructionList.Add(newInstruction);
                        
                        offset += (ulong) instruction.Data.Length;
                    }

                    for (ulong offset = 0; offset < BUS_FETCH_SIZE;) {
                        offset += (ulong) orgInstruction.Data.Length;
                    }
                    
                    yield return new CachedInstructionFaultDefinition(this, address, null, new byte[BUS_FETCH_SIZE], 
                        orgInstructionList, newInstructionList);

                    glitchedInstructions.Add(address);
                }
            }
        }
    }
}