using System;
using System.Collections.Generic;
using System.Linq;

namespace BinInfo.ELF {
    public class ELFSymbolResolver : ISymbolResolver {
        private readonly ELFBinInfo _bin;
        
        private readonly string _toolchainPath;
        private readonly string _toolchainPrefix;

        public ELFSymbolResolver(ELFBinInfo bin, string toolchainPath, string toolchainPrefix) {
            _bin = bin;
            _toolchainPath = toolchainPath;
            _toolchainPrefix = toolchainPrefix;
        }
        
        Dictionary<string, List<ISymbolInfo>> _symbolInfo;

        public ISymbolInfo this[string symbolName] {
            get {
                lock (this) {
                    if (_symbolInfo == null) {
                        _symbolInfo = NMSymbolResolver.LoadSymbolInfo(_bin, _toolchainPath, _toolchainPrefix);
                    }
                }

                if (!_symbolInfo.ContainsKey(symbolName)) {
                    throw new KeyNotFoundException("No symbol with name " + symbolName);
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
                lock (this) {
                    if (_symbolInfo == null) {
                        _symbolInfo = NMSymbolResolver.LoadSymbolInfo(_bin, _toolchainPath, _toolchainPrefix);
                    }
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
