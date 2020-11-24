namespace PlatformSim.Simulation {
    internal abstract class InstructionBase : IInstruction {
        protected InstructionBase(ulong address, byte[] data) {
            Address = address;
            Data = data;
        }
        
        public ulong Address { get; set; }

        public byte[] Data { get; set; }
        
        public abstract string Mnemonic { get; set; }
        public abstract string Operand { get; set; }

        public abstract IInstruction Clone();
    }
}