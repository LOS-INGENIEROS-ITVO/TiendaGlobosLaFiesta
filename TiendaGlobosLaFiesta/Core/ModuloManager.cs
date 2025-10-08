using System;
using System.Collections.Generic;
using System.Diagnostics;
using TiendaGlobosLaFiesta.Views; // Ajusta según la ruta real de tus UserControls
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta.Core
{
    /// <summary>
    /// Clase central que coordina la comunicación entre los módulos del sistema (Ventas, Inventario, Dashboard, etc.).
    /// Gestiona eventos, actualizaciones y validaciones entre componentes.
    /// </summary>
    public class ModuloManager
    {
        // --- INSTANCIA ÚNICA (Singleton) ---
        private static ModuloManager _instancia;
        public static ModuloManager Instancia => _instancia ??= new ModuloManager();

        // --- REFERENCIAS A MÓDULOS ---
        public DashboardGerenteControl Dashboard { get; private set; }
        public VentasControl Ventas { get; private set; }
        public InventarioControl Inventario { get; private set; }

        // --- EVENTOS GLOBALES ---
        public event Action VentaRegistrada;
        public event Action StockActualizado;
        public event Action PedidoCompletado;
        public event Action ClienteAgregado;

        // --- LISTA INTERNA DE MÓDULOS REGISTRADOS ---
        private readonly Dictionary<string, object> _modulosRegistrados = new();

        // --- CONSTRUCTOR PRIVADO ---
        private ModuloManager() { }

        // ==========================================================
        // MÉTODOS DE REGISTRO DE MÓDULOS
        // ==========================================================

        /// <summary>
        /// Registra un módulo para permitir la comunicación entre ellos.
        /// </summary>
        public bool RegistrarModulo(string nombre, object modulo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                Debug.WriteLine("⚠️ No se puede registrar un módulo sin nombre.");
                return false;
            }

            if (modulo == null)
            {
                Debug.WriteLine($"⚠️ El módulo '{nombre}' es nulo y no será registrado.");
                return false;
            }

            if (_modulosRegistrados.ContainsKey(nombre))
            {
                Debug.WriteLine($"ℹ️ El módulo '{nombre}' ya está registrado. Se actualiza su referencia.");
                _modulosRegistrados[nombre] = modulo;
            }
            else
            {
                _modulosRegistrados.Add(nombre, modulo);
            }

            // Asigna referencia según el tipo detectado
            if (modulo is DashboardGerenteControl dashboard) Dashboard = dashboard;
            if (modulo is VentasControl ventas) Ventas = ventas;
            if (modulo is InventarioControl inventario) Inventario = inventario;

            Debug.WriteLine($"✅ Módulo '{nombre}' registrado correctamente.");
            return true;
        }

        /// <summary>
        /// Obtiene un módulo registrado por nombre.
        /// </summary>
        public T ObtenerModulo<T>(string nombre) where T : class
        {
            if (_modulosRegistrados.TryGetValue(nombre, out var modulo))
            {
                return modulo as T;
            }

            Debug.WriteLine($"⚠️ No se encontró el módulo '{nombre}'.");
            return null;
        }

        // ==========================================================
        // EVENTOS Y COORDINACIÓN ENTRE MÓDULOS
        // ==========================================================

        /// <summary>
        /// Se llama cuando una venta fue registrada en el sistema.
        /// Actualiza Dashboard, Inventario y demás suscriptores.
        /// </summary>
        public void NotificarVentaRegistrada()
        {
            try
            {
                VentaRegistrada?.Invoke();

                // Actualiza módulos dependientes
                Dashboard?.RefrescarKPIs();
                Inventario?.RefrescarStock();

                Debug.WriteLine("📊 Venta registrada y módulos actualizados correctamente.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al notificar venta: {ex.Message}");
            }
        }

        /// <summary>
        /// Se llama cuando se ajusta stock de producto o globo.
        /// </summary>
        public void NotificarAjusteStock()
        {
            try
            {
                StockActualizado?.Invoke();
                Dashboard?.RefrescarKPIs();

                Debug.WriteLine("📦 Ajuste de stock notificado y dashboard actualizado.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al notificar ajuste de stock: {ex.Message}");
            }
        }

        /// <summary>
        /// Se llama cuando se completa un pedido al proveedor.
        /// </summary>
        public void NotificarPedidoCompletado()
        {
            try
            {
                PedidoCompletado?.Invoke();
                Inventario?.RefrescarStock();
                Dashboard?.RefrescarKPIs();

                Debug.WriteLine("📬 Pedido completado, inventario y dashboard actualizados.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al notificar pedido completado: {ex.Message}");
            }
        }

        /// <summary>
        /// Se llama cuando se agrega un nuevo cliente al sistema.
        /// </summary>
        public void NotificarClienteAgregado()
        {
            try
            {
                ClienteAgregado?.Invoke();
                Ventas?.RefrescarListaClientes();

                Debug.WriteLine("👥 Nuevo cliente agregado y ventas actualizadas.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al notificar cliente agregado: {ex.Message}");
            }
        }

        // ==========================================================
        // UTILIDADES
        // ==========================================================

        /// <summary>
        /// Limpia el registro de módulos (por ejemplo, al cerrar sesión).
        /// </summary>
        public void LimpiarModulos()
        {
            _modulosRegistrados.Clear();
            Dashboard = null;
            Ventas = null;
            Inventario = null;
            Debug.WriteLine("🧹 Todos los módulos fueron limpiados del registro.");
        }
    }
}