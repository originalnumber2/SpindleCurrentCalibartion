using System;
using System.Threading;

namespace SpindleCurrentCalibartion
{
    public class MotorController
    {
        internal Modbus modbus;
        SpindleMotor spindleMotor;

        internal Mutex commandMutex;

        //motor connection variables
        bool IsSpiCon;
  

        //State Variables of the controller
        double SpiRPM;
        bool SpiDir; //Spindle direction true - Clockwise false - counter clockwise

        //not completely happy with this constructor
        public MotorController()
        {
            //creation of communication protocals
            modbus = new Modbus();

            //creating the spindle motor, it communicatates over Modbus
            spindleMotor = new SpindleMotor(this);

            UpdateParameters();
        }

        void UpdateParameters()
        {
            if (IsSpiCon)
            {
                GetMotorStates();
                GetMotorDirection();
            }
            else Console.WriteLine("connect the motors");
        }

        void GetMotorConnections()
        {
            IsSpiCon = spindleMotor.isSpiCon;
        }

        void GetMotorStates()
        {
            SpiRPM = spindleMotor.SpiRPM;
            SpiDir = spindleMotor.SpiDir;
        }

        void GetMotorDirection()
        {
            SpiDir = spindleMotor.SpiDir;;
        }

        internal void StartSpindle(double RPM, bool dir)
        {
            if (IsSpiCon)
            {
                //need to account for Protocal Scheme and Max Speeds
                spindleMotor.MoveModbus(RPM, dir);
            }
            else
            {
               Console.WriteLine("Lateral motor is not connected");
            }
        }

        internal string ConnSpindle()
        {
            string returnMes = "";
            if (IsSpiCon)
            {
                //Vertical motor is moved in a specified way.
                returnMes = returnMes + "Vertical motor is already connected";
            }
            else
            {
                returnMes = returnMes + spindleMotor.ConnectionToggle();
            }
            return returnMes;
        }

        internal void StopSpindle()
        {
            spindleMotor.StopSpi();
        }

    }
}


