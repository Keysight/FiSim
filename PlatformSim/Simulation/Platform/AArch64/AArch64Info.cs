namespace PlatformSim.Simulation.Platform.AArch64 {
    public static class AArch64Info {
        public static readonly byte[] NOP = { 0x1F, 0x20, 0x03, 0xD5 };

        public static readonly byte[] RET = { 0xc0, 0x03, 0x5f, 0xd6 };
        
        public static readonly byte[] MOV_X0_0_RET = { 0x00, 0x00, 0x80, 0xd2, 0xc0, 0x03, 0x5f, 0xd6 };
    }
}