using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using WebApp.BusinessLogic.Helpers;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.BusinessLogic.Validation;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;
using WebApp.DataAccess.Helpers;
using WebApp.DataAccess.Interfaces;

namespace WebApp.BusinessLogic.Services;
public class UserService : IUserService
{
    private readonly IUser userRepository;
    private readonly IRole roleRepository;
    private readonly IMapper mapper;
    private readonly IWebHostEnvironment appEnvironment;
    private readonly IHttpContextAccessor httpContextAccessor;

    public UserService(IUser userRepository, IRole roleRepository, IMapper mapper, IWebHostEnvironment appEnvironment, IHttpContextAccessor httpContextAccessor)
    {
        this.userRepository = userRepository;
        this.roleRepository = roleRepository;
        this.mapper = mapper;
        this.appEnvironment = appEnvironment;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserModel> AddAsync(UserModel model)
    {
        var user = this.mapper.Map<User>(model);
        await this.userRepository.AddAsync(user);
        return this.mapper.Map<UserModel>(user);
    }

    public async Task DeleteAsync(int modelId)
    {
        var existUser = await this.userRepository.GetByIdAsync(modelId);
        if (existUser == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (existUser.Image != null && existUser.Image != "/userProfileImages/defaultUser.jpg" && File.Exists(this.appEnvironment.WebRootPath + existUser.Image))
        {
            File.Delete(this.appEnvironment.WebRootPath + existUser.Image);
        }

        await this.userRepository.DeleteByIdAsync(modelId);
    }

    public async Task<IEnumerable<UserModel>> GetAllAsync()
    {
        var users = await this.userRepository.GetAllAsync();
        return this.mapper.Map<IEnumerable<UserModel>>(users);
    }

    public async Task<TokenResponseModel> LoginAsync(UserLoginModel userModel)
    {
        if (!await this.userRepository.IsUserExistsAsync(userModel?.Email!))
        {
            throw new UnauthorizedAccessException("User does not exist.");
        }

        string? salt = await this.userRepository.GetUserSaltAsync(userModel?.Email!);
        if (salt == null)
        {
            throw new ArgumentException("Invalid user data: salt is missing.");
        }

        string hashedPassword = SecurityHelper.HashPassword(userModel!.Password, salt, 10101, 70);
        if (await this.userRepository.VerifyUserAsync(new User
        {
            Email = userModel.Email,
            HashedPasssword = hashedPassword,
        }))
        {
            var foundUser = await this.userRepository.GetUserByEmailAsync(userModel.Email);
            return await this.GenerateTokensAsync(foundUser!.Id, foundUser.Email, foundUser!.Role!.Name);
        }
        else
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
    }

    public async Task<TokenResponseModel> RefreshTokenAsync(int userId, string refreshToken)
    {
        bool isValid = await this.userRepository.IsRefreshTokenValidAsync(userId, refreshToken);

        if (!isValid)
        {
            throw new UnauthorizedAccessException();
        }

        var foundUser = await this.userRepository.GetByIdAsync(userId);
        return await this.GenerateTokensAsync(foundUser!.Id, foundUser.Email, foundUser!.Role!.Name);
    }

    public async Task DeleteRefreshTokenAsync(int userId)
    {
        var currentUser = await this.userRepository.GetByIdAsync(userId);

        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User does not exist.");
        }

        await this.userRepository.DeleteRefreshTokenAsync(userId);
    }

    public async Task<bool> RegisterUserAsync(UserRegisterModel userModel, string roleName = "User")
    {
        if (await this.userRepository.IsUserExistsAsync(userModel?.Email!))
        {
            return false;
        }

        string salt = SecurityHelper.GenerateSalt(70);
        string hashedPassword = SecurityHelper.HashPassword(userModel!.Password, salt, 10101, 70);

        User newUser = this.mapper.Map<User>(userModel);
        newUser.Salt = salt;
        newUser.HashedPasssword = hashedPassword;

        var userRole = await this.roleRepository.FindByNameAsync(roleName);
        if (userRole == null)
        {
            throw new RoleNotFoundException(roleName);
        }

        newUser.RoleId = userRole.Id;

        await this.userRepository.AddAsync(newUser);
        return true;
    }

    public async Task<UserModel> GetByIdAsync(int id)
    {
        var user = await this.userRepository.GetByIdAsync(id);
        var userModel = this.mapper.Map<UserModel>(user);
        userModel.Image = $"{this.httpContextAccessor.HttpContext!.Request.Scheme}://{this.httpContextAccessor.HttpContext!.Request.Host}/{user?.Image}";
        return userModel;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await this.userRepository.GetUserByEmailAsync(email);
    }

    public async Task UpdateAsync(UserModel model)
    {
        var existUser = await this.userRepository.GetByIdAsync(model?.Id ?? 0);
        if (existUser == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        existUser.FirstName = model!.FirstName;
        existUser.LastName = model.LastName;
        if (model.File != null)
        {
            if (existUser.Image != null && existUser.Image != "/userProfileImages/defaultUser.jpg" && File.Exists(this.appEnvironment.WebRootPath + existUser.Image))
            {
                File.Delete(this.appEnvironment.WebRootPath + existUser.Image);
            }

            string fileName = model.File.FileName;
            string filePath = FileService.СreateFilePathFromFileName(fileName, foldername: "userProfileImages");
            existUser.Image = filePath;
            await FileService.SaveFile(filePath, model.File, this.appEnvironment);
        }

        await this.userRepository.UpdateAsync(existUser);
    }

    public async Task<UserModel> GetUserByRefreshTokenAsync(string refreshToken)
    {
        var existUser = await this.userRepository.GetUserByRefreshTokenAsync(refreshToken);

        if (existUser == null)
        {
            throw new UnauthorizedAccessException();
        }

        return this.mapper.Map<UserModel>(existUser);
    }

    public UserModelPagination GetAllUsersWithDetails(PaginationParamsFromQueryModel paginationParams)
    {
        var pageListUser = this.userRepository.GetUsersPaged(new QueryOptions
        {
            CurrentPage = (paginationParams?.Page ?? 0) + 1,
            PageSize = paginationParams?.PerPage ?? 0,
            SearchPropertyName = paginationParams!.SearchPropertyName!,
            SearchTerm = paginationParams.SearchTerm!,
        });

        return this.mapper.Map<UserModelPagination>(pageListUser);
    }

    public async Task UpdateUserRoleAsync(int userId, int userRoleId)
    {
        var existUser = await this.userRepository.GetByIdAsync(userId);
        if (existUser == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var existRole = await this.roleRepository.GetByIdAsync(userRoleId);
        if (existRole == null)
        {
            throw new InvalidOperationException("Role not found.");
        }

        existUser.Role = existRole;
        await this.userRepository.UpdateAsync(existUser);
    }

    private async Task<TokenResponseModel> GenerateTokensAsync(int userId, string userEmail, string userRole)
    {
        var claims = new List<Claim>
            {
               new Claim(ClaimTypes.NameIdentifier, userId.ToString(CultureInfo.CurrentCulture)),
               new Claim(ClaimTypes.Email, userEmail),
               new Claim(ClaimTypes.Role, userRole),
            };
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

        var newRefreshToken = TokenHelper.GenerateRefreshToken();
        var newExpiryDate = DateTime.UtcNow.AddHours(1);

        await this.userRepository.StoreRefreshTokenAsync(userId, newRefreshToken, newExpiryDate);

        return new TokenResponseModel
        {
            AccesToken = new JwtSecurityTokenHandler().WriteToken(jwt),
            RefreshToken = newRefreshToken,
        };
    }
}
