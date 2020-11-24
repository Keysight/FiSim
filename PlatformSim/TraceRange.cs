namespace PlatformSim {
    public class TraceRange {
        public ulong Start { get; set; }

        public ulong Size { get; set; }

        public ulong End => Start + Size; // Exclusive

        public bool Contains(ulong value) {
            return Start <= value && value < End;
        }

        public override string ToString() {
            return $"[{Start:X} - {End:X}]";
        }
    }
}