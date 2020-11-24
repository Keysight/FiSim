namespace BinInfo {
    public interface IBinInfo {
        string Path { get; }
    
        ISourceLineResolver SourceLine { get; }
        
        ISymbolResolver Symbols { get; }
        
        string Symbolize(ulong address);
    }
}