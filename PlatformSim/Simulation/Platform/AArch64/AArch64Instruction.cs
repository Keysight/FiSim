using System;

using Gee.External.Capstone;
using Gee.External.Capstone.Arm64;

namespace PlatformSim.Simulation.Platform.AArch64 {
    internal class AArch64Instruction : InstructionBase {
        static readonly CapstoneArm64Disassembler _armDisassembler = CapstoneDisassembler.CreateArm64Disassembler(0);

        public AArch64Instruction(ulong address, byte[] data) : base(address, data) { }

       public override IInstruction Clone() => new AArch64Instruction(Address, Data);

        private Arm64Instruction _ins;

        private Arm64Instruction Ins => _ins ?? (_ins = _armDisassembler.Disassemble(Data, (long) Address, 1)[0]);

        private string _mnemonic;
        public override string Mnemonic {
            get {
                if (_mnemonic == null) {
                    if (Ins != null) {
                        _mnemonic = Ins.Mnemonic;
                    }
                }

                return _mnemonic;
            }
            set => _mnemonic = value;
        }

        private string _operand;
        public override string Operand {
            get {
                if (_operand == null) {
                    if (Ins != null) {
                        _operand = Ins.Mnemonic;
                    }
                }

                return _operand;
            }
            set => _operand = value;
        }

        public override string ToString() {
            try {
                if (!string.IsNullOrEmpty(Operand)) {
                    return $"{Mnemonic.ToUpper()} {Operand}";
                }
                else {
                    return $"{Mnemonic.ToUpper()}";
                }
            }
            catch (MethodAccessException) {
                return "<internal error>";
            }
            catch (InvalidOperationException) {
                return "<invalid instruction>";
            }
        }
    }
}