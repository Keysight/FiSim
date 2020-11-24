using PlatformSim;

namespace FiSim {
    public class InstructionFaultDefinition : IFaultDefinition {
        public InstructionFaultDefinition(IFaultModel faultModel, IInstruction originalInstruction, IInstruction faultedInstruction, string description, ulong faultAddress) {
            //if (originalInstruction.Address != faultedInstruction.Address)
            //    throw new NotSupportedException();

            FaultModel = faultModel;
            Description = description;
            FaultAddress = faultAddress;
//            FaultAddress = originalInstruction.Address;
//            OriginalInstruction = originalInstruction;
//            FaultedInstruction = faultedInstruction;
        }
        
        public IFaultModel FaultModel { get; }
        
        public ulong FaultAddress { get; }
        
        public byte[] OriginalData => throw new System.NotImplementedException();
        public byte[] FaultedData => throw new System.NotImplementedException();
        public void InitSimulator(IPlatformEngine sim) => throw new System.NotImplementedException();

        public int Compare(IFaultDefinition faultDefinition) => throw new System.NotImplementedException();

        public string Description { get; }
//        public IInstruction OriginalInstruction { get; }
//        public IInstruction FaultedInstruction { get; }
    }
}