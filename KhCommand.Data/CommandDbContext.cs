using KhCommand.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KhCommand.Data;

public class CommandDbContext : DbContext
{
    public virtual DbSet<Command> Commands { get; set; }
    public virtual DbSet<Synthesis> Synthesises { get; set; }

    public CommandDbContext()
    {        
    }

    public CommandDbContext(DbContextOptions<CommandDbContext> options)
        : base(options)
    {        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Command>(b =>
        {
            b
                .HasKey(c => c.CommandId);
            b
                .Property(x => x.CommandType)
                .HasConversion(
                    v => v.ToString(),
                    v => (CommandType)Enum.Parse(typeof(CommandType), v));
        });

        modelBuilder.Entity<Synthesis>(b =>
        {
            b
                .HasKey(x => x.Id);

            b
                .HasOne(x => x.Command1)
                .WithMany()
                .HasForeignKey(x => x.Command1Id)
                .OnDelete(DeleteBehavior.Restrict);
            b
                .HasOne(x => x.Command2)
                .WithMany()
                .HasForeignKey(x => x.Command2Id)
                .OnDelete(DeleteBehavior.Restrict);

            b
                .HasOne(x => x.CommandResult)
                .WithMany()
                .HasForeignKey(x => x.ResultId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
