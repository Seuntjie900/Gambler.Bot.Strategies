
using Gambler.Bot.Core.Games;
using Gambler.Bot.Core.Helpers;
using Esprima.Ast;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using static Gambler.Bot.Core.Sites.BaseSite;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Gambler.Bot.Core.Sites.Classes;
using Gambler.Bot.AutoBet.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Gambler.Bot.Core.Storage
{
    /// <summary>
    /// Base interface for reading and writing data to and from a database.
    /// </summary>
    public class BotContext:DbContext
    {
        public DbSet<SiteDetails> Sites { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SeedDetails> Seeds { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<SessionStats> Sessions { get; set; }
        public DbSet<DiceBet> DiceBets { get; set; }
        public DbSet<CrashBet> CrashBets { get; set; }
        public DbSet<PlinkoBet> PlinkoBets { get; set; }
        public DbSet<RouletteBet> RouletteBets { get; set; }


        protected readonly ILogger _Logger;
        public BotContext()
        {
            _Logger = null;
        }
        public BotContext(ILogger logger )
        {
            _Logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string password = null;
            if (Settings?.EncryptConstring??false)
            {
                //raise eventy to get connection string
            }
            string connectionstring = Settings?.GetConnectionString(password);
            if (connectionstring == null)
                connectionstring = "Data Source=GamblerBot.db;";
            switch (Settings?.Provider)
            {
                default:
                case "Sqlite": optionsBuilder.UseSqlite(connectionstring); break;
                case "SQLServer": optionsBuilder.UseSqlServer(connectionstring); break;
                case "PostGres": optionsBuilder.UseNpgsql(connectionstring); break;
                case "MongoDB": optionsBuilder.UseMongoDB(connectionstring, "GamblerBot"); break;
                case "MySQL": optionsBuilder.UseMySql(connectionstring, ServerVersion.AutoDetect(connectionstring)); break;
            }
            
            base.OnConfiguring(optionsBuilder);
        }

        public string ProviderName { get; protected set; }

        protected BotContext(string ConnectionString)
        {
            
        }
        

        public PersonalSettings Settings
        {
            get ;
            set;
        }



        public void CreateTables()
        {
            /*
             perform db migration
             */
        }

    }
    
  
    
    public class User
    {
        
        public SiteDetails Site { get; set; }
        public string UserName { get; set; }
        [Key]
        public string UserId { get; set; }
    }
    
    

    
}
