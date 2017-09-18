using System;
using System.Linq;

namespace XSCal_1D_SDK
{
    class MainClass
    {
        static void Main(string[] args)
        /* 
         * A C# console application must contain a Main method, in which control starts and ends. 
         * The Main method is where you create objects and execute other methods. 
        /*
         * The parameter of the Main method is a String array that represents the command-line arguments.
         * For example, XSCal_1D_SDK.exe read_xs_ini, the main method would be passed a parameter 'read_xs_ini'.
         */
        {
            if (args.Length < 1)
            {
                Helpers.showTips();
                Console.ReadKey();
                return;// exit the console application's running thread.
            }
            //Console.WriteLine(args.Length.ToString());
            //Console.WriteLine("Cross section calculation is going to start!");
            //Console.WriteLine("Press any key to continue!");
            //Console.ReadKey();
            if (args[0] == "read_xs_ini") //read initial cross section data
            {
                XSCal_1D_SDK.DotMatIO.read_ini();
            }
            else if (args[0] == "update_all")
            {
                //parsing evalCount
                int evalCount;
                /* 
                 * Converts the string representation of a number in a specified style and culture-specific
                 * format to its 32-bit signed integer equivalent. A return value indicates whether
                 * the conversion succeeded.
                 */
                bool success = int.TryParse(args[1], out evalCount);   
                if (success)
                {
                    //parsing runId
                    int runId;
                    bool success2 = int.TryParse(args[2], out runId);
                    if (success2)
                    {
                        XSCal_1D_SDK.DotMatIO.write_update_all(evalCount, runId); //run
                    }
                    else
                    {
                        Console.WriteLine("XSCal_1D_SDK.exe: Missing/wrong arguments for update_all!");
                        Console.ReadLine();
                    }
                }
                else
                {
                    Console.WriteLine("XSCal_1D_SDK.exe: Missing/wrong arguments for update_all!");
                    Console.ReadLine();
                }
            }
        }
    }
}