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
            ORM orm = new ORM("Data Source=LAB2OGRETMENPC,57893;Initial Catalog=furkan;User Id=test;PWD=03102593");
            var result = orm.Select<Students>("SELECT Name,Surname FROM Students WHERE Name=@t", new { t="Helin"}).Result;
            foreach (var item in result)
            {
                Console.WriteLine(item.Name+" "+item.Surname);
            }
            Console.ReadKey();
        }
    }
}
