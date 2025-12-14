namespace Core.Interfaces
{
    public interface IApplicationDbContext
    {
        IIncidentRepository Incidents { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
