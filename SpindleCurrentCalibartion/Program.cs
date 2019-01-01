using System;
using System.Threading;

namespace SpindleCurrentCalibartion
{
    class Program
    {
        static NIDaqStrain NIDaq;
        static CsvWriter Writter;
        static Averager Average;
        static MotorController Controller;
        static Dyno dynomometer;
        //static Random random = new Random();
        static int DataPoints = 1000;
        static int SampleDelay = 10;
        static double[] Speeds = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800 };
        //static double[] Speeds = {100, 200, 300, 400, 500};
        static string Dir = "Data_" + DateTime.Now.ToString("yyyy-MM-dd hh-mm");

        static void Main(string[] args)
        {
            double[] Averages = new double[5];

            SetUp();
            for (int i = 0; i < Speeds.Length; i++)
            {
                Controller.StartSpindle(Speeds[i], true);
                Thread.Sleep(10000);
                //Writter.AddDoubleArray(data);
                double[][] data = TakeData(Speeds[i]);
                Averages[0] = Speeds[i];
                Averages[1] = Average.GetAverage(data[0]);
                Averages[2] = Average.GetAverage(data[1]);
                Averages[3] = Averages[1] * 230;
                Averages[4] = Averages[2] * Speeds[i] * Math.PI / 30;
                Controller.StartSpindle(0, true);
                Console.WriteLine(Averages[0].ToString() + ", " + Averages[1].ToString() + ", " + Averages[2].ToString() + ", " + Averages[3].ToString() + ", " + Averages[4].ToString());
                Writter.AddDoubleArray(Averages);
                Thread.Sleep(10000);
                dynomometer.DynoReset();
                Thread.Sleep(10000);
                
            }
            
            Controller.StopSpindle();
        }

        private static double[][] TakeData(double RPM)
        {
            CsvWriter csvWriter = new CsvWriter(",", Dir, "RPM" + RPM + ".csv");
            string[] header = { "Current", "Torque" };
            csvWriter.WriteHeader(header);
            double[][] data = new double[2][];
            data[0] = new double[DataPoints];
            data[1] = new double[DataPoints];

            for (int i = 0; i < DataPoints; i++)
            {
                double[] tempCurrentData = NIDaq.ReadUSBData();
                double[] tempTorqueData = dynomometer.ReadDyno();
                double[] tempWriterData = { tempCurrentData[1], tempTorqueData[3] };
                csvWriter.AddDoubleArray(tempWriterData);
                data[0][i] = tempCurrentData[1];
                data[1][i] = tempTorqueData[3];

                Thread.Sleep(SampleDelay);
                //data[i] = random.NextDouble();
            }
            return data;
        }

        static void SetUp()
        {
            NIDaq = new NIDaqStrain();
            Controller = new MotorController();
            dynomometer = new Dyno();
            Writter = new CsvWriter(",", Dir, "Averages.csv");
            Average = new Averager();
            Controller.ConnSpindle();
            dynomometer.DynoConnect();
            dynomometer.DynoReset();
            Thread.Sleep(500);
            String[] header1 = { "DataPoints:", DataPoints.ToString(), "SampleDelay:", SampleDelay.ToString() };
            String[] header2 = { "RPM", "Current", "Torque", "Power (Current)", "Power (Torque)" };
            Writter.WriteHeader(header1);
            Writter.WriteHeader(header2);
        }

    }
}
