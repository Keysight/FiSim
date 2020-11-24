using PlatformSim;

namespace FiSim {
    public interface IFaultDefinition {
        IFaultModel FaultModel { get; }

        ulong FaultAddress { get; }
        
        byte[] OriginalData { get; }
        
        byte[] FaultedData { get; }
            
        //List<IInstruction> OriginalInstructions { get; }

        //List<IInstruction> FaultedInstructions { get; }

        void InitSimulator(IPlatformEngine sim);
        
        int Compare(IFaultDefinition faultDefinition);

        string ToString();
    }
}