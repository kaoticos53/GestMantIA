using AutoMapper;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Shared.Identity.DTOs;

namespace GestMantIA.Infrastructure.Mappings
{
    /// <summary>
    /// Perfil de AutoMapper para la gestión de roles.
    /// </summary>
    public class RoleProfileMapping : Profile
    {
        public RoleProfileMapping()
        {
            // Mapeo de ApplicationRole a RoleDTO
            CreateMap<ApplicationRole, RoleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.Permissions, opt => opt.Ignore()); // Se llenará manualmente

            // Mapeo de CreateRoleDTO a ApplicationRole
            CreateMap<CreateRoleDTO, ApplicationRole>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpperInvariant()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // Mapeo de UpdateRoleDTO a ApplicationRole
            CreateMap<UpdateRoleDTO, ApplicationRole>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpperInvariant()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
