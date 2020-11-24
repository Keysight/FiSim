using System;
using System.Collections.Generic;
using System.IO;

namespace BinInfo.ELF {
    public class ELFBinInfo : IBinInfo {
        readonly string _filePath;

        public ELFBinInfo(ELFType elfType, string filePath, string toolchainPath, string toolchainPrefix) {
            _filePath = filePath;
            
            ElfType = elfType;

            SourceLine = new ELFSourceLineResolver(this, toolchainPath, toolchainPrefix);
            Symbols    = new ELFSymbolResolver(this, toolchainPath, toolchainPrefix);
        }
        
        public ELFType ElfType { get; }

        public ISourceLineResolver SourceLine { get; }

        public ISymbolResolver Symbols { get; }

        public string Symbolize(ulong address) {
            try {
                var lineInfo = SourceLine[address];

                try {
                    var symbol = Symbols[lineInfo.FunctionName];

                    return $"{symbol.Name} + {address - symbol.Address} {lineInfo.FilePath}:{lineInfo.LineNumber}";
                }
                catch (NotSupportedException) {
                    return $"{lineInfo.FunctionName} ({address:x8})";
                }
            }
            catch (KeyNotFoundException) {
                return $"??? ({address:x8})";
            }
        }

        public string Path => new FileInfo(_filePath).FullName;
    }
}
