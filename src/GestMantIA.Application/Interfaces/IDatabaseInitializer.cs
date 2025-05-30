namespace GestMantIA.Application.Interfaces
{
    public interface IDatabaseInitializer
    {
        Task InitializeDatabaseAsync(CancellationToken cancellationToken = default);
        Task SeedDataAsync(CancellationToken cancellationToken = default);
    }
}
