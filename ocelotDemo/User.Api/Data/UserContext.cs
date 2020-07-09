using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.Api.Models;

namespace User.Api.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {

        }

        public DbSet<AppUser> Users { get; set; }

        public DbSet<UserProperty> UserProperties { get; set; }

        public DbSet<UserTag> UserTags { get; set; }

        public DbSet<BPFile> BPFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>(b =>
            {
                b.ToTable("Users");
                b.HasKey(o => o.Id);
            });

            modelBuilder.Entity<UserProperty>(b =>
            {
                b.ToTable("UserProperties");
                b.Property(a => a.Key).HasMaxLength(100);   //限定主键长度
                b.Property(a => a.Value).HasMaxLength(100); //限定主键长度              
                b.HasKey(a => new { a.AppUserId, a.Key, a.Value });//主键唯一
            });

            modelBuilder.Entity<UserTag>(b =>
            {
                b.ToTable("UserTags");
                b.Property(a => a.Tag).HasMaxLength(100);  //限定主键长度
                b.HasKey(a => new { a.AppUserId, a.Tag });
            });

            modelBuilder.Entity<BPFile>(b => {
                b.ToTable("BPFiles");
                b.HasKey(a => a.Id);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
