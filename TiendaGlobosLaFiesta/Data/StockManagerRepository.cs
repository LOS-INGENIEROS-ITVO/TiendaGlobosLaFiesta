using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Data
{
    public class StockManagerRepository
    {
        private readonly ProductoRepository _productoRepo;
        private readonly GloboRepository _globoRepo;

        public StockManagerRepository(ProductoRepository productoRepo, GloboRepository globoRepo)
        {
            _productoRepo = productoRepo ?? throw new ArgumentNullException(nameof(productoRepo));
            _globoRepo = globoRepo ?? throw new ArgumentNullException(nameof(globoRepo));
        }

        // ---------------------------
        // AJUSTE DE STOCK COMBINADO
        // ---------------------------
        public void AjustarStockCombinado(List<(string id, int cantidad, bool esGlobo)> items, int empleadoId, string motivo, SqlConnection conn, SqlTransaction tran)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("No se proporcionaron items para ajustar.", nameof(items));

            foreach (var item in items)
            {
                if (item.esGlobo)
                {
                    if (!_globoRepo.AjustarStockConHistorial(item.id, item.cantidad, empleadoId, motivo, conn, tran))
                        throw new Exception($"Error ajustando stock del globo {item.id}");
                }
                else
                {
                    if (!_productoRepo.AjustarStockConHistorial(item.id, item.cantidad, empleadoId, motivo, conn, tran))
                        throw new Exception($"Error ajustando stock del producto {item.id}");
                }
            }
        }

        // ---------------------------
        // STOCK CRÍTICO
        // ---------------------------

        /// <summary>
        /// Obtiene productos cuyo stock está por debajo o igual al mínimo indicado.
        /// </summary>
        public List<StockCriticoItem> ObtenerProductosStockCritico(int stockMinimo = 5)
        {
            var lista = _productoRepo.ObtenerProductos();
            if (lista == null) return new List<StockCriticoItem>();

            return lista
    .Where(p => p.Stock <= stockMinimo)
    .Select(p => new StockCriticoItem
    {
        Id = p.ProductoId,
        Nombre = p.Nombre,
        StockActual = p.Stock,
        Precio = p.Costo,
        Tipo = "Producto",
        Unidad = p.Unidad ?? "pieza",
        Color = "-", // productos no tienen color
        Producto = p
    })
    .ToList();
        }

        /// <summary>
        /// Obtiene globos cuyo stock está por debajo o igual al mínimo indicado.
        /// </summary>
        public List<StockCriticoItem> ObtenerGlobosStockCritico(int stockMinimo = 5)
        {
            var lista = _globoRepo.ObtenerGlobos();
            if (lista == null) return new List<StockCriticoItem>();

            return lista
    .Where(g => g.Stock <= stockMinimo)
    .Select(g => new StockCriticoItem
    {
        Id = g.GloboId,
        Nombre = $"{g.Material} {g.Tamano} {g.Forma}".Trim(),
        StockActual = g.Stock,
        Precio = g.Costo,
        Tipo = "Globo",
        Unidad = g.Unidad ?? "pieza",
        Color = g.Color ?? "---",
        Globo = g
    })
    .ToList();

        }

        // ---------------------------
        // MÉTODOS AUXILIARES
        // ---------------------------

        /// <summary>
        /// Devuelve todos los items de stock crítico combinados (productos + globos)
        /// </summary>
        public List<StockCriticoItem> ObtenerStockCriticoCombinado(int stockMinimo = 5)
        {
            var lista = new List<StockCriticoItem>();
            lista.AddRange(ObtenerProductosStockCritico(stockMinimo));
            lista.AddRange(ObtenerGlobosStockCritico(stockMinimo));
            return lista;
        }
    }
}