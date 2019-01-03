using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_MSSQL.Framework
{
    public interface IORM
    {
        Task<List<T>> Select<T>();
        Task<bool> Insert(object value);
        Task<List<T>> Select<T>(string query, object args);
        Task<List<T>> Select<T>(string query);
    }
}
