using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Backend.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatRoomMember> ChatRoomMembers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<UserBlock> UserBlocks { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<SavedPost> SavedPosts { get; set; }
    public DbSet<DeletedMessage> DeletedMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
  
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasQueryFilter(u => !u.IsDeleted);
            entity.HasIndex(u => u.Username).IsUnique().HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(u => u.IsOnline);
        });
   
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.CreatedAt);

            entity.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); 
        });
   
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.PostId);

            entity.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade); 

            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(f => new { f.FollowerId, f.FollowingId });

            entity.HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Following)
                .WithMany(u => u.FollowedBy)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<UserBlock>(entity =>
        {
            entity.HasKey(ub => new { ub.BlockerId, ub.BlockedId });

            entity.HasOne(ub => ub.Blocker)
                .WithMany(u => u.BlockedUsers)
                .HasForeignKey(ub => ub.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ub => ub.Blocked)
                .WithMany(u => u.BlockedBy)
                .HasForeignKey(ub => ub.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.HasKey(pl => new { pl.UserId, pl.PostId });

            entity.HasOne(pl => pl.User)
                .WithMany(u => u.LikedPosts)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pl => pl.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade); 
        });

        
        modelBuilder.Entity<SavedPost>(entity =>
        {
            entity.HasKey(sp => new { sp.UserId, sp.PostId });

            entity.HasOne(sp => sp.User)
                .WithMany(u => u.SavedPosts)
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sp => sp.Post)
                .WithMany(p => p.SavedByUsers)
                .HasForeignKey(sp => sp.PostId)
                .OnDelete(DeleteBehavior.Cascade); 
        });

        
        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasKey(cr => cr.Id);
        });

        modelBuilder.Entity<ChatRoomMember>(entity =>
        {
            entity.HasKey(crm => new { crm.ChatRoomId, crm.UserId });

            entity.HasOne(crm => crm.ChatRoom)
                .WithMany(cr => cr.Members)
                .HasForeignKey(crm => crm.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade); 

            entity.HasOne(crm => crm.User)
                .WithMany(u => u.ChatRoomMemberships)
                .HasForeignKey(crm => crm.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.ChatRoomId);
            entity.HasIndex(m => m.SentAt);

            entity.HasOne(m => m.ChatRoom)
                .WithMany(cr => cr.Messages)
                .HasForeignKey(m => m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade); 

            entity.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<DeletedMessage>(entity =>
        {
            entity.HasKey(dm => new { dm.UserId, dm.MessageId });

            entity.HasOne(dm => dm.User)
                .WithMany(u => u.DeletedMessages)
                .HasForeignKey(dm => dm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dm => dm.Message)
                .WithMany(m => m.DeletedByUsers)
                .HasForeignKey(dm => dm.MessageId)
                .OnDelete(DeleteBehavior.Cascade); 
        });

    }
}
