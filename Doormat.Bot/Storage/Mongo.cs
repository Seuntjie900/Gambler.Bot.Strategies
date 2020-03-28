using System;
using System.Collections.Generic;
using System.Text;
using DoormatCore.Games;
using DoormatCore.Helpers;
using DoormatCore.Sites;

namespace DoormatCore.Storage
{
    class Mongo : SQLBase
    {
        public Mongo(string ConnectionString) : base(ConnectionString)
        {
            Logger.DumpLog("Create Mongo Connection", 6);
        }

        public override string GetConnectionString()
        {
            throw new NotImplementedException();
        }

        protected override void CreateTable(Type type)
        {
            throw new NotImplementedException();
        }

        protected override T[] PerformFind<T>(string Criteria, string Sorting = "", params object[] SqlParams)
        {
            throw new NotImplementedException();
        }

        protected override T PerformGet<T>(int Id)
        {
            throw new NotImplementedException();
        }

        protected override T PerformInsert<T>(T ValueToInsert)
        {
            throw new NotImplementedException();
        }

        protected override T PerformUpdate<T>(T ValueToInsert)
        {
            throw new NotImplementedException();
        }
    }
}
