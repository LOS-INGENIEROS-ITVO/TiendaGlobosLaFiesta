using System;
using System.Collections.Generic;
using System.Linq;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Managers;
using TiendaGlobosLaFiesta.Models.Clientes;
using TiendaGlobosLaFiesta.Models.Utilities;
using TiendaGlobosLaFiesta.Models.Ventas;

namespace TiendaGlobosLaFiesta.Services
{
    public class VentaService
    {
        private readonly VentasRepository _ventasRepo;
        private readonly ModuloManager _moduloManager;

        public VentaService(VentasRepository ventasRepo, ModuloManager moduloManager)
        {
            _ventasRepo = ventasRepo;
            _moduloManager = moduloManager;
        }

        // Obtener clientes activos
        public List<Cliente> ObtenerClientes()
        {
            return _ventasRepo.ObtenerClientes();
        }

        // Registrar venta completa (productos + globos)
        public bool RegistrarVentaCompleta(Venta venta, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (venta == null)
            {
                mensajeError = "La venta es nula.";
                return false;
            }

            if (!venta.Productos.Any() && !venta.Globos.Any())
            {
                mensajeError = "Debe contener al menos un producto o globo.";
                return false;
            }

            try
            {
                // Aquí usamos ModuloManager para registrar la venta
                int empleadoId = SesionActual.EmpleadoId ?? 0;
                string clienteId = venta.ClienteId ?? "C0001";

                bool exito = _moduloManager.RegistrarVenta(
                    venta.VentaId, empleadoId, clienteId,
                    venta.Productos.ToList(), venta.Globos.ToList());

                if (exito)
                {
                    // Notificar si quieres
                    // _moduloManager.NotificarVentaRegistrada(); 
                    return true;
                }
                else
                {
                    mensajeError = "No se pudo registrar la venta.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al registrar la venta: {ex.Message}";
                return false;
            }
        }

        // Obtener historial completo de ventas
        public List<VentaHistorial> ObtenerHistorialVentas()
        {
            return _ventasRepo.ObtenerHistorialVentas();
        }

        // Filtrar historial de ventas por cliente y fechas
        public List<VentaHistorial> FiltrarHistorial(string clienteId, DateTime? desde, DateTime? hasta)
        {
            var historial = ObtenerHistorialVentas();

            if (!string.IsNullOrEmpty(clienteId))
                historial = historial.FindAll(v => v.ClienteId == clienteId);

            if (desde.HasValue)
                historial = historial.FindAll(v => v.FechaVenta.Date >= desde.Value.Date);

            if (hasta.HasValue)
                historial = historial.FindAll(v => v.FechaVenta.Date <= hasta.Value.Date);

            return historial;
        }
    }
}