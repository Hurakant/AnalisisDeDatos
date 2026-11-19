using System;
using System.Drawing;
using System.Windows.Forms;

namespace AnalisisDatos
{
    public class ComparacionForm : Form
    {
        private readonly DataGridView bancos = VistaResultados.Grilla("Banco", "TNA promedio", "TEA anual", "TEA trimestral", "TEA mensual");
        private readonly DataGridView detalle = VistaResultados.Grilla("Reinversión", "TEA", "Ganancia", "Capital final");
        private readonly Label estado = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

        public ComparacionForm()
        {
            Text = "Todos los bancos";
            Font = SystemFonts.MessageBoxFont;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(700, 410);
            ClientSize = new Size(800, 470);

            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), RowCount = 3, ColumnCount = 1 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
            panel.Controls.Add(bancos, 0, 0);
            panel.Controls.Add(estado, 0, 1);
            panel.Controls.Add(detalle, 0, 2);
            Controls.Add(panel);

            foreach (var banco in BancoStore.Instancia.Bancos)
            {
                var valores = new object[] { banco.Nombre, "Pendiente", "—", "—", "—" };
                if (banco.TnaPromedio.HasValue)
                {
                    valores[1] = VistaResultados.Tasa(banco.TnaPromedio.Value);
                    int columna = 2;
                    foreach (var resultado in CalculadoraInversiones.CalcularTodos(new[] { banco }))
                        valores[columna++] = VistaResultados.Tasa(resultado.Tea);
                }
                bancos.Rows[bancos.Rows.Add(valores)].Tag = banco;
            }
            bancos.SelectionChanged += (s, e) => ActualizarDetalle();
            ActualizarDetalle();
        }

        private void ActualizarDetalle()
        {
            var banco = bancos.CurrentRow?.Tag as Banco;
            detalle.Visible = banco != null && banco.TnaPromedio.HasValue;
            estado.Text = banco == null ? "Sin bancos cargados." : banco.Nombre;
            if (banco != null && !banco.TnaPromedio.HasValue)
                estado.Text += " · Historial pendiente";
            VistaResultados.MostrarDetalle(detalle, banco, false);
        }
    }
}
