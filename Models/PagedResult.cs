namespace Inmobiliaria10.Models
{
    public class PagedResult<T>
    {
        public IList<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }          // página actual (1-based)
        public int PageSize { get; set; }      // tamaño de página
        public int TotalItems { get; set; }    // total de registros que cumplen el filtro
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, PageSize));
        public string? Query { get; set; }     // término de búsqueda (opcional)
    }
}
