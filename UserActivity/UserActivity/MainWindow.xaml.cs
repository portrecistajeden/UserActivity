using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserActivity.Files;
using UserActivity.Models;

namespace UserActivity
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<RowModel> gridView { get; set; }

        private readonly FileHandler fileHandler;
        private readonly DataAccess db;
        private readonly int _year = 2020;
        private string path;
        private Boolean DB_EMPTY = true;
        private Boolean FILE_LOADED = false;
        public MainWindow()
        {
            InitializeComponent();
            fileHandler = new FileHandler();
            db = new DataAccess();
            gridView = new ObservableCollection<RowModel>();
        }

        //Wywołanie funkcji pracującej na danych po naciśnięciu guzika 'Wykonaj'
        private void ExecuteButtonClick(object sender, RoutedEventArgs s)
        {
            if (path != "canceled" && path != "")
            {
                gridView.Clear();
                List<string> rows = fileHandler.ReadLogFile(path); 
                if (!rows.Any())
                    return;
                List<string[]> parsedData = ParseData(rows);
                
                db.SetDb(OperationOnData(parsedData));
                this.dataGrid.ItemsSource = db.GetDb();
                DB_EMPTY = false;
                ButtonsEnable();
            }
        }

        //Wyczyszczenie tabeli z danymi
        private void ClearButtonClick (object sender, RoutedEventArgs s)
        {
            db.ClearDb();
            this.dataGrid.ItemsSource = null;
            path = "";
            TextBox pathDisplay = this.FindName("pathDisplay") as TextBox;
            pathDisplay.Text = path;

            DB_EMPTY = true;
            FILE_LOADED = false;
            ButtonsEnable();
        }

        //Wczytanie pliku .log
        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            string localFilePath;
            localFilePath = fileHandler.GetLogFilePath();
            if (path != "" && localFilePath == "canceled")
                return;
            path = localFilePath;
            
            TextBox pathDisplay = this.FindName("pathDisplay") as TextBox;
            pathDisplay.Text = path;

            FILE_LOADED = true;
            ButtonsEnable();
        }

        //Zapisanie tabeli do pliku xlsx
        public void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            XLWorkbook workbook = new XLWorkbook();
            DataTable dataTable = MakeDataTable();
            var ws = workbook.Worksheets.Add(dataTable, "User Activity");
            ws.Columns().AdjustToContents();
            fileHandler.SaveXlsxFile(workbook);
        }

        //Przygotowanie DataTable do zapisania tabeli w pliku xlsx
        public DataTable MakeDataTable()
        {
            DataTable table = new DataTable();
            DataColumn column;
            DataRow dataRow;
            //L.p.
            column = new DataColumn
            {
                DataType = System.Type.GetType("System.Int32"),
                ColumnName = "L.p."
            };
            //column.Unique = true;
            table.Columns.Add(column);
            //Data
            column = new DataColumn
            {
                DataType = System.Type.GetType("System.DateTime"),
                ColumnName = "Data"
            };
            table.Columns.Add(column);
            //Login
            column = new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Login"
            };
            table.Columns.Add(column);
            //Czy zalogowany w danym dniu (TAK/NIE)
            column = new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Czy zalogowany w danym dniu (TAK/NIE)"
            };
            table.Columns.Add(column);
            //Łączny czas aktywności w danym dniu
            column = new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Łączny czas aktywności w danym dniu"
            };
            table.Columns.Add(column);

            foreach(RowModel row in db.GetDb())
            {
                dataRow = table.NewRow();
                dataRow["L.p."] = row.lp;
                dataRow["Data"] = row.date;
                dataRow["Login"] = row.login;
                dataRow["Czy zalogowany w danym dniu (TAK/NIE)"] = row.wasLoggedThatDay;
                dataRow["Łączny czas aktywności w danym dniu"] = row.activityTime.ToString();
                table.Rows.Add(dataRow);
            }

            return table;
        }

        //Wyciąganie z linii tekstu z pliku log najważniejszych informacji potrzebnych do operacji i zapisywanie ich w formie Listy tablic string
        //wyrzucenie komunikatów o nieudanym logowaniu
        private List<string[]> ParseData(List<string> strings)
        {
            string[] parsedString;
            string[] row = new string[5];
            List<string[]> result = new List<string[]>();
            foreach (string s in strings)
            {
                row = new string[5];
                parsedString = new string[11];
                parsedString = s.Split(' ');
                //  0-month 1-day 2-time 5-connect/disconnect 7-login
                row[0] = parsedString[0];   //month
                row[1] = parsedString[1];   //day
                row[2] = parsedString[2];   //time
                row[3] = parsedString[5];   //Connect/Disconnect
                row[4] = parsedString[7];   //login
                result.Add(row);
            }
            return result;
        }

        //Blokowanie i odblokowywanie guzików zależnie od tego czy wczytany jest plik i czy wyświetlony został wynik
        private void ButtonsEnable()
        {
            Button executeButton = this.FindName("ExecuteButton") as Button;
            Button clearButton = this.FindName("ClearButton") as Button;
            Button saveButton = this.FindName("SaveButton") as Button;
            if (FILE_LOADED)
                executeButton.IsEnabled = true;
            else
                executeButton.IsEnabled = false;
            if (!DB_EMPTY)
            {
                clearButton.IsEnabled = true;
                saveButton.IsEnabled = true;
            }
            else
            {
                clearButton.IsEnabled = false;
                saveButton.IsEnabled = false;
            }
        }

        //Praca na przygotowanych danych
        private List<RowModel> OperationOnData(List<string[]> data)
        {
            int lp = 1;
            string[] firstRow = data[0];
            string[] time = firstRow[2].Split(':');
            DateTime date = new DateTime(_year, ConvertMonth(firstRow[0]), Int32.Parse(firstRow[1]), Int32.Parse(time[0]), Int32.Parse(time[1]), Int32.Parse(time[2]));

            List<RowModel> result = new List<RowModel>();   //Lista wszytskich użytowników którzy spędzali czas na stronie
            List<string[]> ignore = new List<string[]>();   //Lista osób do wykluczenia podczas operacji w celu uniknięcia powtórzeń
            List<string> users = new List<string>();        //Lista wszystkich użytkowników występujących w pliku .log
            List<DateTime> days = new List<DateTime>();     //Lista wszystkich dni z pliku .log
            List<RowModel> final = new List<RowModel>();    //Finalna lista z gotowymi statystykami

            TimeSpan interval;
            DateTime date1, date2;

            RowModel model;

            foreach (string[] row in data.Where(x => x[3].Equals("Connect") && !ignore.Contains(x)))
            {
                time = new string[3];
                time = row[2].Split(':');
                date1 = new DateTime(_year, ConvertMonth(row[0]), Int32.Parse(row[1]), Int32.Parse(time[0]), Int32.Parse(time[1]), Int32.Parse(time[2]));

                if (!users.Contains(row[4]))
                    users.Add(row[4]);


                foreach (string[] row2 in data.Where(x => x[3].Equals("Disconnect") && x[1] == row[1] && !ignore.Contains(x)))
                {
                    if (row[4].Equals(row2[4]))
                    {
                        time = new string[3];
                        time = row2[2].Split(':');
                        date2 = new DateTime(_year, ConvertMonth(row2[0]), Int32.Parse(row2[1]), Int32.Parse(time[0]), Int32.Parse(time[1]), Int32.Parse(time[2]));
                        interval = date2 - date1;

                        model = result.Where(x => x.login.Equals(row2[4])
                            && x.date.Day == date2.Day
                            && x.date.Month == date2.Month)
                            .FirstOrDefault();

                        if (model != null)
                        {
                            model.activityTime += interval;
                            var index = result.FindIndex(x => x == model);
                            result[index] = model;
                        }
                        else
                        {
                            result.Add(new RowModel(0, new DateTime(_year, date2.Month, date2.Day), row2[4], "TAK", interval));
                        }

                        ignore.Add(row);
                        ignore.Add(row2);
                        break;
                    }
                }
            }
            //pozyskiwanie listy ze wszystkimi dniami z pliku .log
            foreach (string[] row in data)
            {
                date1 = new DateTime(_year, ConvertMonth(row[0]), Int32.Parse(row[1]));
                if (!days.Contains(date1))
                {
                    days.Add(new DateTime(date1.Year, date1.Month, date1.Day));
                }
            }
            //Tworzenie finalnej listy (dni x użytkownicy); na tym etapie nikt nie odwiedzał strony i spędzał tam 0 sekund
            foreach (DateTime d in days)
            {
                foreach (string user in users)
                {
                    final.Add(new RowModel(lp, d, user, "NIE", new TimeSpan(0)));
                    lp++;
                }
            }
            //przyisanie odpowiedniej ilości spędzonego czasu i zmiana wartości pola wasLoggedThatDay na 'TAK'
            RowModel matchPerson;
            foreach (RowModel row in final)
            {
                matchPerson = result.Where(x => x.date == row.date && x.login == row.login).FirstOrDefault();
                if (matchPerson != null)
                {
                    row.wasLoggedThatDay = "TAK";
                    row.activityTime = matchPerson.activityTime;
                }
            }

            return final;
        }
        //Dekodowanie miesięcy przy parsowaniu pliku .log
        private int ConvertMonth(string month)
        {
            switch (month)
            {
                case "Jan":
                    return 1;
                case "Feb":
                    return 2;
                case "Mar":
                    return 3;
                case "Apr":
                    return 4;
                case "May":
                    return 5;
                case "Jun":
                    return 6;
                case "Jul":
                    return 7;
                case "Aug":
                    return 8;
                case "Sep":
                    return 9;
                case "Oct":
                    return 10;
                case "Nov":
                    return 11;
                case "Dec":
                    return 12;
            }

            return 0;
        }
    }
}
