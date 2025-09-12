using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Services
{
    public class VentaService
    {
        private readonly ProductoRepository _productoRepo = new();
        private readonly GloboRepository _globoRepo = new();

        // Valida que haya suficiente stock
        public bool ValidarStock(IEnumerable<ItemVenta> items, out string mensaje)
        {
            foreach (var item in items)
            {
                if (item.Cantidad > item.Stock)
                {
                    mensaje = $"No hay suficiente stock de {(item is ProductoVenta p ? p.Nombre : ((GloboVenta)item).Nombre)}.";
                    return false;
                }
            }
            mensaje = "";
            return true;
        }

        // Actualiza stock en la DB
        public void ActualizarStock(IEnumerable<ItemVenta> items)
        {
            foreach (var item in items)
            {
                if (item is ProductoVenta pv)
                {
                    var prod = _productoRepo.ObtenerProductoPorId(pv.ProductoId);
                    if (prod != null)
                    {
                        prod.Stock -= pv.Cantidad;
                        _productoRepo.ActualizarProducto(prod);
                    }
                }
                else if (item is GloboVenta gv)
                {
                    var globo = _globoRepo.ObtenerGloboPorId(gv.GloboId);
                    if (globo != null)
                    {
                        globo.Stock -= gv.Cantidad;
                        _globoRepo.ActualizarGlobo(globo);
                    }
                }
            }
        }

        // Crea un historial de venta para mostrar en la UI
        public VentaHistorial CrearHistorial(Venta venta, Cliente cliente, string nombreEmpleado)
        {
            return new VentaHistorial
            {
                VentaId = venta.VentaId,
                ClienteId = cliente.ClienteId,
                ClienteNombre = cliente.NombreCompleto, // <-- CORREGIDO: propiedad, no método
                Empleado = nombreEmpleado,
                FechaVenta = venta.FechaVenta,
                Total = venta.ImporteTotal,
                Productos = new ObservableCollection<ProductoVenta>(venta.Productos),
                Globos = new ObservableCollection<GloboVenta>(venta.Globos)
            };
        }
    }
}



//que bendicion