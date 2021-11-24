using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VarjoCompanion
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryEye
    {
        public bool opened;
        public double pupilSize;
        public double x;
        public double y;

        private void CopyFrom(VarjoEyeTracking.GazeRay ray, double pupil)
        {
            VarjoEyeTracking.Vector forward = ray.forward;
            x = forward.x;
            y = forward.y;
            pupilSize = pupil;
        }

        public void CopyFrom(VarjoEyeTracking.GazeRay ray, double pupil, VarjoEyeTracking.GazeEyeStatus status)
        {
            this.CopyFrom(ray, pupil);
            opened = status == VarjoEyeTracking.GazeEyeStatus.Compensated || status == VarjoEyeTracking.GazeEyeStatus.Tracked;
        }

        public void CopyFrom(VarjoEyeTracking.GazeRay ray, double pupil, VarjoEyeTracking.GazeStatus status)
        {
            this.CopyFrom(ray, pupil);
            opened = status != VarjoEyeTracking.GazeStatus.Invalid;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryData
    {
        public bool shutdown;
        public bool calibrated;
        public MemoryEye leftEye;
        public MemoryEye rightEye;
        public MemoryEye combined;

        public void CopyFrom(VarjoEyeTracking.GazeData data)
        {
            calibrated = data.status != VarjoEyeTracking.GazeStatus.Invalid;
            leftEye.CopyFrom(data.leftEye, data.leftPupilSize, data.leftStatus);
            rightEye.CopyFrom(data.rightEye, data.rightPupilSize, data.rightStatus);
            combined.CopyFrom(data.gaze, (data.leftPupilSize + data.rightPupilSize) / 2, data.status);
        }
    }

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

            MemoryData memoryData = new MemoryData();

            using (var memMapFile = MemoryMappedFile.CreateNew("VarjoEyeTracking", Marshal.SizeOf(memoryData)))
            {
                using (var accessor = memMapFile.CreateViewAccessor())
                {
                    VarjoEyeTracking.GazeData gazeData;
                    Console.WriteLine("Eye tracking session has started!");
                    while (true)
                    {
                        accessor.Read(0, out memoryData);
                        if (memoryData.shutdown) break; // It's time to shut down
                        gazeData = VarjoEyeTracking.GetGaze();
                        memoryData.CopyFrom(gazeData);
                        accessor.Write(0, ref memoryData);
                    }
                }
            }

            VarjoEyeTracking.varjo_SessionShutDown(session);
        }
    }
}
