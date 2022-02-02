using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VarjoCompanion
{
    class MainApp
    {
        static void Main(string[] args)
        {
            IntPtr session = VarjoEyeTracking.Init();
            
            if (!VarjoEyeTracking.IsGazeAllowed())
            {
                Console.WriteLine("Gaze tracking is not allowed! Please enable it in the Varjo Base!");
                return;
            }

            VarjoEyeTracking.GazeInit();
            VarjoEyeTracking.SyncProperties();

            VarjoEyeTracking.GazeData gazeData = new VarjoEyeTracking.GazeData();

            using (var memMapFile = MemoryMappedFile.CreateNew("VarjoEyeTracking", Marshal.SizeOf(gazeData)))
            {
                using (var accessor = memMapFile.CreateViewAccessor())
                {
                    Console.WriteLine("Eye tracking session has started!");
                    while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter))
                    {
                        gazeData = VarjoEyeTracking.GetGaze();
                        accessor.Write(0, ref gazeData);
                    }
                }
            }

            VarjoEyeTracking.varjo_SessionShutDown(session);
        }
    }
}
