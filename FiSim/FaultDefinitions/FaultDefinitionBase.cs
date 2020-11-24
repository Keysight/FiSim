using System;
using System.Collections.Generic;

using PlatformSim;

namespace FiSim.FaultDefinitions {
    public abstract class FaultDefinitionBase : IFaultDefinition {
        public IFaultModel FaultModel { get; }

        public ulong FaultAddress { get; }

        public byte[] OriginalData { get; }
        
        public byte[] FaultedData { get; }

        public List<IInstruction> OriginalInstructions { get; }

        public List<IInstruction> FaultedInstructions { get; }

        protected FaultDefinitionBase(IFaultModel faultModel,
                                      ulong faultAddress,
                                      byte[] originalData,
                                      byte[] faultedData,
                                      List<IInstruction> originalInstructions,
                                      List<IInstruction> faultedInstructions) {
            if (originalInstructions.Count != faultedInstructions.Count)
                throw new NotSupportedException();

            for (var i = 0; i < originalInstructions.Count; i++) {
                if (originalInstructions[i].Address != faultedInstructions[i].Address)
                    throw new NotSupportedException();
            }

            FaultModel = faultModel;
            FaultAddress = faultAddress;
            OriginalInstructions = originalInstructions;
            FaultedInstructions = faultedInstructions;
            OriginalData = originalData;
            FaultedData = faultedData;
        }

        public abstract void InitSimulator(IPlatformEngine sim);

        public virtual int Compare(IFaultDefinition faultDefinition) {
            if (FaultAddress == faultDefinition.FaultAddress) {
                return 0;
            }
            else if (FaultAddress < faultDefinition.FaultAddress) {
                return -1;
            }
            else if (FaultAddress > faultDefinition.FaultAddress) {
                return 1;
            }
            else {
                throw new InvalidOperationException();
            }
        }

        public override string ToString() => throw new NotSupportedException();
    }
}