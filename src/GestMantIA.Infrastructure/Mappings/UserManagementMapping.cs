using AutoMapper;
using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Infrastructure.Mappings
{
    /// <summary>
    /// Perfil de AutoMapper para la gestión de usuarios.
    /// </summary>
    public class UserManagementMapping : Profile
    {
        public UserManagementMapping()
        {
            // Mapeo de CreateUserDTO a ApplicationUser
            CreateMap<GestMantIA.Shared.Identity.DTOs.Requests.CreateUserDTO, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => !src.RequireEmailConfirmation))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            // Mapeo de UpdateUserDTO a ApplicationUser
            CreateMap<GestMantIA.Shared.Identity.DTOs.Requests.UpdateUserDTO, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(d => d.EmailConfirmed, o => o.MapFrom((src, dest) => src.EmailConfirmed ?? dest.EmailConfirmed))
                .ForMember(d => d.PhoneNumberConfirmed, o => o.MapFrom((src, dest) => src.PhoneNumberConfirmed ?? dest.PhoneNumberConfirmed))
                .ForMember(d => d.TwoFactorEnabled, o => o.MapFrom((src, dest) => src.TwoFactorEnabled ?? dest.TwoFactorEnabled))
                .ForMember(d => d.IsActive, o => o.MapFrom((src, dest) => src.IsActive ?? dest.IsActive))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // Mapeo de ApplicationUser a UserResponseDTO
            CreateMap<ApplicationUser, GestMantIA.Shared.Identity.DTOs.Responses.UserResponseDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.DateRegistered, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.IsLockedOut, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())

                .ForMember(dest => dest.Roles, opt => opt.Ignore());
        }
    }
}
