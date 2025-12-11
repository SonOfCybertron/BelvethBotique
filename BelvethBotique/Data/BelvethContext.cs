using BelvethBotique.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BelvethBotique.Data
{
    [DbConfigurationType(typeof(MySql.Data.EntityFramework.MySqlEFConfiguration))]
    public class BelvethContext : DbContext
    {
        public BelvethContext() : base("name=BelvethContext") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<BelvethBotique.Models.Order> Orders { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

    }
}