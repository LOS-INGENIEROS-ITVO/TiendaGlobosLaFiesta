using System;
using System.Linq;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Services
{
    public class VentaService
    {
        private readonly VentasRepository _ventasRepo = new();
        private readonly ProductoRepository _productoRepo = new();
        private readonly GloboRepository _globoRepo = new();
        private readonly StockManagerRepository _stockManager = new(DbHelper.ConnectionString);

        public bool RegistrarVentaCompleta(Venta venta, out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar que el empleado de sesión exista
            if (!SesionActual.EmpleadoId.HasValue)
            {
                mensajeError = "No se ha iniciado sesión con un empleado válido.";
                return false;
            }

            // Validar Stock de productos y globos
            foreach (var item in venta.Productos.Cast<ItemVenta>().Concat(venta.Globos))
            {
                if (item.Cantidad > item.Stock)
                {
                    mensajeError = $"No hay suficiente stock para el item con ID {item.Id}.";
                    return false;
                }
            }

            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                // Insertar venta maestro
                _ventasRepo.InsertarVentaMaestro(venta, conn, tran);

                // Insertar detalle productos y actualizar stock
                foreach (var p in venta.Productos)
                {
                    _ventasRepo.InsertarDetalleProducto(venta.VentaId, p, conn, tran);
                    _stockManager.AjustarStockProducto(
                        p.ProductoId,
                        p.Cantidad,
                        SesionActual.EmpleadoId.Value,
                        "Venta realizada",
                        conn,
                        tran
                    );
                }

                // Insertar detalle globos y actualizar stock
                foreach (var g in venta.Globos)
                {
                    _ventasRepo.InsertarDetalleGlobo(venta.VentaId, g, conn, tran);
                    _stockManager.AjustarStockGlobo(
                        g.GloboId,
                        g.Cantidad,
                        SesionActual.EmpleadoId.Value,
                        "Venta realizada",
                        conn,
                        tran
                    );
                }

                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                tran.Rollback();
                mensajeError = $"Error en BD: {ex.Message}";
                return false;
            }
        }
    }
}