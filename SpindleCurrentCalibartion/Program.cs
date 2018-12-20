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
        //static NIDaqStrain NIDaq;
        static CsvWriter Writter;
        static Averager Average;
        //static MotorController Controller;
        static Random random = new Random();
        static int DataPoints = 1000;
        //static double[] Speeds = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800 };
        static double[] Speeds = {100, 200, 300, 400, 500};

        static void Main(string[] args)
        {


            double[] data;
            double[] Averages = new double[Speeds.Length];

            SetUp();
            for (int i = 0; i < Speeds.Length; i++)
            {
                data = TakeRPMData(Speeds[i]);
                //Writter.AddDoubleArray(data);
                Averages[i] = Average.GetAverage(data);
                Console.WriteLine(Averages[i]);
                Thread.Sleep(100);
            }
            Writter.AddDoubleArray(Averages);
            //Controller.StopSpindle();
        }

        private static double[] TakeRPMData(double rpm)
        {
            //Controller.StartSpindle(rpm, true);
            double[] data = new double[DataPoints];
            for (int i = 0; i < DataPoints; i++)
            {
                //double[] tempData = NIDaq.ReadUSBData();
                //data[i] = tempData[1];
                data[i] = random.NextDouble();
            }
            return data;
        }

        static void SetUp()
        {
            //NIDaq = new NIDaqStrain();
            //Controller = new MotorController();
            Writter = new CsvWriter(",", "Spindle.csv");
            Average = new Averager();
            //Controller.ConnSpindle();
            String[] header = { "hello", "please", "Work" };
            Writter.WriteHeader(header);
        }

    }
}
