using System.IO;

namespace BinInfo.MachO {
    public class MachOBinInfo : IBinInfo {
        readonly string _filePath;

        public MachOBinInfo(string filePath) {
            _filePath = filePath;

            SourceLine = new MachOSourceLineResolver(this);
            Symbols = new MachOSymbolResolver(this);
        }

        public ISourceLineResolver SourceLine { get; }

        public ISymbolResolver Symbols { get; }
        
        public string Symbolize(ulong address) {
            var lineInfo = SourceLine[address];
            var symbol = Symbols[lineInfo.FunctionName];
            
            return $"{symbol.Name} + {address - symbol.Address}";
        }

        public string Path => new FileInfo(_filePath).FullName;
    }
}