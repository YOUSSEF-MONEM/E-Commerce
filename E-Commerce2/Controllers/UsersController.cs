using E_Commerce2.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryPatternWithUnitOfWork.Core;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Users.Constants;
using Users.DTOs;
using Users.Entities;

namespace E_Commerce2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


  
        /// انا كمستخدم بعمل لجن او رجستر فابيرجع لي التوكن مع ال Id بتاعي تمام طيب المفروض ان انا محتاج اجيب البيانات الاساسيه بتاعت كل مستحدم واحطها في حقوول تبع تعديل البيانات 
        /// طيب يعني انا هستخدم الاكشن ده عشان اجيب بياناتي عشان اعرضها في صفحة تعديل البيانات 
        /// بس هنا هاخد ال Id من التوكن مش من باراميتر عشان كده مش محتاج باراميتر
        [HttpGet("GetUserData")]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> GetUserById()
        {
            int id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _unitOfWork.Users.ViewByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            return Ok(new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.Address,
                user.BirthDate,
                user.Age,//غير قابل للتعديل
                user.PhoneNumber, 
                user.MemberSince,//غير قابل للتعديل
                Roles = user.Roles.Select(r => r.Role.ToString())//مش يعرض
            });
        }

        [HttpGet]
        [CheckPermission(Roles.Manegar)]
        [CheckPermission(Roles.Admin)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _unitOfWork.Users.GetAllAsync();

            var usersDto = users.Select(u => new
            {
                u!.Id,
                u.FirstName,
                u.LastName,
                u.BirthDate,
                u.Age,
                u.Email,
                u.Address,
                u.PhoneNumber,
                u.MemberSince,
                Roles = u.Roles.Select(r => r.Role.ToString())
            });

            return Ok(usersDto);
        }


        //دي بتتنادا لما المستخدم يضغط علي زر حفظ في صفحة تعديل البيانات طيب المستحدم هيعدل البيانات اللي هو عايزها في الفورم ويدوس حفظ فبتتبعت الداتا دي هنا عشان تتحدث في الداتا بيز
        [HttpPut()]
        [CheckPermission(Roles.User)]
        public async Task<IActionResult> Update( [FromBody] UpdateUserDto userDto) // هحط في البتن بتاع حفظ اليو ار ال بتاع الاكشن ده
        { 

            var  user =  await _unitOfWork.Users.GetByIdAsync(userDto.Id);

            if (user == null)
                 return NotFound(new { message = $"User with ID {userDto.Id} not found" });

            var resultUpdate = Users.Entities.User.Update(user, userDto);

            if(!resultUpdate.IsSuccess)
                return BadRequest(new { message = resultUpdate.Error });

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "User updated successfully"});


        }

        [HttpDelete("{id}")]
        [CheckPermission(Roles.Manegar)]
        [CheckPermission(Roles.Admin)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            await _unitOfWork.Users.DeleteAsync(user);

            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "User deleted successfully" });
        }


        //  Roles endpoints 


        [HttpPost("{userId}/roles")]
        [CheckPermission(Roles.Manegar)]
        public async Task<IActionResult> AddRole(int userId, [FromBody] AddRoleDto dto)//AddRoleDto دي هتكون بالنسبه للفرنت اند هتكون كحقل يظهر للمنجر فقط وتكون اختيارات محدده 
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = user.AddRole(dto.Role);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });


            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Role added successfully" });
            
        }

        [HttpDelete("{userId}/roles/{role}")]
        public async Task<IActionResult> RemoveRole(int userId, Roles role)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = user.RemoveRole(role);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });


            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Role removed successfully" });
            

        }

        [HttpGet("{userId}/roles")]
        [CheckPermission(Roles.Manegar)]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var user = await _unitOfWork.Users.ViewByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                userId = user.Id,
                roles = user.Roles.Select(r => r.Role.ToString())
            });
        }
    }
}


