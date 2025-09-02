using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasGraficaWindow : Window
    {
        public VentasGraficaWindow(ViewModels.ModeloDeVistaVentas vm)
        {
            InitializeComponent();

            // Agrupar ventas por cliente
            var ventasPorCliente = vm.Historial
                .GroupBy(vh => vh.ClienteNombre)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Total));

            // Crear series
            var series = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Values = ventasPorCliente.Values
                }
            };

            // Configurar ejes
            Chart.Series = series;
            Chart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = ventasPorCliente.Keys.ToArray(),
                    LabelsRotation = 15
                }
            };
            Chart.YAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("C")
                }
            };
        }
    }
}