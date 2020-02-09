using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DriveInfo = WhereTheFile.Types.DriveInfo;

namespace WhereTheFile
{
    public static class DriveGuids
    {
        public static List<DriveInfo> GetOrGenerateDriveGuids()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetOrGenerateDriveGuidsLinux();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetOrGenerateDriveGuidsWindows();
            }

            else
            {
                throw new NotImplementedException();
            }
        }

        internal static List<DriveInfo> GetOrGenerateDriveGuidsLinux()
        {
            throw new NotImplementedException();
        }

        internal static List<DriveInfo> GetOrGenerateDriveGuidsWindows()
        {
            //This is far easier than trying to get the drive serial number, but I guess I'll have to do that eventually
            List<DriveInfo> scannedDrives = new List<DriveInfo>();

            foreach (string drive in System.IO.Directory.GetLogicalDrives())
            {
                string guid = String.Empty;
                string path = Path.Join(drive, ".wtf");
                if (File.Exists(path))
                {
                    guid = File.ReadAllText(path);
                }

                else
                {
                    guid = Guid.NewGuid().ToString();
                    File.WriteAllText(path, guid);
                }
                scannedDrives.Add(new DriveInfo() { CurrentDriveLetter = drive, GeneratedGuid = guid, HasBeenScanned = false });
            }

            return scannedDrives;
        }


        i



    }
}
