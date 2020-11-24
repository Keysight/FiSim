using System;
using System.IO;

using PlatformSim;
using PlatformSim.HwPeripherals;

namespace FiSim.Engine {
    internal class OtpPeripheral : HwPeripheralBase {
        private readonly string _otpBinPath;

        public bool PersistentChanges { get; set; } = false;

        public OtpPeripheral(string otpBinPath) {
            _otpBinPath = otpBinPath;
        }
        
        /*
         struct otp_ctrl_reg {
	         uint32_t status;
	         uint32_t cmd;
	         uint32_t address;
	         uint32_t data;
         };
        */
        
        private byte[] _otpOriginalData = new byte[0];
        
        class Ctx {
            internal uint Status;
            internal uint Address;
            internal uint Data;
            internal uint WaitingData;
            internal byte[] OTPData = new byte[0];
        }

        public override void OnRead(IPlatformEngine engine, ulong address, uint size) {
            var ctx = engine.GetState<Ctx>(this);
            
            switch (address & 0xFFF) {
                case 0: // status
                    engine.Write(address, BitConverter.GetBytes(ctx.Status));

                    if ((ctx.Status & 0x4) == 0x4) { // is ready?
                        ctx.Data = ctx.WaitingData;
                        ctx.Status ^= 0x4;
                    }

                    break;
                case 4: // cmd
                    engine.Write(address, new byte[4]);
                    break;
                case 8: // address
                    engine.Write(address, BitConverter.GetBytes(ctx.Address));
                    break;
                case 12: // data
                    engine.Write(address, BitConverter.GetBytes(ctx.Data));
                    break;
                default:
                    throw new InvalidHwOperationException(engine, "Unknown register");
            }
        }

        public override void OnWrite(IPlatformEngine engine, ulong address, uint size, ulong value) {
            var ctx = engine.GetState<Ctx>(this);
            
            switch (address & 0xFFF) {
                case 0: // status
                    break;
                case 4: // cmd
                    switch (value) {
                        case 1: // OTP_CMD_INIT
                            if (_otpOriginalData.Length == 0 && File.Exists(_otpBinPath)) {
                                _otpOriginalData = File.ReadAllBytes(_otpBinPath);    
                            }
                            else if (_otpOriginalData.Length == 0) {
                                _otpOriginalData = new byte[4096];
                            }

                            ctx.OTPData = _otpOriginalData;
                            
                            ctx.Status = 0x2; // ready
                            break;
                        
                        case 2: // OTP_CMD_READ
                            if ((ctx.Status & 0x2) == 0x2 && (ctx.Status & 0x4) == 0x0) { // is ready and not working?
                                if (ctx.Address + 4 < (ulong) ctx.OTPData.Length) {
                                    ctx.WaitingData = (uint) (ctx.OTPData[ctx.Address] + (ctx.OTPData[ctx.Address + 1] << 8) + (ctx.OTPData[ctx.Address + 2] << 16) + (ctx.OTPData[ctx.Address + 3] << 24));
                                    
                                    ctx.Status |= 0x4; // working
                                }
                                else {
                                    ctx.Status |= 0x1; // error
                                }
                            }
                            break;
                        
                        case 3: // OTP_CMD_WRITE
                            if ((ctx.Status & 0x2) == 0x2 && (ctx.Status & 0x4) == 0x0) { // is ready and not working?
                                if (ctx.Address + 4 < (ulong) ctx.OTPData.Length) {
                                    ctx.OTPData[ctx.Address] = (byte) (ctx.OTPData[ctx.Address] | (byte) (ctx.Data & 0xFF));
                                    ctx.OTPData[ctx.Address+1] = (byte) (ctx.OTPData[ctx.Address+1] | (byte) (ctx.Data>>8 & 0xFF));
                                    ctx.OTPData[ctx.Address+2] = (byte) (ctx.OTPData[ctx.Address+2] | (byte) (ctx.Data>>16 & 0xFF));
                                    ctx.OTPData[ctx.Address+3] = (byte) (ctx.OTPData[ctx.Address+3] | (byte) (ctx.Data>>24 & 0xFF));

                                    if (PersistentChanges) {
                                        _otpOriginalData = ctx.OTPData;
                                        
                                        File.WriteAllBytes(_otpBinPath, ctx.OTPData);
                                    }
                                    
                                    ctx.WaitingData = (uint) value;
                                    
                                    ctx.Status |= 0x4; // working
                                }
                                else {
                                    ctx.Status |= 0x1; // error
                                }
                            }
                            break;
                        
                        default:
                            throw new InvalidHwOperationException(engine, $"Unknown OTP command {value}");
                    }
                    break;
                case 8: // address
                    ctx.Address = (uint) value;
                    break;
                case 12: // data
                    ctx.Data = (uint) value;
                    break;
                default:
                    throw new InvalidHwOperationException(engine, "Unknown register");
            }
        }
    }
}