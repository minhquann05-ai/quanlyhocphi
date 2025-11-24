using QLHocPhi.Common.Dtos;

namespace QLHocPhi.BLL.Interfaces
{
    public interface IBienLaiService
    {
        Task<byte[]> GenerateBienLaiPdfAsync(string maHd);
        Task<IEnumerable<BienLaiDto>> GetAllAsync(); 
        Task<IEnumerable<BienLaiDto>> GetByMaSvAsync(string maSv);
        Task<int> SyncMissingBienLaiAsync();
    }
}
