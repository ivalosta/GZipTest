using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    internal class Compressor : GZip
    {
        public Compressor(string input, string output) : base(input, output)
        {

        }

        public override void Launch()
        {
            Console.WriteLine("Compressing...\n");

            Thread _reader = new Thread(new ThreadStart(Read));
            _reader.Start();

            for (int i = 0; i < _threads; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(Compress, i);
            }

            Thread _writer = new Thread(new ThreadStart(Write));
            _writer.Start();

            WaitHandle.WaitAll(doneEvents);

            if (!_cancelled)
            {
                Console.WriteLine("\nCompressing has been succesfully finished");
                _success = true;
            }
        }

        private void Read()
        {
            try
            {
                using (FileStream _fileToBeCompressed = new FileStream(sourceFile, FileMode.Open))
                {

                    int bytesRead;
                    byte[] lastBuffer;

                    while (_fileToBeCompressed.Position < _fileToBeCompressed.Length && !_cancelled)
                    {
                        if (_fileToBeCompressed.Length - _fileToBeCompressed.Position <= blockSize)
                        {
                            bytesRead = (int)(_fileToBeCompressed.Length - _fileToBeCompressed.Position);
                        }

                        else
                        {
                            bytesRead = blockSize;
                        }

                        lastBuffer = new byte[bytesRead];
                        _fileToBeCompressed.Read(lastBuffer, 0, bytesRead);
                        _queueReader.EnqueueForCompressing(lastBuffer);
                        ConsoleProgress.ProgressBar(_fileToBeCompressed.Position, _fileToBeCompressed.Length);
                    }

                    _queueReader.Stop();
                    _readEnded = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _cancelled = true;
            }

        }

        private void Compress(object i)
        {
            try
            {
                ManualResetEvent doneEvent;
                while (true && !_cancelled && !_readEnded)
                {
                    ByteBlock _block = _queueReader.Dequeue();

                    if (_block == null)
                    {
                        doneEvent = doneEvents[(int)i];
                        doneEvent.Set();
                        return;
                    }

                    using (MemoryStream _memoryStream = new MemoryStream())
                    {
                        using (GZipStream cs = new GZipStream(_memoryStream, CompressionMode.Compress))
                        {
                            cs.Write(_block.Buffer, 0, _block.Buffer.Length);
                        }

                        byte[] compressedData = _memoryStream.ToArray();
                        ByteBlock _out = new ByteBlock(_block.Id, compressedData);
                        _queueWriter.EnqueueForWriting(_out);
                    }
                }

                doneEvent = doneEvents[(int)i];
                doneEvent.Set();

                if (_readEnded)
                {
                    _queueWriter.Stop();
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error in thread number {0}. \n Error description: {1}", i, ex.Message);
                _cancelled = true;
            }
        }

        private void Write()
        {
            try
            {
                using (FileStream _fileCompressed = new FileStream(destinationFile + ".gz", FileMode.Create))
                {
                    while (true && !_cancelled)
                    {
                        ByteBlock _block = _queueWriter.Dequeue();
                        if (_block == null)
                            return;

                        BitConverter.GetBytes(_block.Buffer.Length).CopyTo(_block.Buffer, 4);
                        _fileCompressed.Write(_block.Buffer, 0, _block.Buffer.Length);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _cancelled = true;
            }
        }
    }
}
