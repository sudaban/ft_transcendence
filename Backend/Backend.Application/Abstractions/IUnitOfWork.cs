namespace Backend.Application.Abstractions;

public interface IUnitOfWork : IDisposable
{
    Task<int> CommitAsync();
}