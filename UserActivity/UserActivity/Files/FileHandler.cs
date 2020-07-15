using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserActivity.Logic
{
    public class FileHandler
    {
        public string getLogFilePath()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "";
            dlg.DefaultExt = ".log";
            dlg.Filter = "Pliki log (.log)|*.log";

            Nullable<bool> result = dlg.ShowDialog();
            
            if (result == true)
            {
                return dlg.FileName;
            }
            return "canceled";            
        }

        public List<string> readLogFile(string path)
        {
            var allLines = File.ReadLines(path);

            var result = allLines.Where(x => x.Contains("Connect") || x.Contains("Disconnect")).ToList();
            
            return result;
        }
    }
}
