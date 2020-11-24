using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace FiSim.GUI {
    internal class OutputConsoleWriter : TextWriter {
        private readonly TextBlock _outputView;
        private readonly ScrollViewer _outputViewScrollViewer;

        private readonly ConcurrentMemoryStream _output;

        private readonly StreamReader _outputReader;
        private readonly StreamWriter _outputWriter;

        private readonly Thread _outputThread;
        
        private bool _isDisposed;

        public OutputConsoleWriter(TextBlock outputView, ScrollViewer outputViewScrollViewer) {
            _outputView = outputView;
            _outputViewScrollViewer = outputViewScrollViewer;

            _outputView.Dispatcher.Invoke(() => { _outputView.Text = ""; });

            _output = new ConcurrentMemoryStream();

            _outputReader = new StreamReader(_output);
            _outputWriter = new StreamWriter(_output) {
                AutoFlush = true
            };

            _outputThread = new Thread(() => {
                while (!_isDisposed || !_outputReader.EndOfStream) {
                    while (_outputReader.EndOfStream) {
                        Thread.Sleep(1);
                    }

                    var output = _outputReader.ReadLine() + Environment.NewLine;

                    _outputView.Dispatcher.Invoke(() => { _outputView.Text += output; _outputViewScrollViewer.ScrollToEnd(); });
                }
                
                _outputReader.Dispose();
                _output.Dispose();
            }) {IsBackground = true};
            _outputThread.Start();
        }

        public override Encoding Encoding { get; } = Encoding.UTF8;

        public override void Write(char value) {
            _outputWriter.Write(value);
        }
        
        protected override void Dispose(bool disposing) {
            if (disposing) {
                _isDisposed = true;
                
                _outputWriter?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    // https://stackoverflow.com/questions/12328245/memorystream-have-one-thread-write-to-it-and-another-read
    class ConcurrentMemoryStream : Stream {
        private readonly MemoryStream _innerStream;
        private          long         _readPosition;
        private          long         _writePosition;

        public ConcurrentMemoryStream() => _innerStream = new MemoryStream();

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override void Flush() {
            lock (_innerStream) {
                _innerStream.Flush();
            }
        }

        public override long Length {
            get {
                lock (_innerStream) {
                    return _innerStream.Length;
                }
            }
        }

        public override long Position {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            lock (_innerStream) {
                _innerStream.Position = _readPosition;
                var red = _innerStream.Read(buffer, offset, count);
                _readPosition = _innerStream.Position;

                return red;
            }
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) {
            lock (_innerStream) {
                _innerStream.Position = _writePosition;
                _innerStream.Write(buffer, offset, count);
                _writePosition = _innerStream.Position;
            }
        }
    }
}
