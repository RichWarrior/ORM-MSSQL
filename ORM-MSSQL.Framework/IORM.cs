using System.Collections.Generic;
using System.Threading.Tasks;

namespace ORM_MSSQL.Framework
{
    public interface IORM
    {
        Task<List<T>> Select<T>();
        Task<List<T>> Select<T>(string query);
        Task<List<T>> Select<T>(string query, object args);
        Task<bool> Insert(object value);
        Task<bool> BulkInsert(List<object> value);
        Task<bool> RunQuery(string query);
        Task<bool> RunQuery(string query, object args);        
        Task<List<T>> StoredProcedure<T>(object value);        
    }
}
