namespace PlatformSim.HwPeripherals {
    public delegate void HwPeripheralOnRead(IPlatformEngine  engine, ulong address, uint size);
    public delegate void HwPeripheralOnWrite(IPlatformEngine engine, ulong address, uint size, ulong value);
    
    public class HwPeripheral : HwPeripheralBase {
        readonly HwPeripheralOnRead  _onRead;
        readonly HwPeripheralOnWrite _onWrite;

        public HwPeripheral(HwPeripheralOnRead onRead, HwPeripheralOnWrite onWrite = null, ulong size = 0x1000) {
            _onRead  = onRead;
            _onWrite = onWrite;

            Size = size;
        }
        
        public HwPeripheral(HwPeripheralOnWrite onWrite, ulong size = 0x1000) {
            _onWrite = onWrite;
            
            Size = size;
        }

        public override ulong Size { get; }

        public override void OnRead(IPlatformEngine engine, ulong address, uint size) {
            _onRead?.Invoke(engine, address, size);
        }

        public override void OnWrite(IPlatformEngine engine, ulong address, uint size, ulong value) {
            _onWrite?.Invoke(engine, address, size, value);
        }
    }
}