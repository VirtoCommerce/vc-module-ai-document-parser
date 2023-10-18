using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.AiDocumentParser.Data.Repositories;

public class AiDocumentParserDbContext : DbContextWithTriggers
{
    public AiDocumentParserDbContext(DbContextOptions<AiDocumentParserDbContext> options)
        : base(options)
    {
    }

    protected AiDocumentParserDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<AiDocumentParserEntity>().ToTable("AiDocumentParser").HasKey(x => x.Id);
        //modelBuilder.Entity<AiDocumentParserEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
    }
}
