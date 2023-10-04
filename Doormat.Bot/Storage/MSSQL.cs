using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using Microsoft.Data.SqlClient;
using static DoormatCore.Sites.BaseSite;

namespace DoormatCore.Storage
{
    internal class MSSQL : SQLBase
    {
        
        SqlConnection Connection = new SqlConnection();

        public MSSQL(string ConnectionString) : base(ConnectionString)
        {
            Logger.DumpLog("Create MSSQL Connection", 6);
            Connection = new SqlConnection(ConnectionString);
            Connection.Open();
        }

       
        string GetDBType(string TypeName)
        {
            string DBName = "";
            switch (TypeName.ToLower())
            {
                case "decimal":DBName = "decimal(35,20)";break;
                case "double": DBName = "decimal(35,20)"; break;
                case "int": DBName = "int"; break;
                case "long": DBName = "bigint"; break;
                case "string": DBName = "nvarchar(500)"; break;
                case "byte": DBName = ""; break;
                case "datetime": DBName = "decimal(35, 20)";break;
                case "boolean": DBName = "bit"; break;
                default: DBName = "int"; break;
            }
            return DBName;
        }

        protected override void CreateTable(Type type)
        {
            Logger.DumpLog($"Checking MSSQL Table for {type.Name}", 6);
            string TableName = type.GetCustomAttribute<PersistentTableName>().TableName;

            SqlCommand CheckTableExists = new SqlCommand("select [COLUMN_NAME] from [INFORMATION_SCHEMA].[COLUMNS] where [TABLE_SCHEMA]='dbo' and [TABLE_NAME]='"+ TableName + "'", Connection);
            bool TableExists = false;
            List<string> Columns = new List<string>();
            using (SqlDataReader Reader = CheckTableExists.ExecuteReader())
            {
                if (Reader.HasRows)
                {
                    TableExists = true;
                    
                    while (Reader.Read())
                    {
                        Columns.Add(Reader[0].ToString());
                    }
                }
            }
            if (TableExists)
            {
                foreach (PropertyInfo PI in type.GetProperties())
                {
                    if (!PI.PropertyType.IsArray && !Attribute.IsDefined(PI,typeof(NonPersistent)))
                    {
                        bool found = false;
                        foreach (string x in Columns)
                        {
                            if (PI.Name.ToLower() == x.ToLower())
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            string Query = "alter table [" + TableName + "] add [" + PI.Name + "] " + GetDBType(PI.PropertyType.Name);
                            SqlCommand AddColumn = new SqlCommand(Query, Connection);
                            AddColumn.ExecuteNonQuery();
                        }
                    }
                }
            }
            else          
            {
                string query = "Create table dbo.["+ TableName + "] (id int identity(1,1) primary key";
                foreach (PropertyInfo PI in type.GetProperties())
                {
                    if (!PI.PropertyType.IsArray && !Attribute.IsDefined(PI, typeof(NonPersistent)))
                    {
                        if (PI.Name.ToLower() != "id")
                        {
                            query += ", "+ PI.Name + " " + GetDBType(PI.PropertyType.Name);
                        }
                    }
                }
                query += ")";
                SqlCommand CreateTable = new SqlCommand(query, Connection);
                CreateTable.ExecuteNonQuery();
            }
        }
        
        private string ConstructSelect(Type type)
        {           
            string TableName = type.GetCustomAttribute<PersistentTableName>().TableName;

            string query = "SELECT ";
            bool first = true;
            foreach (PropertyInfo PI in type.GetProperties())
            {
                if (!PI.PropertyType.IsArray && !Attribute.IsDefined(PI, typeof(NonPersistent)))
                {
                    if (!first)
                    {
                        query += ", ";
                    }
                    first = false;
                    query += "["+TableName+"].["+ PI.Name + "]";                       
                }
            }
            query += " FROM " + "[" + TableName + "] ";
            return query;
        }

