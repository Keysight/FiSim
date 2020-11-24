namespace BinInfo {
    public interface ISourceLineInfo {
        string FilePath { get; }

        uint LineNumber { get; }

        string FunctionName { get; }
    }
}