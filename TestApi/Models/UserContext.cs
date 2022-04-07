using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.EntityFrameworkCore;

namespace TestApi.Models
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        //public UserContext(DbContextOptions<UserContext> options) : base(options)
        //{
            
        //}
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.UseSerialColumns();
        //}
    }
}