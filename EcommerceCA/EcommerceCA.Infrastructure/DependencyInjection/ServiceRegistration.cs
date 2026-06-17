using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Application.Mappings;
using EcommerceCA.Application.Services;
using EcommerceCA.Application.Validators.Auth;
using EcommerceCA.Common.Helpers;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Infrastructure.Data;
using EcommerceCA.Infrastructure.ExternalServices;
using EcommerceCA.Infrastructure.Repositories.Implementations.Auth;
using EcommerceCA.Infrastructure.Repositories.Implementations.Cart;
using EcommerceCA.Infrastructure.Repositories.Implementations.Category;
using EcommerceCA.Infrastructure.Repositories.Implementations.Order;
using EcommerceCA.Infrastructure.Repositories.Implementations.Payment;
using EcommerceCA.Infrastructure.Repositories.Implementations.Product;
using EcommerceCA.Infrastructure.Repositories.Implementations.User;
using EcommerceCA.Infrastructure.Repositories.Implementations.Wishlist;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService = EcommerceCA.Application.Services.UserService;

namespace EcommerceCA.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        var conn     = config.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(conn)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
            
        return services;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(o =>
        {
            o.Password.RequireDigit           = true;
            o.Password.RequireLowercase       = true;
            o.Password.RequireUppercase       = true;
            o.Password.RequireNonAlphanumeric = false;
            o.Password.RequiredLength         = 8;
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
            o.User.RequireUniqueEmail         = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        return services;
    }

    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        return services;
    }

    public static IServiceCollection AddHelpers(this IServiceCollection services)
    {
        services.AddSingleton<JwtHelper>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService,     AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService,  ProductService>();
        services.AddScoped<ICartService,     CartService>();
        services.AddScoped<IOrderService,    OrderService>();
        services.AddScoped<IWishlistService, WishlistService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService,  Application.Services.PaymentService>();
        services.AddScoped<IImageService,    ImageService>();
        services.AddScoped<IWhatsAppService, WhatsAppService>();
        services.AddScoped<IEmailService,    EmailService>();
        services.AddScoped<IUserService,     UserService>();
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICategoryRepository,     CategoryRepository>();
        services.AddScoped<IProductRepository,      ProductRepository>();
        services.AddScoped<ICartRepository,         CartRepository>();
        services.AddScoped<IOrderRepository,        OrderRepository>();
        services.AddScoped<IPaymentRepository,      PaymentRepository>();
        services.AddScoped<IUserRepository,         UserRepository>();
        services.AddScoped<IWishlistRepository,     WishlistRepository>();
        return services;
    }
}
