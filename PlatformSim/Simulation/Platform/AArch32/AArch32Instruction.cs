using System;
using System.Linq;

using Gee.External.Capstone;
using Gee.External.Capstone.Arm;

namespace PlatformSim.Simulation.Platform.AArch32 {
    internal class AArch32Instruction : InstructionBase {
        static readonly CapstoneArmDisassembler _armDisassembler =
            CapstoneDisassembler.CreateArmDisassembler(ArmDisassembleMode.Arm);

        static readonly CapstoneArmDisassembler _thumbDisassembler =
            CapstoneDisassembler.CreateArmDisassembler(ArmDisassembleMode.Thumb);

        public AArch32Instruction(ulong address, byte[] data) : base(address, data) {
        }

        public override IInstruction Clone() => new AArch32Instruction(Address, Data);

        private ArmInstruction _ins;

        private ArmInstruction Ins {
            get {
                if (_ins == null) {
                    if (Data.Length == 2) {
                        _ins = _thumbDisassembler.Disassemble(Data, (long) Address, 1)[0];
                    }
                    else if (Data.Length == 4) {
                        if (Data.SequenceEqual(new byte[] { 0x0, 0x0, 0x0, 0x0 })) {
                            _mnemonic = "NOP";
                            _operand = "";
                        }
                        else {
                            _ins = _armDisassembler.Disassemble(Data, (long) Address, 1)[0];
                        }
                    }
                    else {
                        throw new Exception();
                    }
                }

                return _ins;
            }
        }

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
                        _operand = Ins.Operand;
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
            catch (IndexOutOfRangeException) {
                return "<invalid instruction>";
            }
        }
    }
}