using System.Collections.Generic;

namespace PlatformSim {
    public class Trace {
        public ulong AmountInstuctionsExecuted => (ulong) InstructionTrace.Count;
        
        public ulong AmountUniqueInstuctionsExecuted => (ulong) InstructionHitCount.Count;

        public List<IInstruction> InstructionTrace { get; } = new List<IInstruction>();

        public Dictionary<ulong, List<ulong>> InstructionHitCount { get; } = new Dictionary<ulong, List<ulong>>();

        public IInstruction this[ulong address] {
            get {
                foreach (var instruction in InstructionTrace) {
                    if (instruction.Address == address) {
                        return instruction;
                    }
                }
                
                throw new KeyNotFoundException();
            }
        }
    }
}