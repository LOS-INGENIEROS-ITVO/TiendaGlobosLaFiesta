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

            // Validar Stock
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
                _ventasRepo.InsertarVentaMaestro(venta, conn, tran);

                foreach (var p in venta.Productos)
                {
                    _ventasRepo.InsertarDetalleProducto(venta.VentaId, p, conn, tran);
                    _productoRepo.ActualizarStock(p.ProductoId, p.Cantidad, conn, tran);
                }
                foreach (var g in venta.Globos)
                {
                    _ventasRepo.InsertarDetalleGlobo(venta.VentaId, g, conn, tran);
                    _globoRepo.ActualizarStock(g.GloboId, g.Cantidad, conn, tran);
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