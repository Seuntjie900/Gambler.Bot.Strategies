
using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using static DoormatCore.Sites.BaseSite;

namespace DoormatCore.Storage
{
    /// <summary>
    /// Base interface for reading and writing data to and from a database.
    /// </summary>
    public abstract class SQLBase
    {
        public static SQLBase OpenConnection(string ConnectionString, string Provider, List<Type> types)
        {
            SQLBase newbase = null;
            Logger.DumpLog("Creating SQLBase instance", 6);
            switch (Provider.ToLower())
            {
                case "mysql": newbase = new MYSql(ConnectionString);break ;
                case "sqlite": newbase = new SQLite(ConnectionString); break;
                case "mssql": newbase = new MSSQL(ConnectionString); break;
                case "mongo": newbase = new Mongo(ConnectionString); break;
                case "postgre": newbase = new PostGre(ConnectionString); break;
                default: newbase = new SQLite(ConnectionString); break;

            }
            Logger.DumpLog("Created SQLBase instance", 6);

            newbase.CreateTables(types);
            return newbase;
        }

        public static long DateToLong(DateTime DateValue)
        {
            return (long)(DateValue - new DateTime(1970, 1, 1)).TotalSeconds;
        }
        public static decimal DateToDecimal(DateTime DateValue)
        {
            return (decimal)(DateValue - new DateTime(1970, 1, 1)).TotalMilliseconds/1000.0m;
        }
        public static DateTime DateFromLong(long DateValue)
        {
            return new DateTime(1970, 1, 1).AddSeconds(DateValue);
        }
        public static DateTime DateFromDecimal(decimal DateValue)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds((double)(DateValue*1000.0m));
        }

        public string ProviderName { get; protected set; }
        public abstract string GetConnectionString();


        protected SQLBase(string ConnectionString)
        {
            
        }
        
        

        public void CreateTables(List<Type> PersistentClasses)
        {
            Logger.DumpLog("Creating Tables", 6);
            
            foreach (Type x in PersistentClasses)
            {
                try
                {
                    CreateTable(x);
                }
                catch (Exception e)
                {

                }
            }           
            Logger.DumpLog("Tables Created", 6);
        }

        protected abstract void CreateTable(Type type);

        protected T ParseResult<T>(IDataReader Reader) where T : new()
        {
            Logger.DumpLog("Parsing result for DB", 5);
            Type typ = typeof(T);
            T tmp = new T();
            foreach (PropertyInfo x in typ.GetProperties())
            {
                Logger.DumpLog($"Checking {x.Name}", 6);
                if (!Attribute.IsDefined(x, typeof(NonPersistent)) && !x.PropertyType.IsArray)
                {
                    Logger.DumpLog($"Found {x.Name}, getting index and checking null", 6);
                    int index = Reader.GetOrdinal(x.Name);
                    if (Reader.IsDBNull(index))
                    {
                        Logger.DumpLog($"Found {x.Name}, null", 6);
                    }
                    else
                    {
                        Logger.DumpLog($"Found {x.Name}, setting value", 6);
                        x.SetValue(tmp, Reader[x.Name]);
                    }
                }
            }
            return tmp;
        }

        public T Save<T>(T Value) where T : PersistentBase
        {
            if (Value.Id > 0)
                return PerformUpdate<T>(Value);
            else
                return PerformInsert<T>(Value);
        }
        public T Get<T>(int Id) where T : PersistentBase, new()
        {
            return PerformGet<T>(Id);
        }
        public T FindSingle<T>(string Criteria, string Sorting="", params object[] SqlParams) where T : PersistentBase, new()
        {
            T[] results = PerformFind<T>(Criteria, Sorting, SqlParams);
            if (results.Length > 0)
                return results[0];
            else return null;
        }
        public T[] Find<T>(string Criteria, string Sorting = "", params object[] SqlParams) where T : PersistentBase, new()
        {
            return PerformFind<T>(Criteria, Sorting, SqlParams);
        }
        protected abstract T PerformInsert<T>(T ValueToInsert) where T : PersistentBase;
        protected abstract T PerformUpdate<T>(T ValueToInsert) where T : PersistentBase;
        protected abstract T[] PerformFind<T>(string Criteria, string Sorting = "", params object[] SqlParams) where T : PersistentBase, new();
        protected abstract T PerformGet<T>(int Id) where T : PersistentBase, new();


    }
    [PersistentTableName("CURRENCY")]
    public class Currency : PersistentBase
    {
        
        public string Name { get; set; }
        public string Symbol { get; set; }
        public byte[] Icon { get; set; }
    }
    [PersistentTableName("SEED")]
    public class Seed : PersistentBase
    {
        
        public string ServerSeed { get; set; }
        public string ServerSeedHash { get; set; }
        public string ClientSeed { get; set; }
        public long Nonce { get; set; }
    }
    [PersistentTableName("USER")]
    public class User : PersistentBase
    {
        
        public Site Site { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
    }
    [PersistentTableName("SITE")]
    public class Site : PersistentBase
    {
        //Site name, link, class name, image, currencies
        
        public string Name { get; set; }
        public string Link { get; set; }
        public string ClassName { get; set; }
        [NonPersistent]
        public byte[] Image { get; set; }
        public Currency[] Currencies { get; set; }
        private string currencyString;

    }

    
}
