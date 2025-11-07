using AdminRazer.Models;
using AutoMapper;
using Web.Api.DTOs;

namespace Web.Api.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 🔹 Cliente
            CreateMap<Cliente, ClienteDto>();
            CreateMap<ClienteCreateDto, Cliente>();

            // 🔹 Producto
            CreateMap<Producto, ProductoDto>();
            CreateMap<ProductoCreateDto, Producto>();

            // 🔹 DetalleVenta
            CreateMap<DetalleVenta, DetalleVentaDto>()
                .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto.Nombre));
            CreateMap<DetalleVentaCreateDto, DetalleVenta>();

            // 🔹 Venta
            CreateMap<Venta, VentaDto>()
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Cliente.Nombre))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));
            CreateMap<VentaCreateDto, Venta>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));
        }
    }
}