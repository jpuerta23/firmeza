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

            // 🔹 DetalleVenta → DetalleVentaDto (mostrar)
            CreateMap<DetalleVenta, DetalleVentaDto>()
                .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto.Nombre))
                // El Subtotal se calcula automáticamente en el DTO (propiedad => Cantidad * PrecioUnitario)
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore());

            // 🔹 DetalleVentaCreateDto → DetalleVenta (crear)
            CreateMap<DetalleVentaCreateSimpleDto, DetalleVenta>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Venta, opt => opt.Ignore())
                .ForMember(dest => dest.VentaId, opt => opt.Ignore())
                .ForMember(dest => dest.Producto, opt => opt.Ignore())
                .ForMember(dest => dest.PrecioUnitario, opt => opt.Ignore()) // se definirá en el controlador a partir del producto
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore());

            // 🔹 Venta → VentaDto (mostrar)
            CreateMap<Venta, VentaDto>()
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Cliente.Nombre))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));

            // 🔹 VentaCreateDto → Venta (crear)
            CreateMap<VentaCreateDto, Venta>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Total, opt => opt.Ignore())
                .ForMember(dest => dest.Cliente, opt => opt.Ignore())
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));
        }
    }
}
