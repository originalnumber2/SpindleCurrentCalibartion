using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MccDaq;
using System.IO.Ports;
using System.Threading;

namespace SpindleCurrentCalibartion
{
    class Dyno
    {
        //class variables (some of these need to move to the constructor
        //Connection Status of the Dyno
        public bool isDynCon;

        //might want move this into another structure if i need to 
        //Dynamometer Structures
        MccDaq.MccBoard DaqBoard;
        MccDaq.ErrorInfo ULStat;
        MccDaq.Range Range;
        MccDaq.ScanOptions Options;
        SerialPort DynoControlPort;

        //implementing a new averaging object to make things clean and mantain averaging arrays    
        int filterLength;
        MovingAverage averager;

        //constuctor for the dyno
        public Dyno()
        {
            isDynCon = false;

            //Initialize Error Handling
            ULStat = MccDaq.MccService.ErrHandling(MccDaq.ErrorReporting.PrintAll, MccDaq.ErrorHandling.StopAll);

            //Create an object for board 0
            DaqBoard = new MccDaq.MccBoard(0);

            //Set the range
            Range = MccDaq.Range.Bip10Volts;
            //  return scaled data
            Options = MccDaq.ScanOptions.ScaleData;

            // creating and defining the objects for the moving average
            filterLength = 5;
            averager = new MovingAverage(filterLength);

            // Try to initialize the dyno control port and catch any errors
            try
            {
                //Set up port for controlling the dyno
                DynoControlPort = new SerialPort();
                DynoControlPort.PortName = "COM4";
                DynoControlPort.BaudRate = 4800;
                DynoControlPort.Parity = Parity.None;
                DynoControlPort.StopBits = StopBits.One;
                DynoControlPort.DataBits = 8;
                DynoControlPort.Open();
            }
            catch (System.IO.IOException e)
            {
                //MessageBox.Show("Open device manager in windows and the 'Setup Serial Ports' section of the C# code and check the serial port names and settings are correct\n\n" + e.ToString(), "Serial Port Error");
                //Process.GetCurrentProcess().Kill();

            }
            catch (System.UnauthorizedAccessException e)
            {
                //MessageBox.Show("Something is wrong? maybe try to restart computer?\n\nHere is some error message stuff...\n\n" + e.ToString(), "Serial Port Error");
                //Process.GetCurrentProcess().Kill();
            }

        }

        //disconntect the dyno
        public void DynoDisconnect()
        {
            //Release Control of the Dyno
            DynoControlPort.Write("CR0\r");
            isDynCon = false;
            Console.WriteLine("Dyno Disconnected");
        }

        public void DynoConnect()
        {
            //Set up rs232 control:Set the range to one:Reset the Dyno
            DynoControlPort.Write("CR1:RG0:RO0\r");

            Thread.Sleep(100);

            //operate the Dyno
            DynoControlPort.Write("RO1\r");
            Thread.Sleep(100);

            isDynCon = true;
            Console.WriteLine("Dyno Connected");
        }

        public void DynoReset()
        {
            //Reset and then operate the Dyno
            DynoControlPort.Write("RO0\r");

            Thread.Sleep(500);

            DynoControlPort.Write("RO1\r");
            Console.WriteLine("Dyno Reset");
        }

        double Count2Volt(short a)
        {
            //convert count given from PCIM-DAS 1602 DAQ Card to a voltage
            //return ((double)a * 10.0 / 65535.0 - 5);
            float b;
            DaqBoard.ToEngUnits(Range, a, out b);
            //Console.WriteLine(b);
            return b;
        }

        public double[] ReadDyno()
        {
            //establish all the varables for the data
            double[] reading = new double[7];
            double XForce, YForce, ZForceDyno, TForce, VAngle, XYForce, XYForceAverage;

            //sampleing from all 8 channels, DAQ has a max sampling rate of 10,000Hz so for sampleing 8 channels the rate=10,000/8=1,250
            int LowChan = 0, HighChan = 7, Rate = 1250, Count = 8;//JNEW

            //IntPtr MemHandle = IntPtr.Zero;//JNEW //dont think this is needed here
            int NumPoints = 8;    //  Number of data points to collect //JNEW
            int FirstPoint = 0;     //  set first element in buffer to transfer to array //JNEW
            short[] ADData = new short[NumPoints];//JNEW

            IntPtr MemHandle = MccDaq.MccService.ScaledWinBufAllocEx(NumPoints);//JNEW

            //  return scaled data (dont know exactly what this means)
            Options = MccDaq.ScanOptions.ConvertData | MccDaq.ScanOptions.SingleIo;

            //Options = MccDaq.ScanOptions.ScaleData;
            //Range = MccDaq.Range.Bip5Volts; // set the range
            //Range = MccDaq.Range.Bip10Volts; // set the range

            ULStat = DaqBoard.AInScan(LowChan, HighChan, Count, ref Rate, Range, MemHandle, Options);

            if (ULStat.Value == MccDaq.ErrorInfo.ErrorCode.BadRange)
            {
                Console.WriteLine("Change the Range argument to one supported by this board.");
            }
            //  Transfer the data from the memory buffer set up by Windows to an array
            ULStat = MccDaq.MccService.WinBufToArray(MemHandle, ADData, FirstPoint, Count);

            //calibration variables (probably from kistler)
            //Copy into local variables
            XForce = 487.33 * Count2Volt(ADData[0]);
            YForce = 479.85 * Count2Volt(ADData[1]);
            ZForceDyno = 2032.52 * Count2Volt(ADData[2]);
            TForce = 18.91 * Count2Volt(ADData[3]);
            VAngle = Count2Volt(ADData[7]);
            XYForce = Math.Sqrt(XForce * XForce + YForce * YForce);
            XYForceAverage = averager.calculateAverage(XYForce);

            reading[0] = XForce;
            reading[1] = YForce;
            reading[2] = ZForceDyno;
            reading[3] = TForce;
            reading[4] = VAngle;
            reading[5] = XYForce;
            reading[6] = XYForceAverage;

            return reading;
        }

    }

}