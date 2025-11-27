using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using QLHocPhi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Services
{
    public class LopHocPhanService : ILopHocPhanService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public LopHocPhanService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LopHocPhanDto>> GetAllAsync()
        {
            var list = await _context.LopHocPhans
                .Include(l => l.MonHoc)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<LopHocPhanDto>>(list);
        }

        public async Task<IEnumerable<LopHocPhanDto>> GetByNganhAsync(string maNganh)
        {
            // Lọc các lớp học phần mà Môn học của nó thuộc Ngành đó
            var list = await _context.LopHocPhans
                .Include(l => l.MonHoc)
                .Where(l => l.MonHoc.MaNganh == maNganh)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<LopHocPhanDto>>(list);
        }

        public async Task<LopHocPhanDto> CreateAsync(LopHocPhanCreateDto createDto)
        {
            // 1. Kiểm tra trùng mã
            if (await _context.LopHocPhans.AnyAsync(l => l.MaLhp == createDto.MaLhp))
                throw new Exception($"Mã lớp học phần {createDto.MaLhp} đã tồn tại.");

            // 2. Kiểm tra Môn học và Học kỳ có tồn tại không
            if (!await _context.MonHocs.AnyAsync(m => m.MaMh == createDto.MaMh))
                throw new KeyNotFoundException("Mã môn học không tồn tại.");
            if (!await _context.HocKys.AnyAsync(h => h.MaHk == createDto.MaHk))
                throw new KeyNotFoundException("Mã học kỳ không tồn tại.");

            var lopHocPhan = _mapper.Map<LopHocPhan>(createDto);
            lopHocPhan.SiSoThucTe = 0; // Mới tạo thì chưa có ai
            lopHocPhan.TrangThai = "Đang mở";

            _context.LopHocPhans.Add(lopHocPhan);
            await _context.SaveChangesAsync();

            // Load lại MonHoc để map tên ra DTO trả về
            await _context.Entry(lopHocPhan).Reference(l => l.MonHoc).LoadAsync();
            return _mapper.Map<LopHocPhanDto>(lopHocPhan);
        }

        public async Task UpdateAsync(string maLhp, LopHocPhanUpdateDto updateDto)
        {
            var lopHocPhan = await _context.LopHocPhans.FindAsync(maLhp);
            if (lopHocPhan == null) throw new KeyNotFoundException("Lớp học phần không tồn tại.");

            // Chỉ cho phép cập nhật Tên, Sĩ số max, Trạng thái
            _mapper.Map(updateDto, lopHocPhan);

            if (updateDto.SiSoThucTe.HasValue)
            {
                lopHocPhan.SiSoThucTe = updateDto.SiSoThucTe.Value;
            }

            _context.LopHocPhans.Update(lopHocPhan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string maLhp)
        {
            // Dùng Transaction để đảm bảo xóa sạch sẽ hoặc không xóa gì cả
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm lớp học phần và nạp luôn danh sách đăng ký
                var lopHocPhan = await _context.LopHocPhans
                    .Include(l => l.DangKyHocPhans) // Lấy luôn danh sách SV đăng ký
                    .FirstOrDefaultAsync(l => l.MaLhp == maLhp);

                if (lopHocPhan == null) throw new KeyNotFoundException("Lớp học phần không tồn tại.");

                // 2. XỬ LÝ CÁC SINH VIÊN ĐANG ĐĂNG KÝ (Nếu có)
                if (lopHocPhan.DangKyHocPhans != null && lopHocPhan.DangKyHocPhans.Count > 0)
                {
                    // Lặp qua từng bản ghi đăng ký để xử lý Hóa đơn
                    foreach (var dangKy in lopHocPhan.DangKyHocPhans.ToList())
                    {
                        // Tìm Hóa đơn của sinh viên đó trong học kỳ này
                        var hoaDon = await _context.HoaDons
                            .Include(hd => hd.ChiTietHoaDons)
                            .FirstOrDefaultAsync(hd => hd.MaSv == dangKy.MaSv && hd.MaHk == dangKy.MaHk);

                        if (hoaDon != null)
                        {
                            // Tìm dòng chi tiết hóa đơn tương ứng với lớp này để trừ tiền
                            // (Dựa vào nội dung hoặc logic nào đó. Ở đây ta tìm theo tên lớp trong nội dung)
                            var chiTietToRemove = hoaDon.ChiTietHoaDons
                                .FirstOrDefault(ct => ct.NoiDung.Contains(lopHocPhan.TenLhp));

                            if (chiTietToRemove != null)
                            {
                                // Trừ tiền
                                hoaDon.TongTien -= chiTietToRemove.SoTien;
                                if (hoaDon.TongTien < 0) hoaDon.TongTien = 0;

                                // Xóa dòng chi tiết
                                _context.ChiTietHoaDons.Remove(chiTietToRemove);
                            }

                            // Nếu hóa đơn hết tiền và hết chi tiết -> Xóa luôn hóa đơn cho gọn
                            if (hoaDon.ChiTietHoaDons.Count == 0)
                            {
                                _context.HoaDons.Remove(hoaDon);
                            }
                            else
                            {
                                _context.HoaDons.Update(hoaDon);
                            }
                        }

                        // Xóa bản ghi đăng ký
                        _context.DangKyHocPhans.Remove(dangKy);
                    }
                }

                // 3. Xóa Lớp học phần
                _context.LopHocPhans.Remove(lopHocPhan);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
