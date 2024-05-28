using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Gambler.Bot.Core.Games;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using static Gambler.Bot.Core.Sites.BaseSite;

namespace Gambler.Bot.Core.Storage
{
    class MYSql : SQLBase
    {
        MySqlConnection Connection = new MySqlConnection();

        public MYSql(string ConnectionString):base(ConnectionString)
        {
            _Logger?.LogDebug("Create MYSql Connection");
            Connection = new MySqlConnection(ConnectionString);
            Connection.Open();
        }

        string GetDBType(string TypeName)
        {
            string DBName = "";
            switch (TypeName.ToLower())
            {
                case "decimal": DBName = "decimal(35,20)"; break;
                case "double": DBName = "decimal(35,20)"; break;
                case "int": DBName = "int"; break;
                case "long": DBName = "bigint"; break;
                case "string": DBName = "nvarchar(500)"; break;
                case "byte": DBName = ""; break;

                default: DBName = "int"; break;
            }
            return DBName;
        }

        protected override void CreateTable(Type type)
        {


            string TableName = type.GetCustomAttribute<PersistentTableName>().TableName;

            MySqlCommand CheckTableExists = new MySqlCommand("select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA='dbo' and TABLE_NAME='" + TableName + "'", Connection);
            //MySqlDataReader Reader = CheckTableExists.ExecuteReader();
            bool TableExists = false;
            List<string> Columns = new List<string>();
            using (MySqlDataReader Reader = CheckTableExists.ExecuteReader())
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
                    if (!PI.PropertyType.IsArray)
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
                            string Query = "alter table " + TableName + " add " + PI.Name + " " + GetDBType(PI.PropertyType.Name);
                            MySqlCommand AddColumn = new MySqlCommand(Query, Connection);
                            AddColumn.ExecuteNonQuery();
                        }
                    }
                }

            }
            else
            {
                string query = "Create table " + TableName + "(id int AUTO_INCREMENT";
                foreach (PropertyInfo PI in type.GetProperties())
                {
                    if (!PI.PropertyType.IsArray)
                    {
                        if (PI.Name.ToLower() != "id")
                        {
                            query += ", " + PI.Name + " " + GetDBType(PI.PropertyType.Name);
                            
                        }
                    }
                }
                query += ", PRIMARY KEY(id))";
                MySqlCommand CreateTable = new MySqlCommand(query, Connection);
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
                if (!PI.PropertyType.IsArray)
                {
                    if (!first)
                    {
                        query += ", ";
                    }
                    first = false;
                    query += "[" + TableName + "].[" + PI.Name + "]";
                }
            }
            query += " FROM " + "[" + TableName + "] ";
            return query;
        }

        protected override T PerformInsert<T>(T ValueToInsert)// where T : PersistentBase
        {
            Type type = ValueToInsert.GetType();

            string TableName = type.GetCustomAttribute<PersistentTableName>().TableName;
            string query = "SELECT INTO [" + TableName + "](";
            string values = " output INSERTED.ID VALUES(";
            bool first = true;

            int i = 1;
            MySqlCommand tmpCommand = new MySqlCommand();

            foreach (PropertyInfo PI in type.GetProperties())
            {
                if (!PI.PropertyType.IsArray && PI.Name.ToLower() != "id")
                {
                    if (!first)
                    {
                        query += ", ";
                        values += ",";
                    }
                    first = false;
                    query += "[" + PI.Name + "]";
                    values += "@" + i.ToString();
                    tmpCommand.Parameters.AddWithValue("@" + i++.ToString(), PI.GetValue(ValueToInsert));
                }
            }
            query += ") ";
            tmpCommand.CommandText = query + values;
            //tmpCommand.Connection = SqlConnection //Set sql connection here
            ValueToInsert.Id = (int)tmpCommand.ExecuteScalar();
            return ValueToInsert;
        }

        protected override T PerformUpdate<T>(T ValueToUpdate)// where T : PersistentBase
        {
            if (ValueToUpdate.Id <= 0)
                return PerformInsert<T>(ValueToUpdate);
            Type type = ValueToUpdate.GetType();
            string TableName = type.GetCustomAttribute<PersistentTableName>().TableName;

            string query = "UPDATE [" + TableName + "] set";
            bool first = true;

            int i = 1;
            MySqlCommand tmpCommand = new MySqlCommand();

            foreach (PropertyInfo PI in type.GetProperties())
            {
                if (!PI.PropertyType.IsArray && PI.Name.ToLower() != "id")
                {
                    if (!first)
                    {
                        query += ", ";
                    }
                    first = false;
                    query += "[" + PI.Name + "] = @" + i.ToString();

                    tmpCommand.Parameters.AddWithValue("@" + i++.ToString(), PI.GetValue(ValueToUpdate));
                }
            }
            query += " WHERE [" + TableName + "].Id = @" + i.ToString();
            tmpCommand.Parameters.AddWithValue("@" + i++.ToString(), ValueToUpdate.Id);
            tmpCommand.CommandText = query;
            //tmpCommand.Connection = SqlConnection //Set sql connection here
            ValueToUpdate.Id = (int)tmpCommand.ExecuteScalar();

            return ValueToUpdate;
        }

        protected override T[] PerformFind<T>(string Criteria, string Sorting = "", params object[] SqlParams)// where T : PersistentBase, new()
        {
            string Select = ConstructSelect(typeof(T));
            if (string.IsNullOrWhiteSpace(Criteria))
                Select += " WHERE " + Criteria;
            if (string.IsNullOrWhiteSpace(Sorting))
                Select += "ORDER BY " + Sorting;
            List<T> results = new List<T>();
            MySqlCommand SelectCommand = new MySqlCommand(Select, Connection);
            for (int i = 0; i < SqlParams.Length; i++)
            {
                SelectCommand.Parameters.AddWithValue("@" + i + 1, SqlParams[i]);
            }

            MySqlDataReader tmpReader = SelectCommand.ExecuteReader();
            while (tmpReader.Read())
            {
                results.Add(ParseResult<T>(tmpReader));
            }
            return results.ToArray();
        }

        protected override T PerformGet<T>(int Id)// where T : PersistentBase, new()
        {
            T Result = null;
            string Select = ConstructSelect(typeof(T));
            Select += " WHERE [ID]=@1";
            MySqlCommand SelectCommand = new MySqlCommand(Select, Connection );
            SelectCommand.Parameters.AddWithValue("@1", Id);
            MySqlDataReader tmpReader = SelectCommand.ExecuteReader();
            if (tmpReader.HasRows)
            {
                Result = ParseResult<T>(tmpReader);
            }
            return Result;

        }

        public override string GetConnectionString()
        {
            throw new NotImplementedException();
        }

    }
}
