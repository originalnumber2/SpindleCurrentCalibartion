using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpindleCurrentCalibartion
{
    class Program
    {
        static NIDaqStrain NIDaq;
        static CsvWriter Writter;
        static Averager Average;
        static MotorController Controller;
        static double DataPoints = 1000;
        static double[] Speeds = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800 };


        static void Main(string[] args)
        {


            double[] data = { };
            double[] Averages = { };

            SetUp();
            for (int i = 0; i < Speeds.Length; i++)
            {
                data = TakeRPMData(100);
                Averages[i] = Average.GetAverage(data);
            }
        }

        private static double[] TakeRPMData(double rpm)
        {
            Controller.StartSpindle(100, true);
            double[] data = { };
            for (int i = 0; i < DataPoints; i++)
            {
                double[] rData = NIDaq.ReadUSBData();
                data[i] = rData[1];
            }
            return data;
        }

        static void SetUp()
        {
            NIDaq = new NIDaqStrain();
            Controller = new MotorController();
            Writter = new CsvWriter(",", "");
            Average = new Averager();

            Controller.ConnSpindle();
        }

    }
}
