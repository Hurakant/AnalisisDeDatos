using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AnalisisDatos
{
    public partial class Form1 : Form
    {
        private readonly FlowLayoutPanel _contenedor = new FlowLayoutPanel();
        private readonly Label _estado = new Label();
        private readonly Button _administrar = new Button();
        private readonly Button _verTodos = new Button();
        private readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("es-AR");

        // NUEVO
        private readonly NumericUpDown _capital = new NumericUpDown();
        private readonly Label _subtitulo = new Label();

        public Form1()
        {
            InitializeComponent();
            ConfigurarVentana();
            BancoStore.Instancia.BancosCambiaron += BancosCambiaron;
            ActualizarDashboard();
        }

        private void ConfigurarVentana()
        {
            Text = "Análisis de TNA | Comparador de plazos fijos"; StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(780, 570); Size = new Size(980, 700); BackColor = Color.FromArgb(245, 247, 250);
            var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(28) };
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 76)); raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            var cabecera = new Panel { Dock = DockStyle.Fill };
            var titulo = new Label { Text = "Comparador de inversiones", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(26, 43, 69), AutoSize = true, Location = new Point(0, 0) };

            _subtitulo.Font = new Font("Segoe UI", 9.5f);
            _subtitulo.ForeColor = Color.FromArgb(91, 104, 123);
            _subtitulo.AutoSize = true;
            _subtitulo.Location = new Point(2, 42);

            _administrar.Text = "Administrar bancos"; _administrar.Size = new Size(155, 38); _administrar.Anchor = AnchorStyles.Top | AnchorStyles.Right; _administrar.Location = new Point(Width - 245, 13); EstilizarBoton(_administrar, true); _administrar.Click += AdministrarClick;

            var lblCapital = new Label
            {
                Text = "Capital inicial",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(91, 104, 123),
                AutoSize = true
            };
            _capital.Minimum = 1;
            _capital.Maximum = 1000000000;
            _capital.DecimalPlaces = 2;
            _capital.Increment = 10000;
            _capital.ThousandsSeparator = true;
            _capital.Width = 140;
            _capital.Font = new Font("Segoe UI", 10);
            _capital.Value = CalculadoraInversiones.CapitalInicial;
            _capital.ValueChanged += CapitalCambio;

            cabecera.Controls.Add(titulo); cabecera.Controls.Add(_subtitulo); cabecera.Controls.Add(_administrar);
            cabecera.Controls.Add(lblCapital);
            cabecera.Controls.Add(_capital);

            cabecera.Resize += (s, e) => _administrar.Location = new Point(cabecera.ClientSize.Width - _administrar.Width, 13);

            cabecera.Resize += (s, e) =>
            {
                _capital.Location = new Point(_administrar.Left - _capital.Width - 20, 18);
                lblCapital.Location = new Point(_capital.Left, 0);
            };

            _contenedor.Dock = DockStyle.Fill; _contenedor.FlowDirection = FlowDirection.TopDown; _contenedor.WrapContents = false; _contenedor.AutoScroll = true; _contenedor.Padding = new Padding(0, 14, 12, 14);
            var pie = new Panel { Dock = DockStyle.Fill }; _estado.AutoSize = true; _estado.Location = new Point(2, 20); _estado.ForeColor = Color.FromArgb(91, 104, 123); _estado.Font = new Font("Segoe UI", 9);
            _verTodos.Text = "Ver todos los bancos"; _verTodos.Size = new Size(190, 40); _verTodos.Anchor = AnchorStyles.Top | AnchorStyles.Right; EstilizarBoton(_verTodos, false); _verTodos.Click += VerTodosClick;
            pie.Controls.Add(_estado); pie.Controls.Add(_verTodos); pie.Resize += (s, e) => _verTodos.Location = new Point(pie.ClientSize.Width - _verTodos.Width, 8);
            raiz.Controls.Add(cabecera, 0, 0); raiz.Controls.Add(_contenedor, 0, 1); raiz.Controls.Add(pie, 0, 2); Controls.Add(raiz);
        }

        private void CapitalCambio(object sender, EventArgs e)
        {
            CalculadoraInversiones.CapitalInicial = _capital.Value;
            ActualizarDashboard();
        }

        private void ActualizarDashboard()
        {
            // NUEVO
            _subtitulo.Text = "Tasas promedio de los últimos tres años · Capital inicial: "
                              + Formatear(CalculadoraInversiones.CapitalInicial);

            _contenedor.Controls.Clear();
            var bancos = BancoStore.Instancia.Bancos.Where(b => b.TnaPromedio.HasValue).ToList();
            if (bancos.Count == 0)
            {
                _estado.Text = "Cargá las tasas anuales de tres años para ver el análisis.";
                var aviso = new Panel { Width = 810, Height = 210, BackColor = Color.White, Margin = new Padding(0, 18, 0, 0), Padding = new Padding(26) };
                aviso.Controls.Add(new Label { Text = "Todavía no hay bancos con historial completo", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(26, 43, 69), AutoSize = true, Location = new Point(26, 38) });
                aviso.Controls.Add(new Label { Text = "Cada banco necesita las tasas TNA de los tres últimos años.\nAdministrá los bancos para cargar o completar esos valores.", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(91, 104, 123), AutoSize = true, Location = new Point(28, 92) });
                _contenedor.Controls.Add(aviso); return;
            }
            decimal mayorTna = bancos.Max(b => b.TnaPromedio.Value);
            var mejores = bancos.Where(b => b.TnaPromedio.Value == mayorTna).ToList();
            _estado.Text = "Mostrando " + mejores.Count + (mejores.Count == 1 ? " banco destacado." : " bancos empatados en la mayor TNA.");
            foreach (var banco in mejores) _contenedor.Controls.Add(CrearTarjeta(banco));
        }

        private Control CrearTarjeta(Banco banco)
        {
            var tarjeta = new Panel { Width = Math.Max(680, _contenedor.ClientSize.Width - 35), Height = 340, BackColor = Color.White, Margin = new Padding(0, 0, 0, 18), Padding = new Padding(24) };
            var nombre = new Label { Text = banco.Nombre, AutoSize = true, Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Color.FromArgb(26, 43, 69), Location = new Point(24, 20) }; tarjeta.Controls.Add(nombre);
            var promedio = new Label { Text = "TNA promedio: " + banco.TnaPromedio.Value.ToString("N2", _cultura) + "%", AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(28, 117, 92), Location = new Point(26, 59) }; tarjeta.Controls.Add(promedio);
            var anios = new Label { Text = "Historial: " + (DateTime.Today.Year - 2) + "  ·  " + (DateTime.Today.Year - 1) + "  ·  " + DateTime.Today.Year, AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(91, 104, 123), Location = new Point(26, 88) }; tarjeta.Controls.Add(anios);
            var grid = new DataGridView { Location = new Point(24, 126), Width = tarjeta.Width - 48, Height = 175, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 242, 247); grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(45, 59, 79); grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersHeight = 34; grid.RowTemplate.Height = 36;
            grid.Columns.Add("periodo", "Reinversión"); grid.Columns.Add("tea", "TEA"); grid.Columns.Add("ganancia", "Ganancia"); grid.Columns.Add("capital", "Capital final");
            ResultadoInversion global = null;
            foreach (ModalidadInversion m in Enum.GetValues(typeof(ModalidadInversion))) { var r = CalculadoraInversiones.Calcular(banco, m); grid.Rows.Add(NombreModalidad(m), r.Tea.ToString("N2", _cultura) + "%", Formatear(r.Ganancia), Formatear(r.CapitalFinal)); if (global == null || r.CapitalFinal > global.CapitalFinal) global = r; }
            tarjeta.Controls.Add(grid);
            tarjeta.Controls.Add(new Label { Text = "Mejor alternativa global: " + NombreModalidad(global.Modalidad) + " · " + Formatear(global.CapitalFinal), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(28, 117, 92), Location = new Point(26, 309) });
            return tarjeta;
        }

        private void BancosCambiaron(object sender, EventArgs e) { if (InvokeRequired) { BeginInvoke(new Action(ActualizarDashboard)); return; } ActualizarDashboard(); }
        private void AdministrarClick(object sender, EventArgs e) { using (var f = new AdministrarBancosForm()) f.ShowDialog(this); }
        private void VerTodosClick(object sender, EventArgs e) { using (var f = new ComparacionForm()) f.ShowDialog(this); }
        private string Formatear(decimal valor) { return valor.ToString("C2", _cultura); }
        internal static string NombreModalidad(ModalidadInversion modalidad) { return modalidad == ModalidadInversion.Anual ? "Anual" : modalidad == ModalidadInversion.Trimestral ? "Trimestral" : "Mensual"; }
        internal static void EstilizarBoton(Button boton, bool principal) { boton.FlatStyle = FlatStyle.Flat; boton.FlatAppearance.BorderSize = principal ? 0 : 1; boton.FlatAppearance.BorderColor = Color.FromArgb(43, 93, 153); boton.BackColor = principal ? Color.FromArgb(35, 83, 139) : Color.White; boton.ForeColor = principal ? Color.White : Color.FromArgb(35, 83, 139); boton.Font = new Font("Segoe UI", 9, FontStyle.Bold); boton.Cursor = Cursors.Hand; }
    }
}