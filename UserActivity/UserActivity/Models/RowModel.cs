using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserActivity.Models
{
    public class RowModel
    {
        public DateTime date { get; set; }
        public string login { get; set; }
        public Boolean wasLoggedThatDay { get; set; }
        public TimeSpan activityTime { get; set; }

        public RowModel (DateTime date, string login, Boolean wasLoggedThatDay, TimeSpan activityTime)
        {
            this.date = date;
            this.login = login;
            this.wasLoggedThatDay = wasLoggedThatDay;
            this.activityTime = activityTime;
        }
    }
}
