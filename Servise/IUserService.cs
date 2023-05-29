using ITTP23.Models;

namespace ITTP23.Servise
{
    public interface IUserService
    {
        public Task<IEnumerable<User>> GetAllActiveUsersAsync(User editor);
        public Task<User?> GetDetailsAsync(string login, string password);
        public Task<UserUpdateDTO> GetUserAsync(User editor, string login);
        public Task<IEnumerable<User>> GetAllOlderUsersAsync(User editor, int age);

        public Task<User> DeleteSoftAsync(User editor, string login);
        public Task<User> DeleteHardAsync(User editor, string login);

        public Task<User> UserRecoveryAsync(User editor, string login);

        public Task<User> CreateAsync(User editor, UserDTO newUser);

        public Task<User> UpdateAsync(User editor, string? login, string name, int? gender, DateTime? birthday);
        public Task<User> UpdatePasswordAsync(User editor, string login, string password);
        public Task<User> UpdateLoginAsync(User editor, string oldLogin, string newLogin);

        public Task<string> GetTokenAsync(string login, string password);

        public Task<User> LoginByTokenAsync(string token);
    }
}
