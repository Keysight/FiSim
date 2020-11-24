using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PlatformSim;

namespace FiSim {
    public class FaultModelResultsList : List<FaultModelResults> {
        public void DumpResults(TextWriter outputWriter) {
            foreach (var faultModelResult in this) {
                // Init missing values
                foreach (var simResult in Enum.GetValues(typeof(Result))) {
                    faultModelResult.Glitches.GetOrAdd((Result) simResult, new ConcurrentBag<FaultResult>());
                }
                
                outputWriter.WriteLine("{0}: [{1}/{2}] ({3} internal errors)", 
                                       faultModelResult.Model.Name, 
                                       faultModelResult.Glitches[Result.Completed].Count, 
                                       faultModelResult.Attempts.Count, 
                                       faultModelResult.Glitches[Result.Exception].Count);
                
                ulong lastAddress = 0;
                var sameAddress = 0;
                
                foreach (var glitchInfo in faultModelResult.Glitches[Result.Completed].OrderBy(result => result.Fault, new FaultModelResultComparer())) {
                    if (lastAddress != glitchInfo.Fault.FaultAddress) {
                        if (sameAddress > 0) {
                            outputWriter.WriteLine("[+ "+sameAddress+"]");
                                
                            sameAddress = 0;
                        }
                            
                        outputWriter.WriteLine($"{glitchInfo.Fault.ToString()}");

                        lastAddress = glitchInfo.Fault.FaultAddress;
                    }
                    else {
                        sameAddress++;
                    }
                }
                
                if (sameAddress > 0) {
                    outputWriter.WriteLine("[+ "+sameAddress+"]");
                }
            }
        }

        class FaultModelResultComparer : IComparer<IFaultDefinition> {
            public int Compare(IFaultDefinition x, IFaultDefinition y) => x.Compare(y);
        }
    }
}