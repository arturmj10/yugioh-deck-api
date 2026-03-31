using Microsoft.EntityFrameworkCore;
using YugiohDeck.API.Models;

namespace YugiohDeck.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Card> Cards { get; set; } = null!;
    public DbSet<Deck> Decks { get; set; } = null!;
    public DbSet<DeckCard> DeckCards { get; set; } = null!;
    public DbSet<DeckConfiguration> DeckConfigurations { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Card>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });
        
        modelBuilder.Entity<Deck>()
            .HasOne(d => d.Configuration)
            .WithOne(c => c.Deck)
            .HasForeignKey<DeckConfiguration>(c => c.DeckId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<DeckCard>()
            .HasOne(dc => dc.Deck)
            .WithMany(d => d.DeckCards)
            .HasForeignKey(dc => dc.DeckId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<DeckCard>()
            .HasOne(dc => dc.Card)
            .WithMany(c => c.DeckCards)
            .HasForeignKey(dc => dc.CardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}