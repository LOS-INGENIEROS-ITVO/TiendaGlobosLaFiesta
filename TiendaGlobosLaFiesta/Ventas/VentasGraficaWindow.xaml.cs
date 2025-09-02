using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TiendaGlobosLaFiesta.ViewModels;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasGraficaWindow : Window
    {
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        private int _topN;

        public VentasGraficaWindow(ModeloDeVistaVentas vm, int topN = 12)
        {
            InitializeComponent();
            _topN = topN;

            // Inicializar con todo el historial; puede actualizarse luego con filtrado.
            UpdateChart(vm.Historial, topN);

            DataContext = this;
        }

        /// <summary>
        /// Actualiza la gráfica con un conjunto de ventas (por ejemplo: Historial o HistorialView filtrado).
        /// Agrupa por cliente, toma topN y suma el resto como "Otros".
        /// </summary>
        public void UpdateChart(IEnumerable<VentaHistorial> historialFiltrado, int topN = -1)
        {
            if (topN <= 0) topN = _topN;

            var ventas = historialFiltrado
                .GroupBy(v => v.ClienteNombre)
                .Select(g => new { Cliente = g.Key, Total = g.Sum(v => (double)v.Total) })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Top N + "Otros"
            var top = ventas.Take(topN).ToList();
            var othersTotal = ventas.Skip(topN).Sum(x => x.Total);
            if (othersTotal > 0)
            {
                top.Add(new { Cliente = "Otros", Total = othersTotal });
            }

            var labels = top.Select(t => t.Cliente).ToArray();
            var values = top.Select(t => t.Total).ToArray();


            Series = new ISeries[]
{
    new ColumnSeries<double>
    {
        Values = values,
        Name = "Ventas",
        MaxBarWidth = 48,
        Fill = new SolidColorPaint(SKColors.MediumSlateBlue),
        Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 1 },
        AnimationsSpeed = TimeSpan.FromMilliseconds(600)
        // NO usar DataLabels/DataLabelsFormatter/TooltipLabelFormatter si tu paquete no los soporta
    }
};

            XAxes = new[]
            {
    new Axis
    {
        Labels = labels,
        LabelsRotation = 45,
        LabelsPaint = new SolidColorPaint(SKColors.Black),
        TextSize = 12
    }
};

            YAxes = new[]
            {
    new Axis
    {
        Labeler = value => value.ToString("C"),
        LabelsPaint = new SolidColorPaint(SKColors.Black),
        TextSize = 12
    }
};

            DataContext = this;

            // Evitar solapamiento: dar ancho mínimo proportional al número de barras
            if (Chart != null)
            {
                var minWidth = Math.Max(700, labels.Length * 70);
                Chart.MinWidth = minWidth;
            }

            // Forzar actualización del binding (simple y efectivo)
            DataContext = null;
            DataContext = this;
        }
    }
}