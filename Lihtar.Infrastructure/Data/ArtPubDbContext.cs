using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Data;

public class ArtPubDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ArtPubDbContext(DbContextOptions<ArtPubDbContext> options) : base(options) { }

    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuItemTag> MenuItemTags => Set<MenuItemTag>();
    public DbSet<MenuItemTagLink> MenuItemTagLinks => Set<MenuItemTagLink>();

    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventTicket> EventTickets => Set<EventTicket>();

    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentItem> PaymentItems => Set<PaymentItem>();

    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<MenuItemReview> MenuItemReviews => Set<MenuItemReview>();
    public DbSet<EventReview> EventReviews => Set<EventReview>();

    public DbSet<FavoriteMenuItem> FavoriteMenuItems => Set<FavoriteMenuItem>();
    public DbSet<BonusTransaction> BonusTransactions => Set<BonusTransaction>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<GalleryAlbum> GalleryAlbums => Set<GalleryAlbum>();
    public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();

    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ArtPubDbContext).Assembly);

        builder.Entity<Order>()
            .Property(x => x.TotalPrice)
            .HasPrecision(18, 2);

        builder.Entity<OrderItem>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Entity<PaymentItem>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Entity<PaymentItem>()
            .HasOne(x => x.Payment)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PaymentItem>()
            .HasOne(x => x.OrderItem)
            .WithMany()
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}