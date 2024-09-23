
using Microsoft.Data.SqlClient.DataClassification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozetka
{
    internal class TableInfo
    {
        public String Name { get; set; }
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
    }
}
