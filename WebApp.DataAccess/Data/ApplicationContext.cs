using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext()
            : base()
        {
        }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<PostLike> PostLikes { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<CommentLike> CommentLikes { get; set; }

        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            _ = modelBuilder.Entity<User>().Property(e => e.RegistrationDate).HasDefaultValueSql("GETDATE()");
            _ = modelBuilder.Entity<Post>().Property(e => e.PostedDate).HasDefaultValueSql("GETDATE()");
            _ = modelBuilder.Entity<Post>().Property(e => e.Views).HasDefaultValue(0);
            _ = modelBuilder.Entity<Comment>().Property(e => e.CommentDate).HasDefaultValueSql("GETDATE()");
            _ = modelBuilder.Entity<User>().Property(e => e.Image).HasDefaultValue("/userProfileImages/defaultUser.jpg");

            _ = modelBuilder.Entity<Comment>()
              .HasOne(c => c.User)
              .WithMany(u => u.Comments)
              .HasForeignKey(c => c.UserId)
              .OnDelete(DeleteBehavior.Restrict);

            _ = modelBuilder.Entity<PostLike>()
             .HasKey(l => new { l.PostId, l.UserId });

            _ = modelBuilder.Entity<PostLike>()
                .HasOne(l => l.User)
                .WithMany(u => u.PostLikes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            _ = modelBuilder.Entity<PostLike>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = modelBuilder.Entity<CommentLike>()
             .HasKey(l => new { l.CommentId, l.UserId });

            _ = modelBuilder.Entity<CommentLike>()
                .HasOne(l => l.User)
                .WithMany(u => u.CommentLikes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            _ = modelBuilder.Entity<CommentLike>()
                .HasOne(l => l.Comment)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);

            _ = optionsBuilder.LogTo(Console.WriteLine);
        }
    }
}
