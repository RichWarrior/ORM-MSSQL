using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_MSSQL.Framework
{
    public class Students
    {
        public int? id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
