using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PlatformSim;

namespace FiSim.FaultModels {
    public abstract class ModelBase : IFaultModel {
        public abstract IEnumerable<IFaultDefinition> CreateFaultEnumerable(Trace traceData);

        readonly Dictionary<Trace, ulong> _countCache = new Dictionary<Trace, ulong>();
        public ulong CountUniqueFaults(Trace traceData) {
            lock (this) {
                if (_countCache.ContainsKey(traceData)) {
                    return _countCache[traceData];
                }

                long counter = 0;

                Parallel.ForEach(CreateFaultEnumerable(traceData), (faultDefinition, state) => { Interlocked.Increment(ref counter); });

                _countCache.Add(traceData, (ulong) counter);
                
                return (ulong) counter;
            }
        }

        public string Name => GetType().Name;
    }
}