using System;
using System.Collections.Generic;
using TiendaGlobosLaFiesta.Models.Ventas;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta.Managers
{
    public class ModuloManager
    {
        // Singleton
        private static ModuloManager _instancia;
        public static ModuloManager Instancia
        {
            get
            {
                if (_instancia == null)
                    throw new InvalidOperationException("ModuloManager no ha sido inicializado.");
                return _instancia;
            }
        }

        public static void Inicializar(ProductoRepository productoRepo, GloboRepository globoRepo,
                                       StockManagerRepository stockManager, VentasRepository ventasRepo)
        {
            _instancia = new ModuloManager(productoRepo, globoRepo, stockManager, ventasRepo);
        }

        private readonly ProductoRepository _productoRepo;
        private readonly GloboRepository _globoRepo;
        private readonly StockManagerRepository _stockManager;
        private readonly VentasRepository _ventasRepo;

        // Evento para notificar a Dashboard u otros módulos
        public event Action VentaRegistrada;

        private readonly Dictionary<string, object> _modulosRegistrados = new();

        private ModuloManager(ProductoRepository productoRepo,
                              GloboRepository globoRepo,
                              StockManagerRepository stockManager,
                              VentasRepository ventasRepo)
        {
            _productoRepo = productoRepo;
            _globoRepo = globoRepo;
            _stockManager = stockManager;
            _ventasRepo = ventasRepo;
        }

        public void RegistrarModulo(string nombre, object modulo)
        {
            _modulosRegistrados[nombre] = modulo;
        }

        /// <summary>
        /// Registra una venta completa y ajusta stock automáticamente.
        /// </summary>
        public bool RegistrarVenta(string ventaId, int empleadoId, string clienteId,
                                   List<ProductoVenta> productos, List<GloboVenta> globos)
        {
            try
            {
                _ventasRepo.RegistrarVenta(ventaId, empleadoId, clienteId, productos, globos);

                foreach (var p in productos)
                    _stockManager.DisminuirStockProducto(p.Id, p.Cantidad, empleadoId, "Venta registrada");

                foreach (var g in globos)
                    _stockManager.DisminuirStockGlobo(g.Id, g.Cantidad, empleadoId, "Venta registrada");

                VentaRegistrada?.Invoke(); // Notificar suscriptores
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar la venta: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene la lista de productos activos.
        /// </summary>
        public List<ProductoVenta> ObtenerProductos()
        {
            var productos = _productoRepo.ObtenerTodos();
            var lista = new List<ProductoVenta>();
            foreach (var p in productos)
                lista.Add(new ProductoVenta(p));
            return lista;
        }

        /// <summary>
        /// Obtiene la lista de globos activos.
        /// </summary>
        public List<GloboVenta> ObtenerGlobos()
        {
            var globos = _globoRepo.ObtenerTodos();
            var lista = new List<GloboVenta>();
            foreach (var g in globos)
                lista.Add(new GloboVenta(g));
            return lista;
        }
    }
}