using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Mappings;

public class BookTypeConfiguration : EntityTypeConfiguration<Book>
{
    public override void Configure(EntityTypeBuilder<Book> builder)
    {
        base.Configure(builder);

        builder.ToTable("Books");

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Title");

        builder.Property(p => p.Author)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Author");

        builder.Property(p => p.Description)
            .HasMaxLength(1024)
            .HasColumnName("Description");

        // Índice único para título para garantir que não haja duplicatas
        builder.HasIndex(p => p.Title)
            .IsUnique()
            .HasDatabaseName("IX_Books_Title");
    }
}

