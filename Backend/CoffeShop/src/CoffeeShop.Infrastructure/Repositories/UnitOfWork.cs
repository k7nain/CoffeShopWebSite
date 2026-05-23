using CoffeeShop.Domain.Interfaces;
using CoffeeShop.Infrastructure.Persistence;

namespace CoffeeShop.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(
        ApplicationDbContext context,
        IUserRepository users,
        IProductRepository products,
        IOrderRepository orders)
    {
        _context = context;
        Users = users;
        Products = products;
        Orders = orders;
    }

    public IUserRepository Users { get; }
    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
