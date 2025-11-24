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
            var lopHocPhan = await _context.LopHocPhans.FindAsync(maLhp);
            if (lopHocPhan == null) throw new KeyNotFoundException("Lớp học phần không tồn tại.");

            // Kiểm tra ràng buộc: Nếu đã có sinh viên đăng ký thì không cho xóa
            bool hasStudents = await _context.DangKyHocPhans.AnyAsync(dk => dk.MaLhp == maLhp);
            if (hasStudents)
            {
                throw new Exception("Không thể xóa lớp này vì đã có sinh viên đăng ký.");
            }

            _context.LopHocPhans.Remove(lopHocPhan);
            await _context.SaveChangesAsync();
        }
    }
}
