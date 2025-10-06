using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Services
{
    public class VentaService
    {
        private readonly VentasRepository _ventasRepo = new();
        private readonly ProductoRepository _productoRepo = new();
        private readonly GloboRepository _globoRepo = new();

        public bool RegistrarVentaCompleta(Venta venta, out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar Stock de productos y globos
            foreach (var item in venta.Productos.Cast<ItemVenta>().Concat(venta.Globos))
            {
                if (item.Cantidad > item.Stock)
                {
                    mensajeError = $"No hay suficiente stock para el item con ID {item.Id}.";
                    return false;
                }
            }

            // Iniciar Transacción
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
                    _productoRepo.ActualizarStock(p.ProductoId, p.Cantidad, null, conn, tran); // empleadoId null
                }

                // Insertar detalle globos y actualizar stock
                foreach (var g in venta.Globos)
                {
                    _ventasRepo.InsertarDetalleGlobo(venta.VentaId, g, conn, tran);
                    _globoRepo.ActualizarStock(g.GloboId, g.Cantidad, null, conn, tran); // empleadoId null
                }

                // Confirmar transacción
                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // Revertir transacción en caso de error
                tran.Rollback();
                mensajeError = $"Error en BD: {ex.Message}";
                return false;
            }
        }
    }
}