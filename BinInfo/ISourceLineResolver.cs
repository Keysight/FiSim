namespace BinInfo {
    public interface ISourceLineResolver {
        ISourceLineInfo this[ulong address] { get; }
    }
}