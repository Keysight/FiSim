using UnicornManaged.Const;

namespace PlatformSim.Simulation.Engine {
    public abstract class ArchInfo {
        public virtual int[] GPR { get; }

        public virtual int FP { get; }
        public virtual int SP { get; }
        public virtual int PC { get; }

        public virtual int NativeWordSize { get; }

        public static readonly AArch32Info AArch32 = new AArch32Info();

        public static readonly AArch64Info AArch64 = new AArch64Info();
        
        public static readonly MIPS32Info MIPS32 = new MIPS32Info();
    }

    public sealed class AArch32Info : ArchInfo {
        public override int[] GPR { get; } = {
        Arm.UC_ARM_REG_R0, Arm.UC_ARM_REG_R1, Arm.UC_ARM_REG_R2, Arm.UC_ARM_REG_R3, Arm.UC_ARM_REG_R4, Arm.UC_ARM_REG_R5, Arm.UC_ARM_REG_R6, Arm.UC_ARM_REG_R7,
        Arm.UC_ARM_REG_R8, Arm.UC_ARM_REG_R9, Arm.UC_ARM_REG_R10, Arm.UC_ARM_REG_R11, Arm.UC_ARM_REG_R12, Arm.UC_ARM_REG_R13
        };
        
        public int LR { get; } = Arm.UC_ARM_REG_LR;

        public override int FP { get; } = Arm.UC_ARM_REG_FP;
        public override int SP { get; } = Arm.UC_ARM_REG_SP;
        public override int PC { get; } = Arm.UC_ARM_REG_PC;

        public override int NativeWordSize { get; } = 32;
    }

    public sealed class AArch64Info : ArchInfo {
        public override int[] GPR { get; } = {
        Arm64.UC_ARM64_REG_X0, Arm64.UC_ARM64_REG_X1, Arm64.UC_ARM64_REG_X2, Arm64.UC_ARM64_REG_X3, Arm64.UC_ARM64_REG_X4, Arm64.UC_ARM64_REG_X5,
        Arm64.UC_ARM64_REG_X6, Arm64.UC_ARM64_REG_X7, Arm64.UC_ARM64_REG_X8, Arm64.UC_ARM64_REG_X9, Arm64.UC_ARM64_REG_X10, Arm64.UC_ARM64_REG_X11,
        Arm64.UC_ARM64_REG_X12, Arm64.UC_ARM64_REG_X13, Arm64.UC_ARM64_REG_X14, Arm64.UC_ARM64_REG_X15, Arm64.UC_ARM64_REG_X16, Arm64.UC_ARM64_REG_X17,
        Arm64.UC_ARM64_REG_X18, Arm64.UC_ARM64_REG_X19, Arm64.UC_ARM64_REG_X20, Arm64.UC_ARM64_REG_X21, Arm64.UC_ARM64_REG_X22, Arm64.UC_ARM64_REG_X23,
        Arm64.UC_ARM64_REG_X24, Arm64.UC_ARM64_REG_X25, Arm64.UC_ARM64_REG_X26, Arm64.UC_ARM64_REG_X27, Arm64.UC_ARM64_REG_X28, Arm64.UC_ARM64_REG_X29,
        Arm64.UC_ARM64_REG_X30
        };

        public int LR { get; } = Arm64.UC_ARM64_REG_LR;
        
        public override int FP { get; } = Arm64.UC_ARM64_REG_FP;
        public override int SP { get; } = Arm64.UC_ARM64_REG_SP;
        public override int PC { get; } = Arm64.UC_ARM64_REG_PC;

        public override int NativeWordSize { get; } = 64;
    }
    
    public sealed class MIPS32Info : ArchInfo {
        public override int[] GPR { get; } = {
            Mips.UC_MIPS_REG_0, Mips.UC_MIPS_REG_1,Mips.UC_MIPS_REG_2,Mips.UC_MIPS_REG_3,
            Mips.UC_MIPS_REG_4,Mips.UC_MIPS_REG_5, Mips.UC_MIPS_REG_6, Mips.UC_MIPS_REG_7, 
            Mips.UC_MIPS_REG_8,Mips.UC_MIPS_REG_9,Mips.UC_MIPS_REG_10,Mips.UC_MIPS_REG_11,
            Mips.UC_MIPS_REG_12,Mips.UC_MIPS_REG_13, Mips.UC_MIPS_REG_14,Mips.UC_MIPS_REG_15,
            Mips.UC_MIPS_REG_16,Mips.UC_MIPS_REG_17,Mips.UC_MIPS_REG_18, Mips.UC_MIPS_REG_19,
            Mips.UC_MIPS_REG_20, Mips.UC_MIPS_REG_21,Mips.UC_MIPS_REG_22,Mips.UC_MIPS_REG_23,
            Mips.UC_MIPS_REG_24,Mips.UC_MIPS_REG_25,Mips.UC_MIPS_REG_26,Mips.UC_MIPS_REG_27,
            Mips.UC_MIPS_REG_28,Mips.UC_MIPS_REG_29,Mips.UC_MIPS_REG_30,Mips.UC_MIPS_REG_31,
        };
        
        public int LR { get; } = Mips.UC_MIPS_REG_RA;

        public override int FP { get; } = Mips.UC_MIPS_REG_FP;
        public override int SP { get; } = Mips.UC_MIPS_REG_SP;
        public override int PC { get; } = Mips.UC_MIPS_REG_PC;

        public override int NativeWordSize { get; } = 32;
    }
}