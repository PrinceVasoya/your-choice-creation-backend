using EcommerceCA.Domain.Entities;
using EcommerceCA.Domain.Enums;

namespace EcommerceCA.Application.Interfaces.Repositories;

// ── Generic ───────────────────────────────────────────────────────────────────
public interface IRepository<T> where T : class
{
    Task<T?>            GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T>             AddAsync(T entity);
    Task                UpdateAsync(T entity);
    Task                DeleteAsync(T entity);
    Task<int>           SaveChangesAsync();
}

// ── Category ──────────────────────────────────────────────────────────────────
public interface ICategoryRepository : IRepository<Category>
{
    Task<bool>             NameExistsAsync(string name, int? excludeId = null);
    Task<Category?>        GetWithProductsAsync(int id);
    Task<List<Category>>   GetAllWithProductCountAsync();
}

// ── Product ───────────────────────────────────────────────────────────────────
public interface IProductRepository : IRepository<Product>
{
    Task<Product?>  GetWithDetailsAsync(int id);
    Task<(List<Product> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search, int? categoryId,
        decimal? minPrice, decimal? maxPrice,
        bool? isCustomizable, bool? inStock,
        string sortBy, string sortOrder);
    Task<ProductVariant?> GetVariantAsync(int variantId);
}

// ── Cart ──────────────────────────────────────────────────────────────────────
public interface ICartRepository
{
    Task<Cart?>  GetWithItemsAsync(string userId);
    Task<Cart>   GetOrCreateAsync(string userId);
    Task         AddItemAsync(CartItem item);
    Task         RemoveItemAsync(CartItem item);
    Task         ClearAsync(string userId);
    Task         SaveChangesAsync();
}

// ── Order ─────────────────────────────────────────────────────────────────────
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetWithDetailsAsync(int id, string? userId = null);
    Task<(List<Order> Items, int Total)> GetUserOrdersPagedAsync(string userId, int page, int pageSize);
    Task<(List<Order> Items, int Total)> GetAllOrdersPagedAsync(int page, int pageSize, OrderStatus? status);
}

// ── Payment ───────────────────────────────────────────────────────────────────
public interface IPaymentRepository
{
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task           AddAsync(Payment payment);
    Task           SaveChangesAsync();
}

// ── User / Address ────────────────────────────────────────────────────────────
public interface IUserRepository
{
    Task<List<Address>>  GetAddressesAsync(string userId);
    Task<Address?>       GetAddressAsync(int id, string userId);
    Task                 AddAddressAsync(Address address);
    Task                 RemoveAddressAsync(Address address);
    Task                 SaveChangesAsync();
}

// ── Auth / RefreshToken ───────────────────────────────────────────────────────
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetValidAsync(string token);
    Task                AddAsync(RefreshToken token);
    Task                RevokeAsync(RefreshToken token);
    Task                SaveChangesAsync();
}

// ── Wishlist ──────────────────────────────────────────────────────────────────
public interface IWishlistRepository
{
    Task<List<WishlistItem>> GetUserWishlistAsync(string userId);
    Task<WishlistItem?> GetItemAsync(string userId, int productId);
    Task AddAsync(WishlistItem item);
    Task RemoveAsync(WishlistItem item);
    Task SaveChangesAsync();
}
