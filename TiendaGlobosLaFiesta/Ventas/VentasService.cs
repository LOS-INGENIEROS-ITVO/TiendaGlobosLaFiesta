using System;
using System.Collections.Generic;
using System.Linq;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta.Services
{
    public static class VentasService
    {
        public static bool ValidarStock(IEnumerable<ItemVenta> items, out string mensaje)
        {
            foreach (var item in items)
            {
                if (item.Cantidad > item.Stock)
                {
                    mensaje = item is ProductoVenta pv ?
                        $"No hay suficiente stock para el producto {pv.Nombre}." :
                        $"No hay suficiente stock para el globo {((GloboVenta)item).Material} - {((GloboVenta)item).Color}.";
                    return false;
                }
            }
            mensaje = null;
            return true;
        }

        public static void ActualizarStock(IEnumerable<ItemVenta> items)
        {
            foreach (var item in items)
            {
                item.Stock -= item.Cantidad;
                item.Cantidad = 0;
            }
        }

        public static VentaHistorial CrearHistorial(Venta venta, Cliente cliente)
        {
            return new VentaHistorial
            {
                VentaId = venta.VentaId,
                ClienteId = cliente.ClienteId,
                ClienteNombre = cliente.NombreCompleto(),
                Empleado = SesionActual.NombreEmpleadoCompleto,
                FechaVenta = venta.FechaVenta,
                Total = venta.ImporteTotal,
                Productos = venta.Productos,
                Globos = venta.Globos
            };
        }
    }
}