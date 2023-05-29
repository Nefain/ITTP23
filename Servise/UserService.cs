using ITTP23.Models;
using ITTP23.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Plugins;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ITTP23.Servise
{
    public class UserService : IUserService
    {
        private readonly AutoDataContext _context;
        public UserService(AutoDataContext context) 
        { 
            _context = context;
        }

        public async Task<User> CreateAsync(User editor, UserDTO newUser)
        {
            if(editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if(newUser == null)
                throw new Exception("Попытка создать пустого пользователя");

            if(_context.Users.FirstOrDefault(i => i.Login == newUser.Login) != null)
                throw new Exception("Попытка создать уже существуещего пользователя");

            if (editor.Admin == false)
                throw new Exception("Только администратор может выполнить данную операцию");
            
            Regex regLogin_Password = new Regex(@"^[a-zA-Z0-9]+$");
            if (regLogin_Password.Match(newUser.Login).Success == false)
                throw new Exception("Логин не соответсувует требованиям");

            if (regLogin_Password.Match(newUser.Password).Success == false)
                throw new Exception("Пароль не соответсувует требованиям");

            Regex regName = new Regex(@"^[a-zA-Zа-яА-Я]+$");
            if (regName.Match(newUser.Name).Success == false)
                throw new Exception("Имя не соответсувует требованиям");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = newUser.Login,
                Password = newUser.Password,
                Name = newUser.Name,
                Gender = newUser.Gender,
                Birthday = newUser.Birthday,
                Admin = newUser.Admin,

                CreatedOn = DateTime.UtcNow,
                CreatedBy = editor.Login,

                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = editor.Login
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> DeleteHardAsync(User editor, string login)
        {
            if (editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (editor.Admin == false)
                throw new Exception("Только администратор может выполнить данную операцию");

            var user = _context.Users.FirstOrDefault(i => i.Login == login);
            if (user == null)
                throw new Exception("Попытка удалить несуществуещего пользователя");

            _context.Users.Remove(user);
            _context.SaveChanges();

            return user;
        }

        public async Task<User> DeleteSoftAsync(User editor, string login)
        {
            if (editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (editor.Admin == false)
                throw new Exception("Только администратор может выполнить данную операцию");

            var user = _context.Users.FirstOrDefault(i => i.Login == login);
            if (user == null)
                throw new Exception("Попытка заблокировать несуществуещего пользователя");

            user.RevokedOn = DateTime.Now;
            user.RevokedBy = editor.Login;

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }

        public async Task<IEnumerable<User>> GetAllActiveUsersAsync(User editor)
        {
            if (editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (editor.Admin == false)
                throw new Exception("Только администратор может выполнить данную операцию");

            return await _context.Users.Where(i => i.RevokedOn == null).OrderBy(e => e.CreatedOn).ToListAsync();      
        }

        public async Task<IEnumerable<User>> GetAllOlderUsersAsync(User editor, int age)
        {
            if (editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (editor.Admin == false)
                throw new Exception("Только администратор может выполнить данную операцию");

            DateTime currentDate = DateTime.Now;
            DateTime targetDate = currentDate.AddYears(-age);

            return await _context.Users.Where(u => u.Birthday.HasValue && u.Birthday.Value < targetDate).ToListAsync();
        }

        public async Task<User?> GetDetailsAsync(string login, string password)
        {
            var user = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
            if (user == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (user.RevokedOn != null)
                throw new Exception("Данный пользователь находится в блокировке");

            return user;
        }

        public async Task<string> GetTokenAsync(string login, string password)
        {
            var user = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
            if (user == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (string.IsNullOrEmpty(user.Token))
            {
                byte[] time = BitConverter.GetBytes(user.CreatedOn.ToBinary());
                string token = Convert.ToBase64String(time.Concat(user.Id.ToByteArray()).ToArray());
                user.Token = token;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            return user.Token;    
        }

        public async Task<UserUpdateDTO> GetUserAsync(User editor, string login)
        {
            if (editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (editor.Admin == false)
                throw new Exception("Только администратор может выполнить данную операцию");

            var user = _context.Users.FirstOrDefault(i => i.Login == login);
            if (user == null)
                throw new Exception("Попытка получить информацию о не существуещем пользователе");
            
            UserUpdateDTO userDTO = new UserUpdateDTO();

            userDTO.IsActive = user.RevokedOn == null;
            userDTO.Name = user.Name;
            userDTO.Birthday = user.Birthday;
            userDTO.Gender = user.Gender;

            return userDTO;
        }

        public async Task<User> LoginByTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<User> UpdateAsync(User editor, string? login, string name, int? gender, DateTime? birthday)
        {
            if (editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (editor.Admin == true)
            { 
                if(login == String.Empty)
                    throw new Exception("Не указан логин изменяемого пользователя");

                var user = _context.Users.FirstOrDefault(i => i.Login == login);
                if (user == null)
                    throw new Exception("Такого пользователя не существует");

                if (user.RevokedOn != null)
                    throw new Exception("Данный пользователь находится в блокировке");
            }

            if (editor.RevokedOn != null)
                throw new Exception("Данный пользователь находится в блокировке");

            Regex regName = new Regex(@"^[a-zA-Zа-яА-Я]+$");

            if (!string.IsNullOrEmpty(name)) 
                if (regName.Match(name).Success == false)
                    throw new Exception("Имя не соответсувует требованиям");

            if (gender.HasValue)
                if (gender < 0 || gender > 2)
                    throw new Exception("Гендер не соответсувует требованиям");
            
            if (editor.Admin == true)
            {
                var user = _context.Users.FirstOrDefault(i => i.Login == login);
                if (birthday.HasValue)
                    user.Birthday = birthday;

                if (gender.HasValue)
                    user.Gender = gender.Value;

                if (!string.IsNullOrEmpty(name))
                    user.Name = name;

                user.ModifiedBy = editor.Login;
                user.ModifiedOn = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return user;
            }

            if (birthday.HasValue)
                editor.Birthday = birthday;

            if (gender.HasValue)
                editor.Gender = gender.Value;

            if (!string.IsNullOrEmpty(name))
                editor.Name = name;

            editor.ModifiedBy = editor.Login;
            editor.ModifiedOn = DateTime.UtcNow;

            _context.Users.Update(editor);
            await _context.SaveChangesAsync();

            return editor;
        }

        public async Task<User> UpdateLoginAsync(User editor, string oldLogin, string newLogin)
        {
            if (editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (editor.Admin == true)
            {
                if (oldLogin == String.Empty)
                    throw new Exception("Не указан логин изменяемого пользователя");

                var user = _context.Users.FirstOrDefault(i => i.Login == oldLogin);
                if (user == null)
                    throw new Exception("Такого пользователя не существует");

                if (user.RevokedOn != null)
                    throw new Exception("Данный пользователь находится в блокировке");
            }

            if (editor.RevokedOn != null)
                throw new Exception("Данный пользователь находится в блокировке");

            if (_context.Users.FirstOrDefault(i => i.Login == newLogin) != null)
                throw new Exception("Попытка изменить логин на уже существующий. Логин должен быть уникальным");

            Regex regLogin_Password = new Regex(@"^[a-zA-Z0-9]+$");
            if (regLogin_Password.Match(newLogin).Success == false)
                throw new Exception("Логин не соответсувует требованиям");

            if (editor.Admin == true)
            {
                var user = _context.Users.FirstOrDefault(i => i.Login == oldLogin);
                
                user.Login = newLogin;

                user.ModifiedBy = editor.Login;
                user.ModifiedOn = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return user;
            }

            editor.Login = newLogin;

            editor.ModifiedBy = editor.Login;
            editor.ModifiedOn = DateTime.UtcNow;

            _context.Users.Update(editor);
            await _context.SaveChangesAsync();

            return editor;
        }

        public async Task<User> UpdatePasswordAsync(User editor, string login, string password)
        {
            if (editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (editor.Admin == true)
            {
                if (login == String.Empty)
                    throw new Exception("Не указан логин изменяемого пользователя");

                var user = _context.Users.FirstOrDefault(i => i.Login == login);
                if (user == null)
                    throw new Exception("Такого пользователя не существует");

                if (user.RevokedOn != null)
                    throw new Exception("Данный пользователь находится в блокировке");
            }

            if (editor.RevokedOn != null)
                throw new Exception("Данный пользователь находится в блокировке");

            Regex regLogin_Password = new Regex(@"^[a-zA-Z0-9]+$");
            if (regLogin_Password.Match(password).Success == false)
                throw new Exception("Пароль не соответсувует требованиям");

            if (editor.Admin == true)
            {
                var user = _context.Users.FirstOrDefault(i => i.Login == login);

                user.Password = password;

                user.ModifiedBy = editor.Login;
                user.ModifiedOn = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return user;
            }

            editor.Password = password;

            editor.ModifiedBy = editor.Login;
            editor.ModifiedOn = DateTime.UtcNow;

            _context.Users.Update(editor);
            await _context.SaveChangesAsync();

            return editor;
        }

        public async Task<User> UserRecoveryAsync(User editor, string login)
        {
            if (editor == null)
                throw new Exception("Логин или Пароль введен неверно");

            if (editor.Admin == false)
                throw new Exception("Только администратор может выполнить данную операцию");

            var user = _context.Users.FirstOrDefault(i => i.Login == login);
            if (user == null)
                throw new Exception("Попытка получить информацию о не существуещем пользователе");

            user.RevokedOn = null;
            user.RevokedBy = string.Empty;

            user.ModifiedBy = editor.Login;
            user.ModifiedOn = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
