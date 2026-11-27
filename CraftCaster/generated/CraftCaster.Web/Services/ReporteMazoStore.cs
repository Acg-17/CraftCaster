using System.Text.Json;

namespace CraftCaster.Web.Services;

public sealed class ReporteMazoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int AutorId { get; set; }
    public string AutorNombre { get; set; } = string.Empty;
    public int MazoId { get; set; }
    public string MazoNombre { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public string ReportadoPor { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
}

// Almacén ligero de reportes de mazos en JSON (sin modificar el .tdm ni añadir tablas a la BD)
public static class ReporteMazoStore
{
    private static readonly object LockObj = new();
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    private static string RutaFichero()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "App_Data");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "reportes_mazos.json");
    }

    public static List<ReporteMazoItem> ObtenerTodos()
    {
        lock (LockObj)
        {
            var ruta = RutaFichero();
            if (!File.Exists(ruta)) return new List<ReporteMazoItem>();
            try
            {
                var json = File.ReadAllText(ruta);
                return JsonSerializer.Deserialize<List<ReporteMazoItem>>(json) ?? new List<ReporteMazoItem>();
            }
            catch
            {
                return new List<ReporteMazoItem>();
            }
        }
    }

    public static void Agregar(ReporteMazoItem item)
    {
        lock (LockObj)
        {
            var lista = ObtenerTodos();
            lista.Insert(0, item);
            File.WriteAllText(RutaFichero(), JsonSerializer.Serialize(lista, JsonOpts));
        }
    }

    public static void Eliminar(string reporteId)
    {
        lock (LockObj)
        {
            var lista = ObtenerTodos();
            lista.RemoveAll(r => r.Id == reporteId);
            File.WriteAllText(RutaFichero(), JsonSerializer.Serialize(lista, JsonOpts));
        }
    }

    public static void EliminarPorAutor(int autorId)
    {
        lock (LockObj)
        {
            var lista = ObtenerTodos();
            lista.RemoveAll(r => r.AutorId == autorId);
            File.WriteAllText(RutaFichero(), JsonSerializer.Serialize(lista, JsonOpts));
        }
    }
}
