using System;
using System.Collections.Generic;

namespace PlatformSim {
    public class Config : ICloneable<Config> {
        public Config(Config baseConfig = null) {
            if (baseConfig != null) {
                Platform = baseConfig.Platform;
                
                AddressSpace = baseConfig.AddressSpace.Clone();
                
                BreakPoints = new Dictionary<ulong, Action<IPlatformEngine>>(baseConfig.BreakPoints);
                Patches = new Dictionary<ulong, byte[]>(baseConfig.Patches);
                
                EntryPoint = baseConfig.EntryPoint;
                StackBase = baseConfig.StackBase;

                MaxInstructions = baseConfig.MaxInstructions;
                Timeout = baseConfig.Timeout;

                OnUnmappedOrInvalidMemoryAccessEvent = baseConfig.OnUnmappedOrInvalidMemoryAccessEvent;
                OnInvalidInstructionEvent = baseConfig.OnInvalidInstructionEvent;
                OnCodeExecutionTraceEvent = baseConfig.OnCodeExecutionTraceEvent;
            }
        }
        
        public virtual Config Clone() {
            return new Config(this);
        }

        AddressSpace _addressSpace = new AddressSpace();

        public Architecture Platform { get; set; }

        public AddressSpace AddressSpace {
            get => _addressSpace;
            set {
                if (_addressSpace != null) {
                    _addressSpace.Merge(value);
                }
                else {
                    _addressSpace = value;
                }
            }
        }

        public ulong EntryPoint { get; set; }

        public ulong StackBase { get; set; }

        public ulong MaxInstructions { get; set; }

        public ulong Timeout { get; set; }

        public Result FinishedResult { get; set; } = Result.Completed;
        
        public Dictionary<ulong, Action<IPlatformEngine>> BreakPoints { get; } = new Dictionary<ulong, Action<IPlatformEngine>>();

        public Dictionary<ulong, byte[]> Patches { get; } = new Dictionary<ulong, byte[]>();
        
        public Func<IPlatformEngine, int, ulong, uint, ulong, bool> OnUnmappedOrInvalidMemoryAccessEvent { get; set; }
        public Action<IPlatformEngine> OnInvalidInstructionEvent { get; set; }
        public Action<IPlatformEngine> OnCodeExecutionTraceEvent { get; set; }
    }
}