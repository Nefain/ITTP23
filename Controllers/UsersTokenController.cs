using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITTP23.Models;
using ITTP23.Storage;
using ITTP23.Servise;
using Swashbuckle.AspNetCore.Annotations;

namespace ITTP23.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersTokenController : Controller
    {
        private readonly AutoDataContext _context;
        readonly IUserService _service;
        const string _token = "Authorization";

        public UsersTokenController(AutoDataContext context, IUserService service)
        {
            _context = context;
            _service = service;
        }

        // READ

        [HttpGet("GetAllActiveUsers")]
        [SwaggerOperation(Summary = "Запрос списка всех активных пользователей", Description = "Отсутствует RevokedOn, список отсортирован по CreatedOn (Доступно Админам)")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<List<User>>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> GetAllActiveUsers()
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
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
        public async Task<IActionResult> GetUser(string loginUser)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация."); 
                }
                var editor = await _service.LoginByTokenAsync(value);
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
        public async Task<IActionResult> GetDetails()
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
                return Ok(await _service.GetDetailsAsync(editor.Login, editor.Password));
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
        public async Task<IActionResult> GetAllOlderUsers(int age)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
                return Ok(await _service.GetAllOlderUsersAsync(editor, age));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        // CREATE

        [HttpPost("Create")]
        [SwaggerOperation(Summary = "Создание пользователя", Description = "Создание пользователя по логину, паролю, имени, полу и дате рождения + указание будет ли пользователь админом (Доступно Админам)")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> Create([FromBody] UserDTO userDTO)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
                return Ok(await _service.CreateAsync(editor, userDTO));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        // UPDATE - 1

        [HttpPut("UpdateUser")]
        [SwaggerOperation(Summary = "Изменение имени, пола или даты рождения пользователя", Description = "Может менять Администратор, либо лично пользователь, если он активен (отсутствует RevokedOn))")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> UpdateUser(string? loginUser, string name, int gender, DateTime? birthday)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
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
        public async Task<IActionResult> UpdatePassword(string? loginUser, string newPassword)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
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
        public async Task<IActionResult> UpdateLogin(string? loginUser, string newLogin)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
                return Ok(await _service.UpdateLoginAsync(editor, loginUser, newLogin));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        // UPDATE - 2

        [HttpPut("UserRecovery")]
        [SwaggerOperation(Summary = "Восстановление пользователя", Description = "Очистка полей (RevokedOn, RevokedBy) (Доступно Админам)")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> UserRecovery(string loginUser)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
                return Ok(await _service.UserRecoveryAsync(editor, loginUser));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        // DELETE

        [HttpPut("DeleteSoft")]
        [SwaggerOperation(Summary = "Мягкое удаление(Блокировка)", Description = "Происходит простановка RevokedOn и RevokedBy")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> DeleteSoft(string loginUser)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
                return Ok(await _service.DeleteSoftAsync(editor, loginUser));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

        [HttpDelete("DeleteHard")]
        [SwaggerOperation(Summary = "Полное удаление", Description = "Пользователь полностью удаляется из базы данных")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<User>))]
        [SwaggerResponse(400, "Bad Request", typeof(MessageError))]
        public async Task<IActionResult> DeleteHard(string loginUser)
        {
            try
            {
                if (!Request.Headers.TryGetValue(_token, out var value))
                {
                    throw new Exception("Ошибка в получении токена. Необходима авторизация.");
                }
                var editor = await _service.LoginByTokenAsync(value);
                return Ok(await _service.DeleteHardAsync(editor, loginUser));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageError() { Code = BadRequest().StatusCode, Message = ex.Message, Type = ex.GetType().Name }); //Возвращение ошибки
            }
        }

    }
}
