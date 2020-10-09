using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WhereTheFile
{
    [Obsolete("TODO: Move to appsettings.json completely?")]
    public class Settings
    {
        public static string BaseAppDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhereTheFile");
        public static string DatabasePath = Path.Join(BaseAppDataPath, "WTF_EF.db");
    }
}
