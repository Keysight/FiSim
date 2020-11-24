using System.Collections.Generic;

using PlatformSim;

namespace FiSim {
    public interface IFaultModel {
        IEnumerable<IFaultDefinition> CreateFaultEnumerable(Trace traceData);

        ulong CountUniqueFaults(Trace traceData);
        
        string Name { get; }
    }
}