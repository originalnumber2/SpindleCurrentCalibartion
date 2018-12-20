namespace SpindleCurrentCalibartion
{
    class MovingAverage
    {

        private int windowSize;
        private double[] window;
        private int position;


        public MovingAverage(int filterLength)
        {
            windowSize = filterLength;
            window = new double[windowSize];
            position = 0;
        }

        //the array could be migrated to a queue (first in last out) for elegance. But
        //that will be an exersize for another time
        public double calculateAverage(double data)
        {
            double accumulator = 0;
            double average = 0;
            window[position] = data;

            for (int i = 0; i < windowSize; i++)
            {
                accumulator = accumulator + window[i];
            }

            position = ++position % windowSize;
            average = accumulator / windowSize;
            return average;
        }


    }
}
