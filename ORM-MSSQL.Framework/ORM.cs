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
                var query = String.Format("SELECT * FROM {0}",typeof(T).Name);
                cmd = new SqlCommand(query,con);
                await con.OpenAsync();
                reader = await cmd.ExecuteReaderAsync();
                var _instance = (T)Activator.CreateInstance(typeof(T));
                var _properties = _instance.GetType().GetProperties();
                while(await reader.ReadAsync())
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
                Console.WriteLine(ex.Message);
            }finally
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
                var query =String.Format("INSERT INTO {0} (",value.GetType().Name);
                var subQuery = "(";
                var _properties = value.GetType().GetProperties();
                foreach (var item in _properties)
                {
                    query += item.Name+",";
                    if (item.GetValue(value, null) == null)
                    {
                        query = query.Replace(item.Name + ",", "");
                    }else
                    {
                        subQuery += String.Format("@{0},", item.Name);
                    }

                }
                query = query.TrimEnd(',')+")";
                subQuery = subQuery.TrimEnd(',') + ")";
                query = query +" VALUES "+ subQuery;
                await con.OpenAsync();
                cmd = new SqlCommand(query,con);
                foreach (var item in _properties)
                {
                    if(item.GetValue(value,null)!=null)
                    {
                        cmd.Parameters.AddWithValue("@" + item.Name, item.GetValue(value, null));
                    }                    
                }             
              
                await cmd.ExecuteNonQueryAsync();
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                cmd = new SqlCommand(query,con);
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
                    }
                    result.Add(a);
                }
            }
            catch (Exception)
            {
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
            return await Task.Run(() => this.Select<T>(query,new { }));           
        }
    }
}
