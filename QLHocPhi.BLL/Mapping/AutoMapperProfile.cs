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
        .ForMember(dest => dest.TenLop, opt => opt.MapFrom(src => src.LopHoc.TenLop));
            CreateMap<SinhVienCreateDto, SinhVien>();
        }
    }
}
