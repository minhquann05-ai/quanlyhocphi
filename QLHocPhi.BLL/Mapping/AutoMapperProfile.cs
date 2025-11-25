using AutoMapper;
using QLHocPhi.Common.Dtos; 
using QLHocPhi.DAL.Entities; 

namespace QLHocPhi.BLL.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<BieuPhi, BieuPhiDto>()
                .ForMember(dest => dest.TenNganh, opt => opt.MapFrom(src => src.NganhHoc.TenNganh))
                .ForMember(dest => dest.TenHk, opt => opt.MapFrom(src => src.HocKy.TenHk));

            CreateMap<BieuPhiCreateDto, BieuPhi>();

            CreateMap<BieuPhiUpdateDto, BieuPhi>();
            CreateMap<ChiTietHoaDon, ChiTietHoaDonDto>();
            CreateMap<ChiTietHoaDonCreateDto, ChiTietHoaDon>();
            CreateMap<DangKyHocPhanCreateDto, DangKyHocPhan>();
            CreateMap<ThanhToanCreateDto, ThanhToan>();

            CreateMap<HoaDon, HoaDonDto>()
                .ForMember(dest => dest.TenSv, opt => opt.MapFrom(src => src.SinhVien.HoTen))
                .ForMember(dest => dest.TenHk, opt => opt.MapFrom(src => src.HocKy.TenHk))
                .ForMember(dest => dest.ChiTiet, opt => opt.MapFrom(src => src.ChiTietHoaDons));

            CreateMap<HoaDonCreateDto, HoaDon>()
                .ForMember(dest => dest.ChiTietHoaDons, opt => opt.Ignore());
            CreateMap<BienLai, BienLaiDto>()
                .ForMember(dest => dest.MaTt, opt => opt.MapFrom(src => src.ThanhToan.MaTt))
                .ForMember(dest => dest.SoTienThanhToan, opt => opt.MapFrom(src => src.ThanhToan.SoTienTt))
                .ForMember(dest => dest.PhuongThucThanhToan, opt => opt.MapFrom(src => src.ThanhToan.PhuongThuc))
                .ForMember(dest => dest.MaHd, opt => opt.MapFrom(src => src.ThanhToan.HoaDon.MaHd))
                .ForMember(dest => dest.TenSv, opt => opt.MapFrom(src => src.ThanhToan.HoaDon.SinhVien.HoTen));
            CreateMap<SinhVien, SinhVienDto>()
        .ForMember(dest => dest.TenLop, opt => opt.MapFrom(src => src.LopHoc != null ? src.LopHoc.TenLop : ""));
            CreateMap<SinhVienCreateDto, SinhVien>();
            CreateMap<SinhVienUpdateDto, SinhVien>();
            CreateMap<LopHocPhan, LopHocPhanDto>()
    .ForMember(dest => dest.TenMonHoc, opt => opt.MapFrom(src => src.MonHoc.TenMh))
    .ForMember(dest => dest.SoTinChi, opt => opt.MapFrom(src => src.MonHoc.SoTinChi))
            .ForMember(dest => dest.MaNganh, opt => opt.MapFrom(src => src.MonHoc.MaNganh));
            CreateMap<LopHocPhanCreateDto, LopHocPhan>();
            CreateMap<LopHocPhanUpdateDto, LopHocPhan>();
            CreateMap<DangKyHocPhan, KetQuaDangKyDto>()
    .ForMember(dest => dest.MaLhp, opt => opt.MapFrom(src => src.MaLhp))
    .ForMember(dest => dest.TenLhp, opt => opt.MapFrom(src => src.LopHocPhan.TenLhp))
    .ForMember(dest => dest.TenMonHoc, opt => opt.MapFrom(src => src.LopHocPhan.MonHoc.TenMh))
    .ForMember(dest => dest.SoTinChi, opt => opt.MapFrom(src => src.LopHocPhan.MonHoc.SoTinChi))
    .ForMember(dest => dest.NgayDangKy, opt => opt.MapFrom(src => src.NgayDk));
        }
    }
}
