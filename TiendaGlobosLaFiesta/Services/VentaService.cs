using System;
using System.Collections.Generic;
using System.Linq;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models.Ventas;
using TiendaGlobosLaFiesta.Models.Inventario;
using TiendaGlobosLaFiesta.Models.Clientes;
using TiendaGlobosLaFiesta.Models.Utilities;

namespace TiendaGlobosLaFiesta.Services
{
    public class VentaService
    {
        private readonly VentasRepository _ventasRepo;
        private readonly StockManagerRepository _stockManager;
        private readonly ProductoRepository _productoRepo;
        private readonly GloboRepository _globoRepo;

        public VentaService()
        {
            _productoRepo = new ProductoRepository();
            _globoRepo = new GloboRepository();
            _ventasRepo = new VentasRepository();
            _stockManager = new StockManagerRepository(_productoRepo, _globoRepo);
        }

        public bool RegistrarVentaCompleta(Venta venta, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (!SesionActual.EmpleadoId.HasValue)
            {
                mensajeError = "No se ha iniciado sesión con un empleado válido.";
                return false;
            }

            foreach (var item in venta.Productos.Cast<ItemVenta>().Concat(venta.Globos))
            {
                if (item.Cantidad > item.Stock)
                {
                    mensajeError = $"No hay suficiente stock para el item con ID {item.Id}.";
                    return false;
                }
            }

            try
            {
                using var conn = DbHelper.ObtenerConexion();
                using var tran = conn.BeginTransaction();

                _ventasRepo.InsertarVentaMaestro(venta, conn, tran);

                foreach (var p in venta.Productos)
                    _ventasRepo.InsertarDetalleProducto(venta.VentaId, p, conn, tran);

                foreach (var g in venta.Globos)
                    _ventasRepo.InsertarDetalleGlobo(venta.VentaId, g, conn, tran);

                var itemsStock = venta.Productos.Select(p => (id: p.ProductoId, cantidad: p.Cantidad, esGlobo: false))
                    .Concat(venta.Globos.Select(g => (id: g.GloboId, cantidad: g.Cantidad, esGlobo: true)))
                    .ToList();

                _stockManager.AjustarStockCombinado(itemsStock, SesionActual.EmpleadoId.Value, "Venta realizada", conn, tran);

                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                mensajeError = $"Error en BD: {ex.Message}";
                return false;
            }
        }

        public VentaHistorial ObtenerUltimaVenta()
        {
            return _ventasRepo.ObtenerHistorialVentas().FirstOrDefault();
        }

        public List<Cliente> ObtenerClientes()
        {
            return _ventasRepo.ObtenerClientes();
        }

        public List<VentaHistorial> ObtenerHistorialVentas()
        {
            return _ventasRepo.ObtenerHistorialVentas();
        }

        public List<VentaHistorial> FiltrarHistorial(string clienteId, DateTime? desde, DateTime? hasta)
        {
            var historial = ObtenerHistorialVentas(); // Obtenemos todos los registros

            if (!string.IsNullOrEmpty(clienteId))
                historial = historial.Where(v => v.ClienteId == clienteId).ToList();

            if (desde.HasValue)
                historial = historial.Where(v => v.FechaVenta >= desde.Value).ToList();

            if (hasta.HasValue)
                historial = historial.Where(v => v.FechaVenta <= hasta.Value).ToList();

            return historial;
        }
    }
}