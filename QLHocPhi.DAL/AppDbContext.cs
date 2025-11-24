using Microsoft.EntityFrameworkCore;
using QLHocPhi.DAL.Entities;

namespace QLHocPhi.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Khoa> Khoas { get; set; }
        public DbSet<NganhHoc> NganhHocs { get; set; }
        public DbSet<HocKy> HocKys { get; set; }
        public DbSet<BieuPhi> BieuPhis { get; set; }
        public DbSet<LopHoc> LopHocs { get; set; }
        public DbSet<SinhVien> SinhViens { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DbSet<MonHoc> MonHocs { get; set; }
        public DbSet<DangKyHocPhan> DangKyHocPhans { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }
        public DbSet<BienLai> BienLais { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<LopHocPhan> LopHocPhans { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NganhHoc>(entity =>
            {
                entity.HasOne(d => d.Khoa) 
                      .WithMany(p => p.NganhHocs) 
                      .HasForeignKey(d => d.MaKhoa)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BieuPhi>(entity =>
            {
                entity.HasOne(d => d.NganhHoc)
                      .WithMany(p => p.BieuPhis)
                      .HasForeignKey(d => d.MaNganh)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<BieuPhi>(entity =>
            {
                entity.HasOne(d => d.HocKy)
                      .WithMany(p => p.BieuPhis)
                      .HasForeignKey(d => d.MaHk)
                      .OnDelete(DeleteBehavior.ClientSetNull); 
            });
            modelBuilder.Entity<SinhVien>(entity =>
            {
                entity.HasOne(d => d.LopHoc)
                      .WithMany(p => p.SinhViens)
                      .HasForeignKey(d => d.MaLop)
                      .OnDelete(DeleteBehavior.SetNull); 
            });

            modelBuilder.Entity<ChiTietHoaDon>(entity =>
            {
                entity.HasOne(d => d.HoaDon)
                      .WithMany(p => p.ChiTietHoaDons)
                      .HasForeignKey(d => d.MaHd)
                      .OnDelete(DeleteBehavior.Cascade); 
            });
            modelBuilder.Entity<MonHoc>(entity =>
            {
                entity.HasOne(d => d.NganhHoc)
                      .WithMany() 
                      .HasForeignKey(d => d.MaNganh)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ThanhToan>(entity =>
            {
                entity.HasOne(d => d.HoaDon)
                      .WithMany() 
                      .HasForeignKey(d => d.MaHd)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BienLai>(entity =>
            {
                entity.HasOne(d => d.ThanhToan)
                      .WithMany(p => p.BienLais)
                      .HasForeignKey(d => d.MaTt)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
