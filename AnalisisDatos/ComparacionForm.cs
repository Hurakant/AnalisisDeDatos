using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AnalisisDatos
{
    public class ComparacionForm : Form
    {
        private readonly DataGridView _grid=new DataGridView();
        private readonly Label _resumen=new Label();
        private readonly CultureInfo _cultura=CultureInfo.GetCultureInfo("es-AR");
        public ComparacionForm() { ConstruirInterfaz(); CargarComparacion(); BancoStore.Instancia.BancosCambiaron+=CambioBancos; FormClosed+=(s,e)=>BancoStore.Instancia.BancosCambiaron-=CambioBancos; }
        private void ConstruirInterfaz()
        {
            Text="Comparación de todos los bancos"; StartPosition=FormStartPosition.CenterParent; Size=new Size(1260,650); MinimumSize=new Size(900,500); BackColor=Color.FromArgb(245,247,250);
            var titulo=new Label { Text="Comparación completa",Font=new Font("Segoe UI",18,FontStyle.Bold),ForeColor=Color.FromArgb(26,43,69),AutoSize=true,Location=new Point(24,20) };
            var info=new Label { Text="Capital inicial: $ 850.000,00 · Se reinvierte el capital y los intereses al cierre de cada período.",Font=new Font("Segoe UI",9),ForeColor=Color.FromArgb(91,104,123),AutoSize=true,Location=new Point(26,58) };
            _grid.Location=new Point(24,94); _grid.Size=new Size(1190,440); _grid.Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right; _grid.ReadOnly=true; _grid.AllowUserToAddRows=false; _grid.AllowUserToDeleteRows=false; _grid.RowHeadersVisible=false; _grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect; _grid.MultiSelect=false; _grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells; _grid.BackgroundColor=Color.White; _grid.BorderStyle=BorderStyle.None; _grid.ColumnHeadersHeight=42; _grid.RowTemplate.Height=36; _grid.EnableHeadersVisualStyles=false; _grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(237,242,247); _grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(249,251,253);
            AddCol("nombre","Banco"); int y=DateTime.Today.Year-2; AddCol("tna1","TNA "+y+" (%)"); AddCol("tna2","TNA "+(y+1)+" (%)"); AddCol("tna3","TNA "+(y+2)+" (%)"); AddCol("promedio","TNA promedio (%)");
            AddCol("teaA","TEA anual (%)"); AddCol("ganA","Ganancia anual ($)"); AddCol("capA","Capital final anual ($)"); AddCol("teaT","TEA trimestral (%)"); AddCol("ganT","Ganancia trimestral ($)"); AddCol("capT","Capital final trimestral ($)"); AddCol("teaM","TEA mensual (%)"); AddCol("ganM","Ganancia mensual ($)"); AddCol("capM","Capital final mensual ($)");
            _resumen.Location=new Point(26,550); _resumen.Anchor=AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right; _resumen.AutoSize=true; _resumen.Font=new Font("Segoe UI",9.5f,FontStyle.Bold); _resumen.ForeColor=Color.FromArgb(28,117,92);
            var cerrar=new Button { Text="Cerrar",Width=110,Height=36,Anchor=AnchorStyles.Bottom|AnchorStyles.Right }; Form1.EstilizarBoton(cerrar,false); cerrar.Click+=(s,e)=>Close(); cerrar.Location=new Point(Width-150,550); Resize+=(s,e)=>cerrar.Location=new Point(ClientSize.Width-150,ClientSize.Height-52);
            Controls.Add(titulo); Controls.Add(info); Controls.Add(_grid); Controls.Add(_resumen); Controls.Add(cerrar);
        }
        private void AddCol(string key,string title) { _grid.Columns.Add(key,title); }
        private string Tasa(decimal? n) { return n.HasValue?n.Value.ToString("N2",_cultura):"Pendiente"; }
        private string Dinero(decimal n) { return n.ToString("N2",_cultura); }
        private void CargarComparacion()
        {
            _grid.Rows.Clear(); _resumen.Text=""; var calculables=CalculadoraInversiones.CalcularTodos(BancoStore.Instancia.Bancos); decimal? max=calculables.Count==0?(decimal?)null:calculables.Max(r=>r.CapitalFinal);
            foreach(var b in BancoStore.Instancia.Bancos)
            {
                var anual=CalculadoraInversiones.Calcular(b,ModalidadInversion.Anual); var trimestral=CalculadoraInversiones.Calcular(b,ModalidadInversion.Trimestral); var mensual=CalculadoraInversiones.Calcular(b,ModalidadInversion.Mensual);
                int row=_grid.Rows.Add(b.Nombre,Tasa(b.TnaAnio1),Tasa(b.TnaAnio2),Tasa(b.TnaAnio3),b.TnaPromedio.HasValue?Tasa(b.TnaPromedio):"Pendiente",Tea(anual),Gan(anual),Cap(anual),Tea(trimestral),Gan(trimestral),Cap(trimestral),Tea(mensual),Gan(mensual),Cap(mensual));
                if(max.HasValue && ((anual!=null&&anual.CapitalFinal==max.Value)||(trimestral!=null&&trimestral.CapitalFinal==max.Value)||(mensual!=null&&mensual.CapitalFinal==max.Value))) { _grid.Rows[row].DefaultCellStyle.BackColor=Color.FromArgb(227,245,238); _grid.Rows[row].DefaultCellStyle.Font=new Font(_grid.Font,FontStyle.Bold); }
            }
            if(max.HasValue) { var ganadores=calculables.Where(r=>r.CapitalFinal==max.Value).Select(r=>r.Banco.Nombre+" ("+Form1.NombreModalidad(r.Modalidad).ToLowerInvariant()+")").Distinct(); _resumen.Text="Mayor capital final: "+string.Join(", ",ganadores)+" · "+Dinero(max.Value); }
            else _resumen.Text="Completá el historial de tres años en al menos un banco para habilitar la comparación de inversiones.";
        }
        private string Tea(ResultadoInversion r) { return r==null?"—":r.Tea.ToString("N2",_cultura)+"%"; }
        private string Gan(ResultadoInversion r) { return r==null?"—":Dinero(r.Ganancia); }
        private string Cap(ResultadoInversion r) { return r==null?"—":Dinero(r.CapitalFinal); }
        private void CambioBancos(object s,EventArgs e) { if(InvokeRequired) BeginInvoke(new Action(CargarComparacion)); else CargarComparacion(); }
    }
}
