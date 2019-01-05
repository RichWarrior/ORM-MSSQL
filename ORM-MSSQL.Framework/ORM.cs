using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_MSSQL.Framework
{
    public class ORM : IORM
    {
        private string _connectionString { get; set; }
        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataReader reader;
        private static readonly ILog Log =
              LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ORM(string connectionString)
        {
            _connectionString = connectionString;
            if (con == null)
                con = new SqlConnection(_connectionString);
        }

        public async Task<List<T>> Select<T>()
        {
            var result = new List<T>();
            try
            {
                var query = String.Format("SELECT * FROM {0}", typeof(T).Name);
                cmd = new SqlCommand(query, con);
                await con.OpenAsync();
                reader = await cmd.ExecuteReaderAsync();
                var _instance = (T)Activator.CreateInstance(typeof(T));
                var _properties = _instance.GetType().GetProperties();
                while (await reader.ReadAsync())
                {
                    var a = (T)Activator.CreateInstance(typeof(T));
                    foreach (var item in _properties)
                    {
                        item.SetValue(a, reader[item.Name] == DBNull.Value ? null : reader[item.Name], null);
                    }
                    result.Add(a);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
                if (reader != null)
                    reader.Close();
            }
            return result;
        }

        public async Task<bool> Insert(object value)
        {
            var result = false;
            try
            {
                var query = String.Format("INSERT INTO {0} (", value.GetType().Name);
                var subQuery = "(";
                var _properties = value.GetType().GetProperties();
                foreach (var item in _properties)
                {
                    query += item.Name + ",";
                    if (item.GetValue(value, null) == null)
                    {
                        query = query.Replace(item.Name + ",", "");
                    }
                    else
                    {
                        subQuery += String.Format("@{0},", item.Name);
                    }

                }
                query = query.TrimEnd(',') + ")";
                subQuery = subQuery.TrimEnd(',') + ")";
                query = query + " VALUES " + subQuery;
                await con.OpenAsync();
                cmd = new SqlCommand(query, con);
                foreach (var item in _properties)
                {
                    if (item.GetValue(value, null) != null)
                    {
                        cmd.Parameters.AddWithValue("@" + item.Name, item.GetValue(value, null));
                    }
                }

                await cmd.ExecuteNonQueryAsync();
                result = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
            return result;
        }

        public async Task<List<T>> Select<T>(string query, object args)
        {
            var result = new List<T>();
            try
            {
                cmd = new SqlCommand(query, con);
                var _properties = args.GetType().GetProperties();
                var _instance = (T)Activator.CreateInstance(typeof(T));
                var _props = _instance.GetType().GetProperties();
                foreach (var item in _properties)
                {
                    if (query.Contains("@" + item.Name))
                    {
                        cmd.Parameters.AddWithValue("@" + item.Name, item.GetValue(args, null));
                    }
                }
                await con.OpenAsync();
                reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var a = (T)Activator.CreateInstance(typeof(T));
                    foreach (var item in _props)
                    {
                        if (query.Contains(item.Name))
                        {
                            item.SetValue(a, reader[item.Name] == DBNull.Value ? null : reader[item.Name]);
                        }
                        if (query.Contains("*"))
                        {
                            item.SetValue(a, reader[item.Name] == DBNull.Value ? null : reader[item.Name]);
                        }
                    }
                    result.Add(a);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
                if (reader != null)
                    reader.Close();
            }
            return result;
        }

        public async Task<List<T>> Select<T>(string query)
        {
            return await Task.Run(() => this.Select<T>(query, new { }));
        }

        public async Task<bool> BulkInsert<T>(List<T> value)
        {
            var result = true;
            try
            {
                var _instance = (T)Activator.CreateInstance(typeof(T));
                var _props = _instance.GetType().GetProperties();
                var query = String.Format("INSERT INTO {0} (", _instance.GetType().Name);
                foreach (var item in _props)
                {
                    query += String.Format("{0},", item.Name);
                }
                query = query.TrimEnd(',') + ") VALUES (";             
                var count = 0;
                foreach (var item in value)
                {
                    var _itemArgs = item.GetType().GetProperties();
                    count++;
                    foreach (var args in _itemArgs)
                    {
                        query += String.Format("@{0}{1},", args.Name, count);
                    }
                    query = query.TrimEnd(',') + "),(";
                }
                query = query.TrimEnd('(');
                query = query.TrimEnd(',');
                count = 0;
                await con.OpenAsync();
                cmd = new SqlCommand(query, con);
                foreach (var item in value)
                {
                    var _itemArgs = item.GetType().GetProperties();
                    count++;
                    foreach (var args in _itemArgs)
                    {
                        var a = args.GetValue(item,null);
                        cmd.Parameters.AddWithValue(String.Format("@{0}{1}", args.Name, count), args.GetValue(item, null));
                    }
                }
                var rowsCount = await cmd.ExecuteNonQueryAsync();
                if (rowsCount > 1)
                    result = true;

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
                if (reader != null)
                    reader.Close();
            }
            return result;
        }

        public async Task<List<T>> StoredProcedure<T>(object value)
        {
            var result = new List<T>();
            var sql = String.Format("{0} ",value.GetType().Name);
            try
            {
                var _props = value.GetType().GetProperties();
                var args = ((T)Activator.CreateInstance(typeof(T))).GetType().GetProperties();
                //foreach (var item in _props)
                //{
                //    sql += String.Format("@{0}", item.Name);
                //}
                await con.OpenAsync();
                cmd = new SqlCommand(sql,con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                foreach (var item in _props)
                {
                    cmd.Parameters.Add(new SqlParameter(item.Name,item.GetValue(value,null)));
                }
                reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var a = (T)Activator.CreateInstance(typeof(T));                    
                    foreach (var item in args)
                    {
                        item.SetValue(a, reader[item.Name] == DBNull.Value ? null : reader[item.Name], null);
                    }
                    result.Add(a);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
                if (reader != null)
                    reader.Close();
            }            
            return result;
        }

        public async Task<bool> RunQuery(string query)
        {
            return await Task.Run(() => this.RunQuery(query, new { }));
        }

        public async Task<bool> RunQuery(string query, object args)
        {
            var result = false;
            try
            {
                await con.OpenAsync();
                cmd = new SqlCommand(query,con);
                var _props = args.GetType().GetProperties();
                foreach (var item in _props)
                {
                    if (query.Contains(String.Format("@{0}", item.Name)))
                    {
                        cmd.Parameters.AddWithValue(String.Format("@{0}", item.Name), item.GetValue(args, null));
                    }
                }
                await cmd.ExecuteNonQueryAsync();
                result = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);                
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
            return result;
        }
    }
}
