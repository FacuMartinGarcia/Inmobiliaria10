using MySql.Data.MySqlClient;
public abstract class RepositorioBase
{
    private readonly string _cs;

    protected RepositorioBase(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("Inmogenial")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:Inmogenial");
    }

    protected MySqlConnection Conn() => new MySqlConnection(_cs);
}
