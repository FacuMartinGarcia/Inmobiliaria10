using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IImagenRepo
    {
        Task<int> Alta(Imagen p);
        Task<int> Baja(int id);
        Task<int> Modificacion(Imagen p);
        Task<Imagen?> ObtenerPorId(int id);
        Task<IList<Imagen>> ObtenerTodos();
        Task<IList<Imagen>> BuscarPorInmueble(int idInmueble);
    }
}