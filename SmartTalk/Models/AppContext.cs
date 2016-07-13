using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace SmartTalk.Models
{
    public class AppContext : DbContext
    {
        public AppContext() : base("DefaultConnection") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(g => g.MyGroups)
                .WithMany(x => x.Members)
                .Map(m =>
                {
                    m.MapLeftKey("UserID");
                    m.MapRightKey("GroupID");
                    m.ToTable("UserGroups");
                });

            modelBuilder.Entity<User>()
                .HasMany(u => u.MyFavorites)
                .WithMany(x => x.MyFollowers)
                .Map(m =>
                {
                    m.MapLeftKey("UserID1");
                    m.MapRightKey("UserID2");
                    m.ToTable("UserUsers");
                });

            modelBuilder.Entity<Group>()
                .HasMany(g => g.MemberRequests)
                .WithMany(u => u.RequestedGroups)
                .Map(m =>
                {
                    m.MapLeftKey("GroupId");
                    m.MapRightKey("UserId");
                    m.ToTable("UserRequestedGroups");
                });
        }
    }
}