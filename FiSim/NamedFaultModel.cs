using System;
using System.Collections.Generic;
using PlatformSim;

namespace FiSim  {
    public class NamedFaultModel : IFaultModel {
        public NamedFaultModel(string modelName) {
            Name = modelName;
        }

        public IEnumerable<IFaultDefinition> CreateFaultEnumerable(Trace traceData) => throw new NotImplementedException();

        public ulong CountUniqueFaults(Trace traceData) => throw new NotImplementedException();

        public string Name { get; }
    }
}