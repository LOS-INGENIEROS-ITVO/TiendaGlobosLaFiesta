using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using TiendaGlobosLaFiesta.Clientes;

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
                    PrimerNombre = r["primerNombre"].ToString(),
                    SegundoNombre = r["segundoNombre"].ToString(),
                    ApellidoP = r["apellidoP"].ToString(),
                    ApellidoM = r["apellidoM"].ToString(),
                    Telefono = int.TryParse(r["telefono"].ToString(), out int tel) ? tel : (int?)null
                })
            );
        }
    }
}