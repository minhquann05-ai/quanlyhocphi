using QLHocPhi.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Interfaces
{
    public interface INguoiDungService
    {
        Task<UserDto> LoginAsync(LoginDto loginDto);
        Task ChangePasswordAsync(ChangePasswordDto dto);
        Task<int> GenerateAccountsForStudentsAsync(); // Trả về số lượng tài khoản đã tạo
        Task CreateDefaultAdminAsync();
    }
}
