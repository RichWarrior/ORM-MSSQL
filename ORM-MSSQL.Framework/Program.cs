using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_MSSQL.Framework
{
    class Program
    {
        static void Main(string[] args)
        {
            ORM orm = new ORM("Data Source=192.168.2.162;Initial Catalog=MSSQLFramework;User Id=sa;PWD=03102593Faruk");            
            Console.ReadKey();
        }
    }
}
