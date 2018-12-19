using System;
namespace SpindleCurrentCalibartion
{
    public class Averager
    {
        public Averager()
        {
        }


        internal double GetAverage(double[] data)
        {
            int width = data.Length;
            double accumulator = 0;
            for (int i = 0; i < width; i++)
            {
                accumulator = accumulator + data[i];
            }
            return accumulator / width;

        }
    }
}
