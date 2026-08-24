using Agendamento.Models;
using Microsoft.EntityFrameworkCore;

namespace Agendamento.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tarefa> Tarefas => Set<Tarefa>();

        public DbSet<Usuario> Usuarios => Set<Usuario>();

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

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.NomeUsuario).IsRequired().HasMaxLength(100);
                entity.Property(u => u.SenhaHash).IsRequired();
                entity.Property(u => u.Papel).IsRequired().HasMaxLength(50);
                entity.HasIndex(u => u.NomeUsuario).IsUnique();
            });
        }
    }
}
