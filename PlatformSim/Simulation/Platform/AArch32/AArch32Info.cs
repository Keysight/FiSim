namespace PlatformSim.Simulation.Platform.AArch32 {
    public static class AArch32Info {
        public static readonly byte[] A32_NOP = { 0x00, 0x00, 0x00, 0x00 };
        public static readonly byte[] A32_RET = { 0x0E, 0xF0, 0xA0, 0xE1 };
        
        public static readonly byte[] A32_MOV_R0_0_RET = { 0x00, 0x00, 0xa0, 0xe3, 0x0E, 0xF0, 0xA0, 0xE1 };
        
        public static readonly byte[] A32_B_SELF = { 0xfe, 0xff, 0xff, 0xea };

        public static readonly byte[] T32_RET = { 0x70, 0x47 };
    }
}