using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SpindleCurrentCalibartion
{
    class CsvWriter
    {

        String csvSeperator;
        String csvPath;
        StreamWriter outputfile;

        public CsvWriter(string seperator, string filepath)
        {
            csvSeperator = seperator;
            csvPath = filepath;
            outputfile = new StreamWriter(filepath);
        }

        public void WriteHeader(String[] header)
        {
            string NewLine = "";
            int i = 1;
            foreach (string heading in header)
            {
                if (i != header.Length)
                {
                    NewLine = NewLine + heading + csvSeperator;
                    i++;
                }
                else { NewLine = NewLine + heading; }
            }
            outputfile.WriteLine(NewLine);
        }

        //Adds all data from a double array to a file with csvSeperatore between elements
        public void AddDoubleArray(double[] dataArray)
        {
            string NewLine = "";
            int i = 1;

            //PrintDoubleToConsole(dataArray);

            foreach (double data in dataArray)
            {
                if (i != dataArray.Length)
                {
                    NewLine = NewLine + data.ToString() + csvSeperator;
                    i++;
                }
                else { NewLine = NewLine + data.ToString(); }
            }
            //Console.WriteLine(NewLine);
            outputfile.WriteLine(NewLine);
        }

        //prints data arry from dyno to console. doesnt check that it is the
        //right array first
        void PrintDoubleToConsole(double[] dataArray)
        {
            Console.WriteLine("XForce: " + dataArray[0]);
            Console.WriteLine("YForce: " + dataArray[1]);
            Console.WriteLine("ZForceDyno: " + dataArray[2]);
            Console.WriteLine("TForce: " + dataArray[3]);
            Console.WriteLine("VAngle: " + dataArray[4]);
            Console.WriteLine("XYForce: " + dataArray[5]);
            Console.WriteLine("XYForceAverage: " + dataArray[6]);
            Console.WriteLine("");
            Console.WriteLine("");

        }

    }
}
