using BinInfo;

namespace PlatformSim {
    public class SymbolTraceRange : TraceRange {
        public SymbolTraceRange(ISymbolInfo symbolInfo) {
            Start = symbolInfo.Address;
            Size = symbolInfo.Size;
        }
    }
}