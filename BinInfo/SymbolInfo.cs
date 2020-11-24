namespace BinInfo {
    public class SymbolInfo : ISymbolInfo {
        public ulong Address { get; set; }
        
        public ulong Size { get; set; }

        public string Name { get; set; }
    }
}