using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AnalisisDatos
{
    public partial class Form1 : Form
    {
        private readonly Label destacado = new Label { Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        private readonly DataGridView resultados = VistaResultados.Grilla("Reinversión", "TEA", "Ganancia", "Capital final");

        public Form1()
        {
            InitializeComponent();
            Text = "Comparador de plazos fijos";
            Font = SystemFonts.MessageBoxFont;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(680, 330);
            ClientSize = new Size(780, 310);

            var capital = new NumericUpDown
            {
                Minimum = 1, Maximum = 1000000000, DecimalPlaces = 2,
                ThousandsSeparator = true, Increment = 10000, Width = 145,
                Value = CalculadoraInversiones.CapitalInicial, AccessibleName = "Capital inicial"
            };
            capital.ValueChanged += (s, e) =>
            {
                CalculadoraInversiones.CapitalInicial = capital.Value;
                ActualizarDashboard();
            };

            var administrar = new Button { Text = "Administrar bancos", AutoSize = true };
            administrar.Click += (s, e) =>
            {
                using (var ventana = new AdministrarBancosForm()) ventana.ShowDialog(this);
                ActualizarDashboard();
            };
            var comparar = new Button { Text = "Ver todos los bancos", AutoSize = true };
            comparar.Click += (s, e) =>
            {
                using (var ventana = new ComparacionForm()) ventana.ShowDialog(this);
            };

            var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            barra.Controls.Add(new Label { Text = "Capital inicial ($)", AutoSize = true, Anchor = AnchorStyles.Left });
            barra.Controls.Add(capital);
            barra.Controls.Add(administrar);
            barra.Controls.Add(comparar);

            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 3 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.Controls.Add(barra, 0, 0);
            panel.Controls.Add(destacado, 0, 1);
            panel.Controls.Add(resultados, 0, 2);
            Controls.Add(panel);
            ActualizarDashboard();
        }

        private void ActualizarDashboard()
        {
            var mejores = BancoStore.Instancia.Bancos.Where(b => b.TnaPromedio.HasValue)
                .GroupBy(b => b.TnaPromedio.Value).OrderByDescending(g => g.Key).FirstOrDefault();
            resultados.Visible = mejores != null;
            if (mejores == null)
            {
                destacado.Text = "Cargá las TNA de los tres años de al menos un banco para ver el resultado.";
                return;
            }
            destacado.Text = string.Join(", ", mejores.Select(b => b.Nombre)) +
                "  ·  Mayor TNA promedio " + VistaResultados.Tasa(mejores.Key);
            VistaResultados.MostrarDetalle(resultados, mejores.First(), true);
        }

        internal static void EstilizarBoton(Button boton, bool principal)
        {
            boton.UseVisualStyleBackColor = true;
        }
    }

    internal static class VistaResultados
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

        internal static string Tasa(decimal valor)
        {
            return valor.ToString("N2", Cultura) + " %";
        }

        internal static DataGridView Grilla(params string[] columnas)
        {
            var grilla = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, BackgroundColor = SystemColors.Window,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            foreach (string titulo in columnas)
            {
                int indice = grilla.Columns.Add(titulo, titulo);
                grilla.Columns[indice].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            return grilla;
        }

        internal static void MostrarDetalle(DataGridView grilla, Banco banco, bool marcarMejor)
        {
            grilla.Rows.Clear();
            if (banco == null || !banco.TnaPromedio.HasValue) return;
            var opciones = CalculadoraInversiones.CalcularTodos(new[] { banco });
            decimal mayor = opciones.Max(r => r.CapitalFinal);
            foreach (var r in opciones)
            {
                string modalidad = r.Modalidad.ToString();
                if (marcarMejor && r.CapitalFinal == mayor) modalidad += " · Mejor";
                grilla.Rows.Add(modalidad, Tasa(r.Tea), r.Ganancia.ToString("C2", Cultura),
                    r.CapitalFinal.ToString("C2", Cultura));
            }
            grilla.ClearSelection();
        }
    }
}
