namespace BinInfo {
    public interface ISymbolInfo {
        ulong Address { get; }
        
        ulong Size { get; }
        
        string Name { get; }
    }
}