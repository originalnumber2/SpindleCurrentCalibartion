using System;

namespace SpindleCurrentCalibartion
{
    // Simplificatin and lubrication control is needed. if the spindle is going to fast the lube is required.
    public class SpindleMotor
    {

        MotorController controller;

        public bool isSpiCon;
        public bool isSimulinkControl;
        public double SpiRPM;
        public bool SpiDir; //Spindle direction true - Clockwise false - counter clockwise
        public double RPMmin;
        public double RPMmax;
        private double epsilon;

        public SpindleMotor(MotorController motorController)
        {
            isSpiCon = false;
            isSimulinkControl = false;
            SpiRPM = 0;
            SpiDir = true;
            RPMmax = 2000;
            RPMmin = 0;

            controller = motorController;

        }

        internal string ConnectionToggle()
        {
            string message;
            if (isSpiCon) message = Disconnect();
            else message = Connect();
            return message;
        }


        internal string Connect()
        {
            string message;
            //attempt to sync with motor
            //int speed = (int)((double)nmSpiRPM.Value * 2.122);
            //int speed = (int)((double)nmSpiRPM.Value * 3.772);
            //int speed = (int)((double)nmSpiRPM.Value * 3.7022); //Adjusted by BG and CC on 9/7/12
            if (controller.modbus.WriteModbusQueue(1, 2000, 0, true))
            {
                isSpiCon = true;
                message = "Spindle Connected";
                Console.WriteLine(message);
            }
            else {
                isSpiCon = false;
                message = "Spindle Failed to connect";
                Console.WriteLine(message);
                    }
            return message;
        }

        private string Disconnect()
        {
            controller.modbus.WriteModbusQueue(1, 2000, 0, false);
            isSpiCon = false;
            return "Spindle Disconnected";
        }

        public void StopSpi()//Stop the Spindle Motor
        {
            //Stop the spindle
            controller.modbus.WriteModbusQueue(1, 2000, 0, false);
        }

        private bool ChangeDir(bool dir)
        {
            if (dir = SpiDir)
            {
                return false;
            }
            SpiDir = dir;
            return true;
        }

        private bool ChangeRPM(Double RPM)
        {
            //allow for checking of maximum speeds and insure IPM is positive
            double CheckRPM = Math.Abs(RPM);
            if (CheckRPM > RPMmax)
            {
                CheckRPM = RPMmax;
            }
            else
            {
                if (CheckRPM < RPMmin)
                {
                    CheckRPM = RPMmin;
                }

            }
            if (Math.Abs(CheckRPM - SpiRPM) > epsilon)
            {
                SpiRPM = CheckRPM;
                //int speed = (int)(RPM * 3.7022); //Adjusted by BG and CC 9/7/12
                controller.modbus.WriteModbusQueue(1, 2002, (int)(SpiRPM * 3.772), false);
                return true;
            }
            return false;
        }

        private void MoveCCModbus()
        {
            if (isSpiCon)
            {
                controller.modbus.WriteModbusQueue(1, 2000, 1, false);
            }
        }

        private void MoveCCWModbus()
        {
            if (isSpiCon)
            {
                controller.modbus.WriteModbusQueue(1, 2000, 1, false);
            }
        }

        public void MoveModbus(double RPM, bool dir)
        {
            if (ChangeDir(dir) || ChangeRPM(RPM))
            {
                if (SpiDir) { MoveCCModbus(); }
                else MoveCCWModbus();
            }

        }

        //internal void SpindleUDPControl()
        //{
            //if (isSpiSpeedCW)
            //{
            //    StartSpiCW();
            //    isSpiCW = true;
            //}
            //else
            //{
            //    StartSpiCCW();
            //    isSpiCW = false;
            //}
            //SpiSpeed[0] = -99.9;
            //SpiSpeed[1] = SpiSpeed[0];
            //SpiSpeed[0] = BitConverter.ToDouble(RecieveBytes, 8);
            //if (SpiSpeed[0] != SpiSpeed[1])
            //{
            //    isSpiSpeedCW = trueifpositive(SpiSpeed[0]);
            //    SpiSpeedMagnitude = Math.Abs(SpiSpeed[0]);
            //    if (SpiSpeedMagnitude > 100)
            //        isLubWanted = true;
            //    else
            //        isLubWanted = false;
            //    //Limit Spindle speed
            //    if (SpiSpeedMagnitude > SpiSpeedLimit) SpiSpeedMagnitude = SpiSpeedLimit;
            //    if (isSpiSpeedCW)
            //    {
            //        if (!isSpiCW)
            //        {
            //            StartSpiCW();
            //            isSpiCW = true;
            //        }
            //        SpiMessage = "CW";
            //    }
            //    else
            //    {
            //        if (isSpiCW)
            //        {
            //            StartSpiCCW();
            //            isSpiCW = false;
            //        }
            //        SpiMessage = "CCW";
            //    }
            //    ChangeSpiRef(SpiSpeedMagnitude);
            //    WriteMessageQueue("Spi set to:" + SpiSpeedMagnitude.ToString("F0") + SpiMessage);

            //}
        //}
    }
}
