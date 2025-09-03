using System.Linq;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasGraficaWindow : Window
    {
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        // Paleta de colores más atractiva
        private readonly SKColor barraColor = SKColors.MediumSlateBlue;
        private readonly SKColor ejeColor = SKColors.DarkSlateGray;

        public VentasGraficaWindow(ModeloDeVistaVentas vm)
        {
            InitializeComponent();

            var ventasPorCliente = vm.Historial
                .GroupBy(v => v.ClienteNombre)
                .Select(g => new
                {
                    Cliente = g.Key,
                    Total = g.Sum(v => (double)v.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var labels = ventasPorCliente.Select(d => d.Cliente).ToArray();
            var values = ventasPorCliente.Select(d => d.Total).ToArray();

            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = values,
                    Name = "Ventas",
                    Fill = new SolidColorPaint(barraColor) // Color más atractivo
                }
            };

            XAxes = new[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 45,
                    LabelsPaint = new SolidColorPaint(ejeColor)
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("C"),
                    LabelsPaint = new SolidColorPaint(ejeColor)
                }
            };

            DataContext = this;
        }
    }
}
