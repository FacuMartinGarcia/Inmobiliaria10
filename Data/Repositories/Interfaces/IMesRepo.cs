using Inmobiliaria10.Models;

public interface IMesRepo
{
    Task<IReadOnlyList<Mes>> GetAllAsync(CancellationToken ct = default);
}
