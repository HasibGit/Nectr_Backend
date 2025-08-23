using System;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) :
    IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole,
            IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    public DbSet<Like> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
               .HasMany(appUser => appUser.UserRoles)
               .WithOne(appUser => appUser.User)
               .HasForeignKey(appUser => appUser.UserId)
               .IsRequired();

        builder.Entity<AppRole>()
               .HasMany(appRole => appRole.UserRoles)
               .WithOne(appRole => appRole.Role)
               .HasForeignKey(appRole => appRole.RoleId)
               .IsRequired();

        builder.Entity<Like>().HasKey(record => new { record.SourceUserId, record.TargetUserId });

        builder.Entity<Like>()
            .HasOne(s => s.SourceUser)
            .WithMany(l => l.Likes)
            .HasForeignKey(s => s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Like>()
            .HasOne(s => s.TargetUser)
            .WithMany(l => l.LikedBy)
            .HasForeignKey(s => s.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Message>()
            .HasOne(x => x.Recipient)
            .WithMany(x => x.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(x => x.Sender)
            .WithMany(x => x.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