        protected override T PerformInsert<T>(T ValueToInsert) //where T:PersistentBase
        {
            Type type = ValueToInsert.GetType();

            string TableName = type.GetCustomAttribute<PersistentTableName>().TableName;
            string query = "INSERT INTO [" + TableName + "](";
            string values = " OUTPUT INSERTED.ID VALUES(";
            bool first = true;
              
            int i = 1;
            SqlCommand tmpCommand = new SqlCommand();

            foreach (PropertyInfo PI in type.GetProperties())
            {
                if (!PI.PropertyType.IsArray && PI.Name.ToLower()!= "id" && !Attribute.IsDefined(PI, typeof(NonPersistent)))
                {
                    if (!first)
                    {
                        query += ", ";
                        values += ",";
                    }
                    first = false;
                    query += "[" + PI.Name + "]";
                    values += "@"+i.ToString();
                    object val = (PI.GetValue(ValueToInsert));
                    /*if (val is decimal)
                        tmpCommand.Parameters.AddWithValue("@"+i++.ToString(), ((decimal)val).ToString("#############00.0000000000#####"));
                    else*/
                        tmpCommand.Parameters.AddWithValue("@" + i++.ToString(), val??DBNull.Value);
                }
            }
            query += ") ";
            values += ")";
            tmpCommand.CommandText = query + values;
            tmpCommand.Connection = Connection;
            ValueToInsert.Id = (int)tmpCommand.ExecuteScalar();
            return ValueToInsert;
        }

        protected override T PerformUpdate<T>(T ValueToUpdate) //where T : PersistentBase
        {
            if (ValueToUpdate.Id <= 0)
                return PerformInsert<T>(ValueToUpdate);
            Type type = ValueToUpdate.GetType();
            string TableName = type.GetCustomAttribute<PersistentTableName>().TableName;

            string query = "UPDATE [" + TableName + "] set";            
            bool first = true;
            
            int i = 1;
            SqlCommand tmpCommand = new SqlCommand();

            foreach (PropertyInfo PI in type.GetProperties())
            {
                if (!PI.PropertyType.IsArray && PI.Name.ToLower() != "id" && !Attribute.IsDefined(PI, typeof(NonPersistent)))
                {
                    if (!first)
                    {
                        query += ", ";
                    }
                    first = false;
                    query += "[" + PI.Name + "] = @" + i.ToString();
                    
                    tmpCommand.Parameters.AddWithValue("@" + i++.ToString(), PI.GetValue(ValueToUpdate)??DBNull.Value);
                }
            }
            query += " WHERE [" + TableName + "].Id = @"+i.ToString();
            tmpCommand.Parameters.AddWithValue("@" + i++.ToString(), ValueToUpdate.Id);
            tmpCommand.CommandText = query ;
            tmpCommand.Connection = Connection;
            ValueToUpdate.Id = (int)tmpCommand.ExecuteScalar();

            return ValueToUpdate;
        }

        protected override T[] PerformFind<T>(string Criteria, string Sorting="", params object[] SqlParams) //where T : PersistentBase, new()
        {
            Logger.DumpLog("Performing Find in MSSQL", 5);
            Logger.DumpLog($"Finding {typeof(T).Name} in MSSQL with Criteria {Criteria}", 6);
            string Select = ConstructSelect(typeof(T));
            
            if (!string.IsNullOrWhiteSpace(Criteria))
                Select += " WHERE " + Criteria;
            if (!string.IsNullOrWhiteSpace(Sorting))
                Select += "ORDER BY " + Sorting;
            Logger.DumpLog($"Select Query: {Select}", 6);
            List<T> results = new List<T>();
            SqlCommand SelectCommand = new SqlCommand(Select, Connection);
            Logger.DumpLog($"Created Command, adding params", 6);
            for (int i=0;i<SqlParams.Length;i++)
            {
                SelectCommand.Parameters.AddWithValue("@"+(i+1),SqlParams[i] ?? DBNull.Value);
            }
            Logger.DumpLog($"Params Added", 6);
            using (SqlDataReader tmpReader = SelectCommand.ExecuteReader())
            {
                while (tmpReader.Read())
                {
                    Logger.DumpLog($"Row Found, parsing", 6);
                    results.Add(ParseResult<T>(tmpReader));
                }
            }
            return results.ToArray();
        }

        protected override T PerformGet<T>(int Id) //where T : PersistentBase, new()
        {
            T Result = null;
            string Select = ConstructSelect(typeof(T));
            Select += " WHERE [ID]=@1";
            SqlCommand SelectCommand = new SqlCommand(Select, Connection);
            SelectCommand.Parameters.AddWithValue("@1", Id);
            using (SqlDataReader tmpReader = SelectCommand.ExecuteReader())
            {
                if (tmpReader.HasRows)
                {
                    Result = ParseResult<T>(tmpReader);
                }
            }
            return Result;

        }

        public override string GetConnectionString()
        {
            throw new NotImplementedException();
        }
    }
}
