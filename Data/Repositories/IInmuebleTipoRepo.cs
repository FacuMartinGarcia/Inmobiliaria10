
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IInmuebleTipoRepo
    {
        public List<InmuebleTipo> MostrarTodosInmuebleTipos();
        public int Agregar(InmuebleTipo i);
    }
}