using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserActivity.Models;

namespace UserActivity
{
    class DataAccess
    {
        private List<RowModel> db = new List<RowModel>();

        public List<RowModel> GetDb ()
        {
            return db;
        }
        public void SetDb(List<RowModel> list)
        {
            db = list;
        }
    }
}
