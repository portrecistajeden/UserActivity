using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserActivity.Files
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
        public void SaveXlsxFile (XLWorkbook workbook)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Pliki Excel (.xlsx)|*.xlsx";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                workbook.SaveAs(dlg.FileName);
            }
        }
    }
}
