using System;
using System.Collections.Generic;
using System.Diagnostics;
using TiendaGlobosLaFiesta.Views;

namespace TiendaGlobosLaFiesta.Core
{
    public class ModuloManager
    {
        private static ModuloManager _instancia;
        public static ModuloManager Instancia => _instancia ??= new ModuloManager();

        // Referencias a módulos
        public VentasControl Ventas { get; private set; }

        // Eventos globales
        public event Action VentaRegistrada;
        public event Action ClienteAgregado;
        public event Action StockActualizado;

        private readonly Dictionary<string, object> _modulosRegistrados = new();

        private ModuloManager() { }

        // Registrar módulo (si ya existe, actualiza la referencia)
        public bool RegistrarModulo(string nombre, object modulo)
        {
            if (string.IsNullOrWhiteSpace(nombre) || modulo == null)
            {
                Debug.WriteLine($"⚠️ Módulo '{nombre}' inválido.");
                return false;
            }

            if (_modulosRegistrados.ContainsKey(nombre))
                _modulosRegistrados[nombre] = modulo;
            else
                _modulosRegistrados.Add(nombre, modulo);

            if (modulo is VentasControl ventas)
            {
                // Registrar solo una vez
                if (Ventas != ventas)
                    Ventas = ventas;
            }

            Debug.WriteLine($"✅ Módulo '{nombre}' registrado correctamente.");
            return true;
        }

        public T ObtenerModulo<T>(string nombre) where T : class
        {
            if (_modulosRegistrados.TryGetValue(nombre, out var modulo))
                return modulo as T;

            Debug.WriteLine($"⚠️ No se encontró el módulo '{nombre}'.");
            return null;
        }

        // Notificaciones
        public void NotificarVentaRegistrada() => VentaRegistrada?.Invoke();
        public void NotificarClienteAgregado()
        {
            ClienteAgregado?.Invoke();
            Ventas?.RefrescarListaClientes();
        }
        public void NotificarStockActualizado() => StockActualizado?.Invoke();

        public void LimpiarModulos()
        {
            _modulosRegistrados.Clear();
            Ventas = null;
            Debug.WriteLine("🧹 Todos los módulos fueron limpiados.");
        }
    }
}