using System;
using System.Linq;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Services
{
    public class VentaService
    {
        private readonly VentasRepository _ventasRepo;
        private readonly StockManagerRepository _stockManager;

        public VentaService()
        {
            var productoRepo = new ProductoRepository();
            var globoRepo = new GloboRepository();
            _ventasRepo = new VentasRepository();
            _stockManager = new StockManagerRepository(productoRepo, globoRepo);
        }

        public bool RegistrarVentaCompleta(Venta venta, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (!SesionActual.EmpleadoId.HasValue)
            {
                mensajeError = "No se ha iniciado sesión con un empleado válido.";
                return false;
            }

            // Validar stock antes de iniciar transacción
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

                // Insertar venta maestro
                _ventasRepo.InsertarVentaMaestro(venta, conn, tran);

                // Insertar detalle de productos
                foreach (var p in venta.Productos)
                    _ventasRepo.InsertarDetalleProducto(venta.VentaId, p, conn, tran);

                // Insertar detalle de globos
                foreach (var g in venta.Globos)
                    _ventasRepo.InsertarDetalleGlobo(venta.VentaId, g, conn, tran);

                // Preparar lista de items para ajuste combinado
                var itemsStock = venta.Productos
                    .Select(p => (id: p.ProductoId, cantidad: p.Cantidad, esGlobo: false))
                    .Concat(venta.Globos.Select(g => (id: g.GloboId, cantidad: g.Cantidad, esGlobo: true)))
                    .ToList();

                // Ajustar stock usando la transacción existente
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
    }
}
