using System.Collections.Generic;

namespace BinInfo {
    public interface ISymbolResolver {
        ISymbolInfo this[string symbolName] { get; }

        bool HasSymbol(string symbolName);
        
        IEnumerable<ISymbolInfo> All { get; }
    }
}