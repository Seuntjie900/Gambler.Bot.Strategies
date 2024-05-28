using System;
using System.Collections.Generic;
using System.Text;
using Gambler.Bot.Core.Games;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites;
using Microsoft.Extensions.Logging;

namespace Gambler.Bot.Core.Storage
{
    class Mongo : SQLBase
    {
        
        public Mongo(string ConnectionString) : base(ConnectionString)
        {
            _Logger?.LogDebug("Create Mongo Connection");
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
