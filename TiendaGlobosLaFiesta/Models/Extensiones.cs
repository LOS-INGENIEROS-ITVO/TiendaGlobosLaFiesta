using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace TiendaGlobosLaFiesta.Models
{
    public static class Extensiones
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }

        public static ObservableCollection<Cliente> DataTableToClientes(this DataTable dt)
        {
            return new ObservableCollection<Cliente>(
                dt.Rows.Cast<DataRow>().Select(r => new Cliente
                {
                    ClienteId = r["clienteId"].ToString(),
                    Nombre = $"{r["primerNombre"]} {r["segundoNombre"]} {r["apellidoP"]} {r["apellidoM"]}".Trim()
                })
            );
        }
    }
}