using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoviePlatformAPI.Models;

namespace MoviePlatformAPI.Data.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(1500);

        builder.Property(m => m.Genre)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(m => m.Owner)
            .WithMany(u => u.Movies)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}