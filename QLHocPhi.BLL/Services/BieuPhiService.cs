using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using QLHocPhi.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Services
{
    public class BieuPhiService : IBieuPhiService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BieuPhiService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BieuPhiDto>> GetAllAsync()
        {
            var bieuPhis = await _context.BieuPhis
                                .Include(bp => bp.NganhHoc) 
                                .Include(bp => bp.HocKy)   
                                .AsNoTracking() 
                                .ToListAsync();
            return _mapper.Map<IEnumerable<BieuPhiDto>>(bieuPhis);
        }

        public async Task<IEnumerable<BieuPhiDto>> GetByNganhAsync(string maNganh)
        {
            var bieuPhis = await _context.BieuPhis
                                .Where(bp => bp.MaNganh == maNganh) 
                                .Include(bp => bp.NganhHoc)
                                .Include(bp => bp.HocKy)
                                .AsNoTracking()
                                .ToListAsync();

            if (bieuPhis == null || !bieuPhis.Any())
            {
                return new List<BieuPhiDto>();
            }

            return _mapper.Map<IEnumerable<BieuPhiDto>>(bieuPhis);
        }

        public async Task<BieuPhiDto> CreateAsync(BieuPhiCreateDto createDto)
        {
            var existing = await _context.BieuPhis
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    bp => bp.MaNganh == createDto.MaNganh && bp.MaHk == createDto.MaHk);

            if (existing != null)
            {
                throw new System.Exception("Biểu phí cho ngành và học kỳ này đã tồn tại.");
            }

            var bieuPhi = _mapper.Map<BieuPhi>(createDto);

            var allMaBps = await _context.BieuPhis
                                .Select(bp => bp.MaBp)
                                .AsNoTracking()
                                .ToListAsync();

            int maxId = 0;

            foreach (var maBp in allMaBps)
            {
                if (maBp != null && maBp.StartsWith("BP"))
                {
                    string numberPart = maBp.Substring(2);

                    if (int.TryParse(numberPart, out int id))
                    {
                        if (id > maxId)
                        {
                            maxId = id; 
                        }
                    }
                }
            }

            int nextIdNumber = maxId + 1; 

            bieuPhi.MaBp = $"BP{nextIdNumber:D4}";

            _context.BieuPhis.Add(bieuPhi);
            await _context.SaveChangesAsync();

            var nganhHoc = await _context.NganhHocs.FindAsync(createDto.MaNganh);
            var hocKy = await _context.HocKys.FindAsync(createDto.MaHk);

            return new BieuPhiDto
            {
                MaBp = bieuPhi.MaBp,
                MaNganh = bieuPhi.MaNganh,
                TenNganh = nganhHoc?.TenNganh,
                MaHk = bieuPhi.MaHk,
                TenHk = hocKy?.TenHk,
                DonGiaTinChi = bieuPhi.DonGiaTinChi
            };
        }

        public async Task UpdateAsync(string maBp, BieuPhiUpdateDto updateDto)
        {
            var bieuPhi = await _context.BieuPhis.FindAsync(maBp);
            if (bieuPhi == null)
            {
                throw new KeyNotFoundException("Không tìm thấy biểu phí.");
            }

            bieuPhi.DonGiaTinChi = updateDto.DonGiaTinChi;

            _context.BieuPhis.Update(bieuPhi);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string maBp)
        {
            var bieuPhi = await _context.BieuPhis.FindAsync(maBp);
            if (bieuPhi == null)
            {
                throw new KeyNotFoundException("Không tìm thấy biểu phí.");
            }

            _context.BieuPhis.Remove(bieuPhi);
            await _context.SaveChangesAsync();
        }
    }
}
