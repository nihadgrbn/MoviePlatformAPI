using Microsoft.EntityFrameworkCore;
using MoviePlatformAPI.Models;

namespace MoviePlatformAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
    public DbSet<User> Users { get; set; }
    public DbSet<Movie> Movies { get; set; }
    

}