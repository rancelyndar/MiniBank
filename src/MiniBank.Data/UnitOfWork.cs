namespace MiniBank.Core;

public class UnitOfWork : IUnitOfWork
{
    private readonly MiniBankContext _context;

    public UnitOfWork(MiniBankContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken token)
    {
        return _context.SaveChangesAsync(token);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }
}