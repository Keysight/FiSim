using System.Collections.Concurrent;

using PlatformSim;

namespace FiSim {
    public class FaultModelResults {
        public IFaultModel Model { get; set; }
        
        public ConcurrentBag<FaultResult> Attempts { get; } = new ConcurrentBag<FaultResult>();
        
        public ConcurrentDictionary<Result, ConcurrentBag<FaultResult>> Glitches { get; } = new ConcurrentDictionary<Result, ConcurrentBag<FaultResult>>();
    }
}