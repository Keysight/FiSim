namespace BinInfo {
    public class SourceLineInfo : ISourceLineInfo {
        public string FilePath { get; set; }

        public uint LineNumber { get; set; }

        public string FunctionName { get; set; }
        
        public override string ToString() {
            if (FilePath != null) {
                return $"{FunctionName} ({FilePath}:{LineNumber})";
            }
            else {
                return $"{FunctionName}";
            }
        }
    }
}