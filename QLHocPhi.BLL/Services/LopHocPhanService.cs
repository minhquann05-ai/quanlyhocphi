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

        private readonly IDangKyHocPhanService _dangKyService;

        public LopHocPhanService(AppDbContext context, IMapper mapper, IDangKyHocPhanService dangKyService)
        {
            _context = context;
            _mapper = mapper;
            _dangKyService = dangKyService;
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
            var list = await _context.LopHocPhans
                .Include(l => l.MonHoc)
                .Where(l => l.MonHoc.MaNganh == maNganh)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<LopHocPhanDto>>(list);
        }

        public async Task<LopHocPhanDto> CreateAsync(LopHocPhanCreateDto createDto)
        {
            if (await _context.LopHocPhans.AnyAsync(l => l.MaLhp == createDto.MaLhp))
                throw new Exception($"Mã lớp học phần {createDto.MaLhp} đã tồn tại.");

            if (!await _context.MonHocs.AnyAsync(m => m.MaMh == createDto.MaMh))
                throw new KeyNotFoundException("Mã môn học không tồn tại.");
            if (!await _context.HocKys.AnyAsync(h => h.MaHk == createDto.MaHk))
                throw new KeyNotFoundException("Mã học kỳ không tồn tại.");

            var lopHocPhan = _mapper.Map<LopHocPhan>(createDto);
            lopHocPhan.SiSoThucTe = 0;       
            lopHocPhan.TrangThai = "Đang mở";

            _context.LopHocPhans.Add(lopHocPhan);
            await _context.SaveChangesAsync();

            await _context.Entry(lopHocPhan).Reference(l => l.MonHoc).LoadAsync();
            return _mapper.Map<LopHocPhanDto>(lopHocPhan);
        }

        public async Task UpdateAsync(string maLhp, LopHocPhanUpdateDto updateDto)
        {
            var lopHocPhan = await _context.LopHocPhans.FindAsync(maLhp);
            if (lopHocPhan == null) throw new KeyNotFoundException("Lớp học phần không tồn tại.");

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

            bool hasStudents = await _context.DangKyHocPhans.AnyAsync(dk => dk.MaLhp == maLhp);
            if (hasStudents)
            {
                throw new Exception("Không thể xóa lớp này vì đã có sinh viên đăng ký.");
            }

            _context.LopHocPhans.Remove(lopHocPhan);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveAllStudentsAsync(string maLhp)
        {
                var listDangKy = await _context.DangKyHocPhans
                    .Where(dk => dk.MaLhp == maLhp)
                    .AsNoTracking() 
                    .ToListAsync();

                if (listDangKy.Count == 0) return;

                foreach (var dk in listDangKy)
                {
                    try
                    {
                        await _dangKyService.CancelRegistrationAsync(dk.MaSv, maLhp, "PhongTaiChinh");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Lỗi khi hủy sinh viên {dk.MaSv}: {ex.Message}");
                    }
                }

                var lop = await _context.LopHocPhans.FindAsync(maLhp);
                if (lop != null)
                {
                    lop.SiSoThucTe = 0;
                    _context.LopHocPhans.Update(lop);
                    await _context.SaveChangesAsync();
                }
            }
    }
}
