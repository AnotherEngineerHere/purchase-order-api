using AutoMapper;
using PurchaseOrderApi.DTOs.Customers;
using PurchaseOrderApi.DTOs.Orders;
using PurchaseOrderApi.DTOs.Products;
using PurchaseOrderApi.Models;

namespace PurchaseOrderApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        // Customer
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerDto, Customer>();

        // Order
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName));

        // OrderItem
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
    }
}
