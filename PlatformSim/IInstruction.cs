namespace PlatformSim {
    public interface IInstruction : ICloneable<IInstruction> {
        ulong Address { get; set; }
        
        byte[] Data { get; set; }
        
        string Mnemonic { get; set; }

        string Operand { get; set; }

        string ToString();
    }
}