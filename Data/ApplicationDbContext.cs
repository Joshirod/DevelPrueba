using DevelPrueba.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace DevelPrueba.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }


        public DbSet<Encuesta> Encuestas => Set<Encuesta>();
        public DbSet<Campo> Campos => Set<Campo>();
        public DbSet<Respuesta> Respuestas => Set<Respuesta>();
        public DbSet<RespuestaValor> RespuestaValores => Set<RespuestaValor>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Encuesta>()
                .HasIndex(e => e.SlugPublico)
                .IsUnique();

            // Encuesta -> Campos (CASCADE)
            builder.Entity<Encuesta>()
                .HasMany(e => e.Campos)
                .WithOne(c => c.Encuesta!)
                .HasForeignKey(c => c.EncuestaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Respuesta -> RespuestaValores (CASCADE)
            builder.Entity<Respuesta>()
                .HasMany(r => r.Valores)
                .WithOne(v => v.Respuesta!)
                .HasForeignKey(v => v.RespuestaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices de Campo
            builder.Entity<Campo>()
                .HasIndex(c => new { c.EncuestaId, c.Orden });
            builder.Entity<Campo>()
                .HasIndex(c => new { c.EncuestaId, c.Nombre })
                .IsUnique();

            // Respuesta -> Encuesta (RESTRICT)  ← clave para evitar múltiples cascadas
            builder.Entity<Respuesta>()
                .HasOne(r => r.Encuesta!)
                .WithMany()
                .HasForeignKey(r => r.EncuestaId)
                .OnDelete(DeleteBehavior.Restrict);

            // RespuestaValor -> Campo (RESTRICT)  ← clave para evitar múltiples cascadas
            builder.Entity<RespuestaValor>()
                .HasOne(v => v.Campo!)
                .WithMany()
                .HasForeignKey(v => v.CampoId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}