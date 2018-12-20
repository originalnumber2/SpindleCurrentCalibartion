using System;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Diagnostics;

namespace SpindleCurrentCalibartion
{
    public class Modbus
    // The Modbus controls the motors via the GUI
    {
        Semaphore ModBusQueueSemaphore = new Semaphore(0, 50);
        Queue ModBusQueue = new Queue();
        SerialPort ModbusPort;
        Semaphore ModBusQueueMutex = new Semaphore(1, 1);//I know, it is not a mutex, but had some weird issues using mutex across multiple threads so this is my solution to use a sempafore as a mutex
        Thread ModBusWriteThread;


        public Modbus()
        {
            try
            {
                ModbusPort = new SerialPort
                {
                    BaudRate = 38400,
                    PortName = "COM3",
                    Parity = System.IO.Ports.Parity.None,
                    StopBits = System.IO.Ports.StopBits.Two
                };
                ModbusPort.Open();
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("Open device manager in windows and the 'Setup Serial Ports' section of the C# code and check the serial port names and settings are correct\n\n" + e.ToString(), "Serial Port Error");
                Process.GetCurrentProcess().Kill();

            }
            catch (System.UnauthorizedAccessException e)
            {
                Console.WriteLine("Something is wrong? maybe try to restart computer?\n\nHere is some error message stuff...\n\n" + e.ToString(), "Serial Port Error");
                Process.GetCurrentProcess().Kill();
            }

            //Start the Modbus Writer thread
            ModBusWriteThread = new Thread(new ThreadStart(WriteModbus));
            ModBusWriteThread.Start();

        }

        public void WriteModbus()
        {
            byte[] RSMessage = { 0, 0, 0, 0, 0, 0, 0, 0 };
            while (true)
            {
                ModBusQueueSemaphore.WaitOne();//wait on something to be put into motor comand queue
                ModBusQueueMutex.WaitOne();
                RSMessage = (byte[])ModBusQueue.Dequeue();

                try { ModBusQueueMutex.Release(); }
                catch (System.Threading.SemaphoreFullException ex)
                {
                    Console.WriteLine("error releasing ModBusQueueMutex 3 " + ex.Message.ToString());
                }
                ModbusPort.Write(RSMessage, 0, 8);

                Thread.Sleep(25);//wait for a period of at least 25ms to send comands to motors on the modbus,
                                 // commands seem to be dropped if sent any faster. J 9/10/2015
            }
        }

        internal bool WriteModbusQueue(int motor, int address, int data, bool checkreturn)
        {
            //Info for how this talks to the motor diver can be seen in Table 5-1 Communication Mapping Table on the mvx9000 data sheet for the lateral and traverse motor drivers
            int motorreturned = 0;
            //For write modbus
            byte[] Returned = { 0, 0, 0, 0, 0, 0, 0, 0 };//delete
            byte[] RSMessage = { 0, 0, 0, 0, 0, 0, 0, 0 };

            //some local declarations
            ulong chkval;

            //(address >> 8) this is a bit shift right by 8 bits, used to grab bits 8-15 of the int
            //(data & 0xFF) bit and operation that preserves only the lower 8 bits, bits 0-7 of the int

            //Slave ID and function code
            //This byte is the motor address can be found on the menu 90.02 on the mvx9000
            //byte is also motor adress for the svx9000
            RSMessage[0] = (byte)motor;
            //This is the length of the message for mvx9000 
            //This is the function for the svx9000  0x06 is for write single register
            RSMessage[1] = (byte)0x06;

            //Address
            RSMessage[2] = (byte)(address >> 8);//see table 5-1 mvx9000
            RSMessage[3] = (byte)(address & 0xFF);

            //Data
            RSMessage[4] = (byte)(data >> 8);
            RSMessage[5] = (byte)(data & 0xFF);

            //get crc info
            chkval = crc_chk(RSMessage, 6);

            RSMessage[6] = (byte)(chkval & 0xFF);
            RSMessage[7] = (byte)(chkval >> 8);

            ////Transmit
            //ModbusPort.Write(RSMessage, 0, 8); now will send to queue and another thread will transmit commands 9/10/2015

            if (!checkreturn)
            {
                //Send to Queue
                ModBusQueueMutex.WaitOne();
                ModBusQueue.Enqueue(RSMessage);
                try { ModBusQueueMutex.Release(); }
                catch (System.Threading.SemaphoreFullException ex)
                {
                    Console.WriteLine("error releasing ModBusQueueMutex 4 " + ex.Message.ToString());
                }

                //tell other thread message is available
                try { ModBusQueueSemaphore.Release(1); }
                catch (System.Threading.SemaphoreFullException ex)
                {
                    Console.WriteLine("error releasing ModBusQueueSemaphore 3 " + ex.Message.ToString());
                }
            }

            if (checkreturn)
            {
                ModBusQueueMutex.WaitOne();
                //if the calling functions wants a check on whether the motor resonds
                ///must clear out and info in port
                ModbusPort.ReadExisting();
                ModbusPort.Write(RSMessage, 0, 8);
                for (int i = 0; i < 3; i++)
                {
                    if (ModbusPort.BytesToRead > 0)
                    {
                        //Check to see if the requested motor that responded matches the right motor
                        ModbusPort.Read(Returned, 0, ModbusPort.BytesToRead);
                        Console.WriteLine(BitConverter.ToString(Returned));
                        motorreturned = Returned[0];
                        if (motor == motorreturned)
                        {
                            try { ModBusQueueMutex.Release(); }
                            catch (System.Threading.SemaphoreFullException ex)
                            {
                                Console.WriteLine("error releasing ModBusQueueMutex 5 " + ex.Message.ToString());
                            }
                            return true;
                        }
                    }
                    Thread.Sleep(20);
                }
                try { ModBusQueueMutex.Release(); }
                catch (System.Threading.SemaphoreFullException ex)
                {
                    Console.WriteLine("error releasing ModBusQueueMutex 6 " + ex.Message.ToString());
                }
                return false;
            }
            return true;
        }

        ulong crc_chk(byte[] data, int length)
        {
            int j;
            int pos = 0;
            ulong reg_crc;
            reg_crc = 0xFFFF;
            while (length != 0)
            {
                length--;
                reg_crc ^= data[pos];
                pos++;
                for (j = 0; j < 8; j++)
                {
                    if ((reg_crc & 0x01) != 0)
                    {
                        reg_crc = (reg_crc >> 1) ^ 0xA001;
                    }
                    else
                        reg_crc = reg_crc >> 1;
                }
            }
            return reg_crc;
        }


    }
}
