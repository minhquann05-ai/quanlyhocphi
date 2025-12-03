using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt; 
using Microsoft.IdentityModel.Tokens;
using QLHocPhi.DAL.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Services
{
    public class NguoiDungService : INguoiDungService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private const string SECRET_KEY = "111111111111111111111111111111111111111111111";

        public NguoiDungService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.SinhVien)
                .FirstOrDefaultAsync(u => u.TenDangNhap == loginDto.TenDangNhap);

            if (user == null || user.MatKhau != loginDto.MatKhau)
            {
                throw new Exception("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SECRET_KEY);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.TenDangNhap),
                    new Claim(ClaimTypes.Role, user.VaiTro ?? "SinhVien"),
                    new Claim("MaSv", user.MaSv ?? "")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new UserDto
            {
                TenDangNhap = user.TenDangNhap,
                HoTen = user.SinhVien?.HoTen ?? "Admin",
                VaiTro = user.VaiTro,
                MaSv = user.MaSv,
                Token = tokenString
            };
        }

        public async Task ChangePasswordAsync(ChangePasswordDto dto)
        {
            var user = await _context.NguoiDungs.FindAsync(dto.TenDangNhap);

            if (user == null)
                throw new Exception("Tài khoản không tồn tại.");

            if (user.MatKhau != dto.MatKhauCu)
                throw new Exception("Mật khẩu cũ không chính xác.");

            user.MatKhau = dto.MatKhauMoi;

            _context.NguoiDungs.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GenerateAccountsForStudentsAsync()
        {
            var studentsWithoutAccount = await _context.SinhViens
                .Where(sv => !_context.NguoiDungs.Any(u => u.MaSv == sv.MaSv))
                .ToListAsync();

            if (!studentsWithoutAccount.Any()) return 0;

            var newAccounts = new List<NguoiDung>();

            foreach (var sv in studentsWithoutAccount)
            {
                newAccounts.Add(new NguoiDung
                {
                    TenDangNhap = sv.MaSv,
                    MatKhau = "123456",
                    VaiTro = "SinhVien",
                    MaSv = sv.MaSv
                });
            }

            await _context.NguoiDungs.AddRangeAsync(newAccounts);
            await _context.SaveChangesAsync();

            return newAccounts.Count;      
        }

        public async Task CreateDefaultAdminAsync()
        {
            var adminExists = await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == "admin");

            if (adminExists)
            {
                throw new Exception("Tài khoản admin đã tồn tại.");
            }

            var admin = new NguoiDung
            {
                TenDangNhap = "admin",
                MatKhau = "123456",
                VaiTro = "PhongTaiChinh",
                MaSv = null       
            };

            _context.NguoiDungs.Add(admin);
            await _context.SaveChangesAsync();
        }
    }
}
