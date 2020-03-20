using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    class Program
    {
        static GZip zipper;

        static int Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPress);

            try
            {
                /*
                args = new string[3];
                args[0] = @"compress";
                args[1] = @"C:\Users\Ivan\Downloads\20191230_162353.jpg";
                args[2] = @"C:\Users\Ivan\Downloads\zipped";
                */

                /*
                args = new string[3];
                args[0] = @"decompress";
                args[1] = @"C:\Users\Ivan\Downloads\zipped.gz";
                args[2] = @"C:\Users\Ivan\Downloads\unzipped.jpg";
                */

                Validation.StringReadValidation(args);

                switch (args[0].ToLower())
                {
                    case "compress":
                        zipper = new Compressor(args[1], args[2]);
                        break;
                    case "decompress":
                        zipper = new Decompressor(args[1], args[2]);
                        break;
                }

                zipper.Launch();
                return zipper.CallBackResult();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error is occured!\n Method: {0}\n Error description {1}", ex.TargetSite, ex.Message);
                return 1;
            }
        }

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs _args)
        {
            if (_args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Console.WriteLine("\nCancelling...");
                _args.Cancel = true;
                zipper.Cancel();
            }
        }
    }
}
