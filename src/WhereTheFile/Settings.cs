using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WhereTheFile
{
    public class Settings
    {
        public static string BaseAppDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhereTheFile");
        public static string DatabasePath = Path.Join(BaseAppDataPath, "WTF_EF.db");
        public Settings()
        {
            Console.WriteLine($"Config path is {BaseAppDataPath}");
            if (!Directory.Exists(BaseAppDataPath))
            {
                Directory.CreateDirectory(BaseAppDataPath);
            }

            IConfiguration config = new ConfigurationBuilder().SetBasePath(BaseAppDataPath)
                .AddJsonFile("appsettings.json", true, true)
                .Build();
        }


    }
}
