using System;
using System.Collections.Generic;
using TiendaGlobosLaFiesta.Clientes;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public static class VentaDAO
    {
        // -------------------------
        // Clientes
        // -------------------------
        public static List<Cliente> ObtenerClientes()
        {
            return ConexionBD.ObtenerClientes();
        }

        // -------------------------
        // Productos
        // -------------------------
        public static List<ProductoVenta> ObtenerProductos()
        {
            return ConexionBD.ObtenerProductos();
        }

        // -------------------------
        // Globos
        // -------------------------
        public static List<GloboVenta> ObtenerGlobos()
        {
            return ConexionBD.ObtenerGlobos();
        }

        // -------------------------
        // Registrar venta
        // -------------------------
        public static bool RegistrarVenta(Venta venta)
        {
            return ConexionBD.RegistrarVenta(venta);
        }

        // -------------------------
        // Historial de ventas
        // -------------------------
        public static List<VentaHistorial> ObtenerHistorial()
        {
            return ConexionBD.ObtenerHistorialVentas();
        }
    }
}