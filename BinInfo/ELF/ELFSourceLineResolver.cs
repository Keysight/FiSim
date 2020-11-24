using System.Collections.Generic;

namespace BinInfo.ELF {
    public class ELFSourceLineResolver : ISourceLineResolver {
        private readonly ELFBinInfo _bin;
        
        private readonly string _toolchainPath;
        private readonly string _toolchainPrefix;

        public ELFSourceLineResolver(ELFBinInfo bin, string toolchainPath, string toolchainPrefix) {
            _bin = bin;
            
            _toolchainPath = toolchainPath;
            _toolchainPrefix = toolchainPrefix;
        }

        readonly Dictionary<ulong, ISourceLineInfo> _sourceLineInfo = new Dictionary<ulong, ISourceLineInfo>();

        public ISourceLineInfo this[ulong address] {
            get {
                lock (this) {
                    if (!_sourceLineInfo.ContainsKey(address)) {
                        var sourceInfoList = Addr2LineResolver.LoadSourceInfo(_bin, new List<ulong> { address }, _toolchainPath, _toolchainPrefix);

                        if (sourceInfoList.Count == 1) {
                            _sourceLineInfo.Add(address, sourceInfoList[address]);
                        }
                        else {
                            throw new KeyNotFoundException($"Cannot find source info for address {address:x16}");
                        }
                    }
                }

                return _sourceLineInfo[address];
            }
        }
    }
}