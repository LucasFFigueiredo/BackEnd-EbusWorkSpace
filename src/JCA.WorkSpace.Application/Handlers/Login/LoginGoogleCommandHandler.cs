using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using AutoMapper;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Login;
using JCA.WorkSpace.Application.Dtos.Auth;
using JCA.WorkSpace.Application.Dtos.Users;

namespace JCA.WorkSpace.Application.Handlers.Login;

public class LoginGoogleCommandHandler : IRequestHandler<LoginGoogleCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public LoginGoogleCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IConfiguration configuration, IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Handle(LoginGoogleCommand request, CancellationToken cancellationToken)
    {
        var googleClientId = _configuration["GoogleAuth:ClientId"];
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string> { googleClientId! }
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.GoogleToken, settings);
        }
        catch (Exception)
        {
            throw new UnauthorizedAccessException("Token do Google inválido ou expirado.");
        }

        var allowedDomains = _configuration.GetSection("GoogleAuth:AllowedDomains")
            .GetChildren()
            .Select(x => x.Value!)
            .ToArray();
        bool isDomainAllowed = allowedDomains.Contains(payload.HostedDomain) ||
                               allowedDomains.Any(domain => payload.Email.EndsWith($"@{domain}", StringComparison.OrdinalIgnoreCase));

        if (!isDomainAllowed)
        {
            throw new UnauthorizedAccessException("Acesso negado. Utilize um e-mail corporativo autorizado para acessar o Workspace.");
        }

        var user = await _userRepository.GetByEmailAsync(payload.Email);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = payload.Email,
                Name = payload.Name,
                Profile = UserProfile.Employee,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
        }
        else
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        await _unitOfWork.CommitAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = _mapper.Map<UserDto>(user)
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Profile.ToString())
        };

        if (!string.IsNullOrEmpty(user.Sector))
            claims.Add(new Claim("sector", user.Sector));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationInMinutes"]!)),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}