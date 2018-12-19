using System;
using System.Threading;

namespace SpindleCurrentCalibartion
{
    public class NIDaqStrain
    {
        readonly double NIMaxVolt = 1;
        readonly double NIMinVolt = 0;
        readonly string loc = "dev3";
        bool IsStrainCon;


        //private NationalInstruments.DAQmx.Task USB6008_2_AITask;
        private int count; //number of samples to take when zeroing the Straingauges
        internal double Zoffset, SpiCurrent, ZForce;
        Semaphore USB6008_Mutex = new Semaphore(1, 1);//I know, it is not a mutex, but had some weird issues using mutex across multiple threads so this is my solution to use a sempafore as a mutex

        private NationalInstruments.DAQmx.Task USB6008_AITask;
        private NationalInstruments.DAQmx.Task USB6008_AOTask;
        private NationalInstruments.DAQmx.Task USB6008_DOTask;
        private AnalogMultiChannelReader USB6008_Reader;
        private AnalogMultiChannelWriter USB6008_Analog_Writter;
        private DigitalSingleChannelWriter USB6008_Digital_Writter;

        public NIDaqStrain()
        {
            count = 5; //I am not so sure i like this implementation of the counter. 
            Zoffset = 0;
            SpiCurrent = 0;
            ZForce = 0;
            Setup_USB6008();
            SetOffset();
        }


        public double[] ReadUSBData()
        {
            double[] data = { 0, 0 };
            try
            {
                USB6008_Mutex.WaitOne();
                data = USB6008_Reader.ReadSingleSample();
                USB6008_Mutex.Release();
            }
            catch (NationalInstruments.DAQmx.DaqException ex)
            {
                Console.WriteLine("usb6008 1 read error" + ex.Message.ToString());
                //error has occured need to reset daq
                Setup_USB6008();
                Thread.Sleep(50);
            }
            return ConvertData(data);
        }

        double[] ConvertData(double[] data)
        {
            ZForce = 21050 * (data[0] - Zoffset); //BG commented out above 4 lines and added this to report full range (+ and -) Z values for Russ's bobbin welding
                                                  //Read in rotation angles of the lateral traverse and vertical using the second ni usb6008
            SpiCurrent = data[1] * 10;            //Subtract offset and multiply gain....Recalibrated from 17760 N/V on 9/16/2011, Recalibrated from 18843 N/V on 10/5/2011, Recalibrated from 19785 N/V on 5/7/2012

            double[] rData = { ZForce, SpiCurrent };

            return rData;
        }

        public void Setup_USB6008()
        {

            //Resets and configures the NI USB6008 Daq boards
            Device dev = DaqSystem.Local.LoadDevice(loc);//added to reset the DAQ boards if they fail to comunicate giving error code 50405
            dev.Reset();
            AIChannel StrainChannel, CurrentChannel;
            AOChannel LateralMotorChannel, TraverseMotorChannel;
            try
            {
                //Setting up NI DAQ for Axial Force Measurment via Strain Circuit and current Measurment of Spindle Motor for torque 
                USB6008_AITask = new NationalInstruments.DAQmx.Task();

                StrainChannel = USB6008_AITask.AIChannels.CreateVoltageChannel(
                    loc + "/ai0",  //Physical name of channel
                    "strainChannel",  //The name to associate with this channel
                    AITerminalConfiguration.Differential,  //Differential Wiring
                    -0.1,  //-0.1v minimum
                    NIMaxVolt,  //1v maximum
                    AIVoltageUnits.Volts  //Use volts
                    );
                CurrentChannel = USB6008_AITask.AIChannels.CreateVoltageChannel(
                   loc + "/ai1",  //Physical name of channel
                   "CurrentChannel",  //The name to associate with this channel
                   AITerminalConfiguration.Differential,  //Differential Wiring
                   -0.1,  //-0.1v minimum
                   10,  //10v maximum
                   AIVoltageUnits.Volts  //Use volts
                   );
                USB6008_Reader = new AnalogMultiChannelReader(USB6008_1_AITask.Stream);
                ////////////////////////////////////////////////////////////
                USB6008_AOTask = new NationalInstruments.DAQmx.Task();
                TraverseMotorChannel = USB6008_AOTask.AOChannels.CreateVoltageChannel(
                    loc + "/ao0",  //Physical name of channel)
                    "TravverseMotorChannel",  //The name to associate with this channel
                    0,  //0v minimum
                    5,  //5v maximum
                    AOVoltageUnits.Volts
                    );
                LateralMotorChannel = USB6008_AOTask.AOChannels.CreateVoltageChannel(
                    loc + "/ao1",  //Physical name of channel)
                    "LateralMotorChannel",  //The name to associate with this channel
                    0,  //0v minimum
                    5,  //5v maximum
                    AOVoltageUnits.Volts
                    );
                USB6008_Analog_Writter = new AnalogMultiChannelWriter(USB6008_AOTask.Stream);
                ////////////////////////////////////////////////////////////
                USB6008_DOTask = new NationalInstruments.DAQmx.Task();
                USB6008_DOTask.DOChannels.CreateChannel(loc + "/port0", "port0", ChannelLineGrouping.OneChannelForAllLines);
                USB6008_Digital_Writter = new DigitalSingleChannelWriter(USB6008_DOTask.Stream);
            }
            catch (NationalInstruments.DAQmx.DaqException e)
            {
                MessageBox.Show("Error?\n\n" + e.ToString(), "NI USB 6008 1 Error");
            }
        }

        double ZeroStrainGauges(int x)
        {
            double ZMeasureSum = 0;

            for (int i = 0; i < x; i++)
            {
                try
                {
                    USB6008_Mutex.WaitOne();
                    ZMeasureSum += USB6008_Reader.ReadSingleSample()[0];
                    USB6008_Mutex.Release();
                }
                catch (NationalInstruments.DAQmx.DaqException ex)
                {
                    MessageBox.Show("Error Reading Analog input for zero button " + ex.Message.ToString());
                }
                Thread.Sleep(5);
            }
            return ZMeasureSum / count;
        }

        void SetOffset()
        {
            Zoffset = ZeroStrainGauges(count);
        }
    }
}

