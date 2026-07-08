using Agendamento.Models;
using Microsoft.EntityFrameworkCore;

namespace Agendamento.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tarefa> Tarefas => Set<Tarefa>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tarefa>(entity =>
            {
                entity.ToTable("Tarefas");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Titulo).IsRequired().HasMaxLength(150);
                entity.Property(t => t.Descricao).HasMaxLength(1000);
                entity.Property(t => t.Local).HasMaxLength(150);
                entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(t => t.DataAgendamento);
                entity.HasIndex(t => t.Status);
            });
        }
    }
}
