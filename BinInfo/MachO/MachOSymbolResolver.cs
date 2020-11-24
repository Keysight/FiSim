using System;
using System.Collections.Generic;
using System.Linq;

namespace BinInfo.MachO {
    public class MachOSymbolResolver : ISymbolResolver {
        readonly MachOBinInfo _bin;

        public MachOSymbolResolver(MachOBinInfo bin) {
            _bin = bin;
        }
        
        Dictionary<string, List<ISymbolInfo>> _symbolInfo;

        public ISymbolInfo this[string symbolName] {
            get {
                if (_symbolInfo == null) {
                    _symbolInfo = NMSymbolResolver.LoadSymbolInfo(_bin);
                }

                if (_symbolInfo[symbolName].Count > 1) {
                    throw new NotSupportedException("More than a single function with this name");
                }

                return _symbolInfo[symbolName].First();
            }
        }

        public bool HasSymbol(string symbolName) {
            return _symbolInfo.ContainsKey(symbolName);
        }

        public IEnumerable<ISymbolInfo> All {
            get {
                if (_symbolInfo == null) {
                    _symbolInfo = NMSymbolResolver.LoadSymbolInfo(_bin);
                }

                var l = new List<ISymbolInfo>();

                foreach (var kv in _symbolInfo) {
                    foreach (var symbolInfo in kv.Value) {
                        l.Add(symbolInfo);
                    }
                }

                return l;
            }
        }
    }
}