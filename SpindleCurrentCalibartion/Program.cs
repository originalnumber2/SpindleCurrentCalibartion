using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        //static double[] Speeds = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800 };
        static double[] Speeds = {100, 200, 300, 400, 500};

        static void Main(string[] args)
        {
            double[] Averages = new double[2];

            SetUp();
            for (int i = 0; i < Speeds.Length; i++)
            {
                Controller.StartSpindle(Speeds[i], true);
                Thread.Sleep(1000);
                //Writter.AddDoubleArray(data);
                double[][] data = TakeData();
                Averages[0] = Average.GetAverage(data[0]);
                Averages[1] = Average.GetAverage(data[1]);                     
                Console.WriteLine(Averages[0].ToString() + Averages[1].ToString());

            }
            Writter.AddDoubleArray(Averages);
            //Controller.StopSpindle();
        }

        private static double[][] TakeData()
        {
            
            double[][] data = new double[2][];
            data[0] = new double[DataPoints];
            data[1] = new double[DataPoints];

            for (int i = 0; i < DataPoints; i++)
            {
                double[] tempCurrentData = NIDaq.ReadUSBData();
                double[] tempTorqueData = dynomometer.ReadDyno();
                data[0][i] = tempCurrentData[1];
                data[1][i] = tempTorqueData[3];
                Thread.Sleep(10);
                //data[i] = random.NextDouble();
            }
            return data;
        }

        static void SetUp()
        {
            NIDaq = new NIDaqStrain();
            Controller = new MotorController();
            dynomometer = new Dyno();
            Writter = new CsvWriter(",", "Spindle.csv");
            Average = new Averager();
            Controller.ConnSpindle();
            dynomometer.DynoConnect();
            dynomometer.DynoReset();
            String[] header = { "Current", "Torque" };
            Writter.WriteHeader(header);
        }

    }
}
