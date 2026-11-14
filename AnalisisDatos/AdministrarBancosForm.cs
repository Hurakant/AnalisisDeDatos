using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AnalisisDatos
{
    public class AdministrarBancosForm : Form
    {
        private readonly DataGridView _grid = new DataGridView();
        public AdministrarBancosForm() { ConstruirInterfaz(); CargarBancos(); BancoStore.Instancia.BancosCambiaron += CambioBancos; FormClosed += (s,e) => BancoStore.Instancia.BancosCambiaron -= CambioBancos; }
        private void ConstruirInterfaz()
        {
            Text = "Administrar bancos"; StartPosition = FormStartPosition.CenterParent; Size = new Size(820, 510); MinimumSize = new Size(700,420); BackColor = Color.FromArgb(245,247,250);
            var titulo = new Label { Text = "Bancos y tasas", Font = new Font("Segoe UI",18,FontStyle.Bold), ForeColor=Color.FromArgb(26,43,69), AutoSize=true, Location=new Point(24,20) };
            var ayuda = new Label { Text = "Ingresá la TNA de cada uno de los últimos tres años. Los campos pueden quedar vacíos mientras completás el historial.", Font=new Font("Segoe UI",9), ForeColor=Color.FromArgb(91,104,123), AutoSize=true, Location=new Point(26,58) };
            _grid.Location=new Point(24,92); _grid.Size=new Size(756,300); _grid.Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right; _grid.ReadOnly=true; _grid.AllowUserToAddRows=false; _grid.AllowUserToDeleteRows=false; _grid.RowHeadersVisible=false; _grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect; _grid.MultiSelect=false; _grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill; _grid.BackgroundColor=Color.White; _grid.BorderStyle=BorderStyle.None; _grid.ColumnHeadersHeight=36; _grid.RowTemplate.Height=34; _grid.EnableHeadersVisualStyles=false; _grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(237,242,247);
            _grid.Columns.Add("id","id"); _grid.Columns[0].Visible=false; _grid.Columns.Add("nombre","Banco"); _grid.Columns.Add("t1", (DateTime.Today.Year-2).ToString()); _grid.Columns.Add("t2", (DateTime.Today.Year-1).ToString()); _grid.Columns.Add("t3", DateTime.Today.Year.ToString()); _grid.Columns.Add("promedio","TNA promedio");
            var pie=new FlowLayoutPanel { Dock=DockStyle.Bottom, Height=64, FlowDirection=FlowDirection.RightToLeft, Padding=new Padding(16,10,16,8), WrapContents=false };
            var eliminar=Boton("Eliminar",false); eliminar.Click+=EliminarClick; var modificar=Boton("Modificar",false); modificar.Click+=ModificarClick; var agregar=Boton("Agregar banco",true); agregar.Click+=AgregarClick; var cerrar=Boton("Cerrar",false); cerrar.DialogResult=DialogResult.Cancel; pie.Controls.Add(cerrar); pie.Controls.Add(eliminar); pie.Controls.Add(modificar); pie.Controls.Add(agregar);
            Controls.Add(_grid); Controls.Add(titulo); Controls.Add(ayuda); Controls.Add(pie); AcceptButton=agregar; CancelButton=cerrar;
        }
        private Button Boton(string texto,bool principal) { var b=new Button { Text=texto, Width=125, Height=38, Margin=new Padding(8,0,0,0) }; Form1.EstilizarBoton(b, principal); return b; }
        private void CargarBancos()
        {
            _grid.Rows.Clear(); foreach(var b in BancoStore.Instancia.Bancos) { int i=_grid.Rows.Add(b.Id,b.Nombre,Formato(b.TnaAnio1),Formato(b.TnaAnio2),Formato(b.TnaAnio3),b.TnaPromedio.HasValue?b.TnaPromedio.Value.ToString("N2")+"%":"Pendiente"); _grid.Rows[i].Tag=b.Id; }
        }
        private string Formato(decimal? n) { return n.HasValue?n.Value.ToString("N2")+"%":"—"; }
        private Guid? Seleccion { get { return _grid.CurrentRow==null? (Guid?)null : (Guid)_grid.CurrentRow.Cells[0].Value; } }
        private void AgregarClick(object s,EventArgs e) { using(var f=new BancoEditForm(null)) if(f.ShowDialog(this)==DialogResult.OK) BancoStore.Instancia.Agregar(f.BancoEditado); }
        private void ModificarClick(object s,EventArgs e) { var id=Seleccion; if(!id.HasValue) { MessageBox.Show(this,"Seleccioná un banco para modificar.","Administrar bancos",MessageBoxButtons.OK,MessageBoxIcon.Information); return; } var actual=BancoStore.Instancia.Bancos.First(b=>b.Id==id.Value); using(var f=new BancoEditForm(actual)) if(f.ShowDialog(this)==DialogResult.OK) { f.BancoEditado.Id=actual.Id; BancoStore.Instancia.Modificar(f.BancoEditado); } }
        private void EliminarClick(object s,EventArgs e) { var id=Seleccion; if(!id.HasValue) { MessageBox.Show(this,"Seleccioná un banco para eliminar.","Administrar bancos",MessageBoxButtons.OK,MessageBoxIcon.Information); return; } var banco=BancoStore.Instancia.Bancos.First(b=>b.Id==id.Value); if(MessageBox.Show(this,"¿Querés eliminar "+banco.Nombre+"?","Confirmar eliminación",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)==DialogResult.Yes) BancoStore.Instancia.Eliminar(id.Value); }
        private void CambioBancos(object s,EventArgs e) { if(InvokeRequired) BeginInvoke(new Action(CargarBancos)); else CargarBancos(); }
    }

    internal class BancoEditForm : Form
    {
        private readonly TextBox _nombre=new TextBox(); private readonly TextBox[] _tasas={new TextBox(),new TextBox(),new TextBox()};
        public Banco BancoEditado { get; private set; }
        public BancoEditForm(Banco original)
        {
            Text=original==null?"Agregar banco":"Modificar banco"; StartPosition=FormStartPosition.CenterParent; FormBorderStyle=FormBorderStyle.FixedDialog; MaximizeBox=false; MinimizeBox=false; ShowInTaskbar=false; ClientSize=new Size(470,310); BackColor=Color.White;
            var titulo=new Label { Text=Text, Font=new Font("Segoe UI",16,FontStyle.Bold), ForeColor=Color.FromArgb(26,43,69), AutoSize=true, Location=new Point(24,18) }; Controls.Add(titulo);
            var etiqueta=new Label { Text="Nombre del banco", Font=new Font("Segoe UI",9,FontStyle.Bold), AutoSize=true, Location=new Point(26,68) }; _nombre.Location=new Point(28,91); _nombre.Width=410; _nombre.Font=new Font("Segoe UI",10); Controls.Add(etiqueta); Controls.Add(_nombre);
            var anio=DateTime.Today.Year-2; for(int i=0;i<3;i++) { var l=new Label { Text="TNA "+(anio+i)+" (%)",AutoSize=true,Font=new Font("Segoe UI",9,FontStyle.Bold),Location=new Point(28,134+i*42) }; _tasas[i].Location=new Point(175,130+i*42); _tasas[i].Width=263; _tasas[i].Font=new Font("Segoe UI",10); Controls.Add(l); Controls.Add(_tasas[i]); }
            var guardar=new Button { Text="Guardar",Location=new Point(310,258),Size=new Size(126,36) }; Form1.EstilizarBoton(guardar,true); guardar.Click+=GuardarClick; var cancelar=new Button { Text="Cancelar",Location=new Point(178,258),Size=new Size(118,36),DialogResult=DialogResult.Cancel }; Form1.EstilizarBoton(cancelar,false); Controls.Add(guardar); Controls.Add(cancelar); AcceptButton=guardar; CancelButton=cancelar;
            if(original!=null) { _nombre.Text=original.Nombre; _tasas[0].Text=original.TnaAnio1.HasValue?original.TnaAnio1.Value.ToString(CultureInfo.GetCultureInfo("es-AR")):""; _tasas[1].Text=original.TnaAnio2.HasValue?original.TnaAnio2.Value.ToString(CultureInfo.GetCultureInfo("es-AR")):""; _tasas[2].Text=original.TnaAnio3.HasValue?original.TnaAnio3.Value.ToString(CultureInfo.GetCultureInfo("es-AR")):""; }
        }
        private void GuardarClick(object sender,EventArgs e)
        {
            if(String.IsNullOrWhiteSpace(_nombre.Text)) { MessageBox.Show(this,"El nombre del banco es obligatorio.","Dato requerido",MessageBoxButtons.OK,MessageBoxIcon.Warning); _nombre.Focus(); return; }
            decimal?[] tasas=new decimal?[3]; for(int i=0;i<3;i++) { string s=_tasas[i].Text.Trim(); if(s.Length==0) continue; decimal valor; s=s.Replace("%","").Replace(" ","").Replace(",","."); if(!Decimal.TryParse(s,NumberStyles.AllowDecimalPoint|NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out valor)||valor<0||valor>1000) { MessageBox.Show(this,"Ingresá una tasa numérica entre 0 y 1000, o dejá el campo vacío.","Tasa inválida",MessageBoxButtons.OK,MessageBoxIcon.Warning); _tasas[i].Focus(); return; } tasas[i]=valor; }
            BancoEditado=new Banco(_nombre.Text.Trim(),tasas[0],tasas[1],tasas[2]); DialogResult=DialogResult.OK; Close();
        }
    }
}
