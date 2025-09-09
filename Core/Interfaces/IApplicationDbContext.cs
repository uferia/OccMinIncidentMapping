namespace Core.Interfaces
{
    public interface IApplicationDbContext
    {
        IIncidentRepository Incidents { get; }
    }
}
