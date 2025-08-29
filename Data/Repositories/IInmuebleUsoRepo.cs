using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IInmuebleUsoRepo
    {
        int Agregar(InmuebleUso inmuebleUso);
        void Editar(InmuebleUso inmuebleUso);
        void Eliminar(int id);
        InmuebleUso? ObtenerPorId(int id);
        List<InmuebleUso> MostrarTodosInmuebleUsos();
        bool ExisteDenominacion(string denominacion, int? id = null);}
}