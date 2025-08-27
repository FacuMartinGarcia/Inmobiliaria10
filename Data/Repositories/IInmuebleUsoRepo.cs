using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IInmuebleUsoRepo
    {
        List<InmuebleUso> MostrarTodosInmuebleUsos();
        int Agregar(InmuebleUso inmuebleUso);
    }
}