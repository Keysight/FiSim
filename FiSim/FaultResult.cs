using PlatformSim;

namespace FiSim {
    public class FaultResult {
        public IFaultDefinition Fault { get; set; }

        public Result Result { get; set; }

        public SimulationException Exception { get; set; }
    }
}