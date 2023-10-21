using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;
using static DoormatCore.Sites.BaseSite;
using Microsoft.Data.Sqlite;

namespace DoormatCore.Storage
{
    class Sqlite : SQLBase
    {
        SqliteConnection Connection = new SqliteConnection();

        public Sqlite(string ConnectionString) : base(ConnectionString)
        {
            Logger.DumpLog("Create Sqlite Connection", 6);
            Connection = new SqliteConnection(ConnectionString);
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

            if (type.GetCustomAttribute<PersistentTableName>() == null)
                return;
            string TableName = type.GetCustomAttribute<PersistentTableName>().TableName;

            SqliteCommand CheckTableExists = new SqliteCommand("SELECT name FROM Sqlite_master WHERE type='table' AND name='" + TableName + "'", Connection);
            SqliteDataReader Reader = CheckTableExists.ExecuteReader();
            if (Reader.HasRows)
            {
                /*List<string> Columns = new List<string>();
                while (Reader.Read())
                {
                    Columns.Add(Reader[0].ToString());
                }

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
                            SqliteCommand AddColumn = new SqliteCommand(Query, Connection);
                            AddColumn.ExecuteNonQuery();
                        }
                    }
                }*/

            }
            else
            {
                string query = "Create table " + TableName + "(id BIGINT PRIMARY KEY ";
                foreach (PropertyInfo PI in type.GetProperties())
                {
                    if (!PI.PropertyType.IsArray)
                    {
                        if (PI.Name.ToLower() != "id")
                        {
                            query += ", " + PI.Name + " " + GetDBType(PI.PropertyType.Name);
                            /*SqliteCommand AddColumn = new SqliteCommand(Query, Connection);
                            AddColumn.ExecuteNonQuery();*/
                        }
                    }
                }
                query += ")";
                SqliteCommand CreateTable = new SqliteCommand(query, Connection);
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
            string query = "INSERT INTO [" + TableName + "](";
            string values = " Values (";
            bool first = true;

            int i = 1;
            SqliteCommand tmpCommand = new SqliteCommand();

            foreach (PropertyInfo PI in type.GetProperties())
            {
                
                if (!PI.PropertyType.IsArray && PI.Name.ToLower() != "id" && PI.GetCustomAttribute(typeof(NonPersistent)) == null)
                {

                    if (!first)
                    {
                        query += ", ";
                        values += ",";
                    }
                    first = false;
                    query += "[" + PI.Name + "]";
                    values += "@" + i.ToString();
                    tmpCommand.Parameters.AddWithValue("@" + i++.ToString(), PI.GetValue(ValueToInsert)?? DBNull.Value );
                }
            }
            query += ") ";
            values += "); select last_insert_rowid()";
            tmpCommand.CommandText = query + values;
            tmpCommand.Connection = Connection; //Set sql connection here
            ValueToInsert.Id = (int)(long)tmpCommand.ExecuteScalar();
            return ValueToInsert;
        }

        protected override T PerformUpdate<T>(T ValueToUpdate)// where T : PersistentBase
        {
            if (ValueToUpdate.Id<=0)
            {
                return PerformInsert<T>(ValueToUpdate);
            }
            Type type = ValueToUpdate.GetType();
            string TableName = type.GetCustomAttribute<PersistentTableName>().TableName;

            string query = "UPDATE [" + TableName + "] set";
            bool first = true;

            int i = 1;
            SqliteCommand tmpCommand = new SqliteCommand();

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
            tmpCommand.Connection = Connection; //Set sql connection here
            tmpCommand.ExecuteNonQuery();

            return ValueToUpdate;
        }

        protected override T[] PerformFind<T>(string Criteria, string Sorting = "", params object[] SqlParams)// where T : PersistentBase, new()
        {
            string Select = ConstructSelect(typeof(T));
            if (!string.IsNullOrWhiteSpace(Criteria))
                Select += " WHERE " + Criteria;
            if (!string.IsNullOrWhiteSpace(Sorting))
                Select += "ORDER BY " + Sorting;
            List<T> results = new List<T>();
            SqliteCommand SelectCommand = new SqliteCommand(Select, Connection);
            for (int i = 0; i < SqlParams.Length; i++)
            {
                SelectCommand.Parameters.AddWithValue("@" + (i + 1), SqlParams[i]);
            }

            SqliteDataReader tmpReader = SelectCommand.ExecuteReader();
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
            SqliteCommand SelectCommand = new SqliteCommand(Select, Connection);
            SelectCommand.Parameters.AddWithValue("@1", Id);
            SqliteDataReader tmpReader = SelectCommand.ExecuteReader();
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
