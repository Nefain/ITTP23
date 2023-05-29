using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITTP23.Models;
using ITTP23.Storage;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using ITTP23.Servise;
using NuGet.Protocol.Plugins;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

namespace ITTP23.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly AutoDataContext _context;
        readonly IUserService _service;

        public UsersController(AutoDataContext context, IUserService service)
        {
            _context = context;
            _service = service;
        }

        // GET
        [HttpGet("GetToken")]
        [SwaggerOperation(Summary = "Запрос токена", Description = "Создание токена, если его нет у пользователя, если есть, то возвращает уже имеющийся токен")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<List<User>>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> GetToken(string login, string password)
        {
            try
            {         
                return Ok(await _service.GetTokenAsync(login, password));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        [HttpGet("GetAllActiveUsers")]
        [SwaggerOperation(Summary = "Запрос списка всех активных пользователей", Description = "Отсутствует RevokedOn, список отсортирован по CreatedOn (Доступно Админам)")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<List<User>>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> GetAllActiveUsers(string login, string password)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.GetAllActiveUsersAsync(editor));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }      

        [HttpGet("GetUser")]
        [SwaggerOperation(Summary = "Запрос пользователя по логину", Description = "В списке имя, пол и дата рождения статус активный или нет (Доступно Админам)")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<UserUpdateDTO>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> GetUser(string login, string password, string loginUser)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.GetUserAsync(editor, loginUser));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        [HttpGet("GetDetails")]
        [SwaggerOperation(Summary = "Запрос пользователя по логину и паролю", Description = "Доступно только самому пользователю, если он активен (отсутствует RevokedOn)")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> GetDetails(string login, string password)
        {
            try
            {
                return Ok(await _service.GetDetailsAsync(login, password));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        [HttpGet("GetAllOlderUsers")]
        [SwaggerOperation(Summary = "Запрос всех пользователей старше определённого возраста", Description = "Администратор вводит возраст и получает всех пользователей старше введенного возраста")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<List<User>>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> GetAllOlderUsers(string login, string password, int age)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.GetAllOlderUsersAsync(editor, age));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        // POST
        [HttpPost("Create")]
        [SwaggerOperation(Summary = "Создание пользователя", Description = "Создание пользователя по логину, паролю, имени, полу и дате рождения + указание будет ли пользователь админом (Доступно Админам)")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> Create(string login, string password, [FromBody] UserDTO userDTO)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.CreateAsync(editor, userDTO));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        // PUT
        [HttpPut("UpdateUser")]
        [SwaggerOperation(Summary = "Изменение имени, пола или даты рождения пользователя", Description = "Может менять Администратор, либо лично пользователь, если он активен (отсутствует RevokedOn))")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> UpdateUser(string login, string password,  string? loginUser, string name, int gender, DateTime? birthday)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.UpdateAsync(editor, loginUser, name, gender, birthday));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        [HttpPut("UpdatePassword")]
        [SwaggerOperation(Summary = "Изменение пароля", Description = "Пароль может менять либо Администратор, либо лично пользователь, если он активен (отсутствует RevokedOn)")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> UpdatePassword(string login, string password, string? loginUser, string newPassword)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.UpdatePasswordAsync(editor, loginUser, newPassword));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        [HttpPut("UpdateLogin")]
        [SwaggerOperation(Summary = "Изменение логина", Description = "Логин может менять либо Администратор, либо лично пользователь, если он активен (отсутствует RevokedOn), логин должен оставаться уникальным")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> UpdateLogin(string login, string password, string? loginUser, string newLogin)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.UpdateLoginAsync(editor, loginUser, newLogin));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        [HttpPut("UserRecovery")]
        [SwaggerOperation(Summary = "Восстановление пользователя", Description = "Очистка полей (RevokedOn, RevokedBy) (Доступно Админам)")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> UserRecovery(string login, string password, string loginUser)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.UserRecoveryAsync(editor, loginUser));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        [HttpPut("DeleteSoft")]
        [SwaggerOperation(Summary = "Мягкое удаление(Блокировка)", Description = "Происходит простановка RevokedOn и RevokedBy")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> DeleteSoft(string login, string password, string loginUser)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.DeleteSoftAsync(editor, loginUser));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        // DELETE
        [HttpDelete("DeleteHard")]
        [SwaggerOperation(Summary = "Полное удаление", Description = "Пользователь полностью удаляется из базы данных")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> DeleteHard(string login, string password, string loginUser)
        {
            try
            {
                var editor = _context.Users.FirstOrDefault(i => i.Login == login && i.Password == password);
                return Ok(await _service.DeleteHardAsync(editor, loginUser));
            }
            catch (Exception ex)
            {
               return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }
    }
}
