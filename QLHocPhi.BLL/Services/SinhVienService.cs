using AutoMapper;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using QLHocPhi.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Services
{
    public class SinhVienService : ISinhVienService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public SinhVienService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SinhVienDto>> GetAllAsync(string? keyword = null)
        {
            var query = _context.SinhViens
                .Include(sv => sv.LopHoc)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();

                // --- CẬP NHẬT LOGIC TÌM KIẾM ĐA NĂNG ---
                // Tìm trong Mã SV, Họ Tên, Mã Lớp, Email hoặc SĐT
                query = query.Where(sv => sv.HoTen.ToLower().Contains(keyword)
                                       || sv.MaSv.ToLower().Contains(keyword)
                                       || (sv.MaLop != null && sv.MaLop.ToLower().Contains(keyword))
                                       || (sv.Email != null && sv.Email.ToLower().Contains(keyword))
                                       || (sv.Sdt != null && sv.Sdt.Contains(keyword)));
                // ---------------------------------------
            }

            var list = await query.ToListAsync();
            return _mapper.Map<IEnumerable<SinhVienDto>>(list);
        }

        public async Task<SinhVienDto> GetByIdAsync(string maSv)
        {
            var sv = await _context.SinhViens
                .Include(s => s.LopHoc)
                .FirstOrDefaultAsync(s => s.MaSv == maSv);
            if (sv == null) throw new KeyNotFoundException("Sinh viên không tồn tại");
            return _mapper.Map<SinhVienDto>(sv);
        }

        public async Task<SinhVienDto> CreateAsync(SinhVienCreateDto createDto)
        {
            // 1. Kiểm tra trùng mã
            if (await _context.SinhViens.AnyAsync(s => s.MaSv == createDto.MaSv))
                throw new Exception("Mã sinh viên đã tồn tại.");

            var sinhVien = _mapper.Map<SinhVien>(createDto);

            // Bắt đầu Transaction để đảm bảo tính toàn vẹn
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Thêm Sinh Viên
                _context.SinhViens.Add(sinhVien);

                // 3. TỰ ĐỘNG TẠO TÀI KHOẢN (Logic chúng ta đã bàn)
                var taiKhoan = new NguoiDung
                {
                    TenDangNhap = sinhVien.MaSv, // User = Mã SV
                    MatKhau = "123456",          // Pass mặc định
                    VaiTro = "SinhVien",
                    MaSv = sinhVien.MaSv
                };
                _context.NguoiDungs.Add(taiKhoan);

                // 4. Lưu cả 2 cùng lúc
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdAsync(sinhVien.MaSv);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task UpdateAsync(string maSv, SinhVienUpdateDto updateDto)
        {
            var sinhVien = await _context.SinhViens.FindAsync(maSv);
            if (sinhVien == null)
            {
                throw new KeyNotFoundException("Sinh viên không tồn tại.");
            }

            // Dùng AutoMapper để cập nhật dữ liệu từ DTO vào Entity
            _mapper.Map(updateDto, sinhVien);

            _context.SinhViens.Update(sinhVien);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string maSv)
        {
            var sinhVien = await _context.SinhViens.FindAsync(maSv);
            if (sinhVien == null)
            {
                throw new KeyNotFoundException("Sinh viên không tồn tại.");
            }

            // 1. Xóa tài khoản đăng nhập tương ứng (nếu có)
            var taiKhoan = await _context.NguoiDungs.FindAsync(maSv); // Vì User = MaSv
            if (taiKhoan != null)
            {
                _context.NguoiDungs.Remove(taiKhoan);
            }

            // 2. Xóa sinh viên
            // Lưu ý: Nếu sinh viên đã có Hóa đơn/Điểm, việc xóa có thể bị lỗi do ràng buộc khóa ngoại (tùy CSDL)
            _context.SinhViens.Remove(sinhVien);

            await _context.SaveChangesAsync();
        }
    }
}
