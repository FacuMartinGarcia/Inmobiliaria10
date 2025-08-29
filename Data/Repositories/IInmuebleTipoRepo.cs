using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IInmuebleTipoRepo
    {
        public List<InmuebleTipo> MostrarTodosInmuebleTipos();
        public int Agregar(InmuebleTipo i);
        public void Editar(InmuebleTipo i);
        public void Eliminar(int id);
        public InmuebleTipo? ObtenerPorId(int id);
    }
}