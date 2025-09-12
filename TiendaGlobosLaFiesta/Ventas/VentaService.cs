using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private readonly GloboRepository _globoRepo = new(); // Asumimos que existe y funciona como ProductoRepository

        /// <summary>
        /// Orquesta el registro completo de una venta, incluyendo validación de stock y
        /// una transacción para garantizar la integridad de los datos.
        /// </summary>
        /// <param name="venta">El objeto de venta a registrar.</param>
        /// <param name="mensajeError">Mensaje de salida en caso de error.</param>
        /// <returns>True si la venta fue exitosa, de lo contrario False.</returns>
        public bool RegistrarVentaCompleta(Venta venta, out string mensajeError)
        {
            mensajeError = string.Empty;

            // 1. Validar Stock antes de iniciar cualquier operación de base de datos.
            if (!ValidarStock(venta, out mensajeError))
            {
                return false;
            }

            // 2. Iniciar la conexión y la transacción.
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();

            try
            {
                // 3. Insertar el registro principal (maestro) de la venta.
                _ventasRepo.InsertarVentaMaestro(venta, conn, tran);

                // 4. Insertar cada línea de detalle y actualizar el stock correspondiente.
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

                // 5. Si todas las operaciones fueron exitosas, se confirma la transacción.
                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // 6. Si ocurre cualquier error, se revierten todos los cambios.
                tran.Rollback();
                mensajeError = $"Error en la base de datos: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Valida que haya suficiente stock para todos los artículos en la venta.
        /// </summary>
        private bool ValidarStock(Venta venta, out string mensaje)
        {
            // Usamos Concat para tratar productos y globos de la misma forma gracias a la clase base ItemVenta.
            foreach (var item in venta.Productos.Concat<ItemVenta>(venta.Globos))
            {
                if (item.Cantidad > item.Stock)
                {
                    string nombre = (item is ProductoVenta p) ? p.Nombre : ((GloboVenta)item).Nombre;
                    mensaje = $"No hay suficiente stock para '{nombre}'. Disponible: {item.Stock}, Solicitado: {item.Cantidad}.";
                    return false;
                }
            }
            mensaje = string.Empty;
            return true;
        }
    }
}