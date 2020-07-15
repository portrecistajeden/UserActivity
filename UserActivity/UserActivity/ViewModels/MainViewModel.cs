using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserActivity.Logic;
using UserActivity.Models;

namespace UserActivity.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<RowModel> gridView { get; set; }

        FileHandler fileHandler;
        DataAccess db;
        int _year = 2020;
        public MainViewModel()
        {
            fileHandler = new FileHandler();
            db = new DataAccess();
            gridView = new ObservableCollection<RowModel>();
        }
        public void LoadButtonClick()
        {
            string path = fileHandler.getLogFilePath();
            if(path != "canceled")
            {
                gridView.Clear();
                List<string> rows = fileHandler.readLogFile(path);
                List<string[]> parsedData = ParseData(rows);
                db.SetDb(DoEverything(parsedData));
                foreach(RowModel r in db.GetDb())
                {
                    gridView.Add(r);
                }
            }
        }

        private List<string[]> ParseData(List<string> strings)
        {
            string[] parsedString;
            string[] row = new string[5];
            List<string[]> result = new List<string[]>();
            foreach(string s in strings)
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
        
        private List<RowModel> DoEverything(List<string[]> data)
        {           
            string[] firstRow = data[0];
            string[] time = firstRow[2].Split(':');
            DateTime date = new DateTime(_year, convertMonth(firstRow[0]), Int32.Parse(firstRow[1]), Int32.Parse(time[0]), Int32.Parse(time[1]), Int32.Parse(time[2]));

            List<RowModel> result = new List<RowModel>();
            List<string[]> ignore = new List<string[]>();
            TimeSpan interval;
            DateTime date1, date2;

            RowModel model;

            foreach (string[] row in data.Where(x => x[3].Equals("Connect") && !ignore.Contains(x)))
            {
                time = new string[3];
                time = row[2].Split(':');
                date1 = new DateTime(_year, convertMonth(row[0]), Int32.Parse(row[1]), Int32.Parse(time[0]), Int32.Parse(time[1]), Int32.Parse(time[2]));
                foreach(string[] row2 in data.Where(x => x[3].Equals("Disconnect") && x[1]==row[1] && !ignore.Contains(x)))
                {
                    if(row[4].Equals(row2[4]))
                    {
                        time = new string[3];
                        time = row2[2].Split(':');
                        date2 = new DateTime(_year, convertMonth(row2[0]), Int32.Parse(row2[1]), Int32.Parse(time[0]), Int32.Parse(time[1]), Int32.Parse(time[2]));
                        interval = date2 - date1;

                        model = result.Where(x => x.login.Equals(row2[4]) 
                            && x.date.Day == date2.Day 
                            && x.date.Month == date2.Month)
                            .FirstOrDefault();

                        //Console.WriteLine(date1 + " " + row2[4] + " " + interval);
                        if(model != null)
                        {
                            model.activityTime += interval;
                            var index = result.FindIndex(x => x == model);
                            result[index] = model;
                        }
                        else
                            result.Add(new RowModel(new DateTime(_year, date2.Month, date2.Day), row2[4], true, interval));

                        ignore.Add(row);
                        ignore.Add(row2);
                        break;
                    }
                }
            }
            
            return result;
        }
        private int convertMonth(string month)
        {
            switch(month)
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
