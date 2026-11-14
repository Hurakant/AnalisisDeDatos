using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalisisDatos
{
    public sealed class BancoStore
    {
        private static readonly BancoStore _instancia = new BancoStore();
        public static BancoStore Instancia { get { return _instancia; } }
        private readonly List<Banco> _bancos = new List<Banco>();
        public event EventHandler BancosCambiaron;
        public IList<Banco> Bancos { get { return _bancos.AsReadOnly(); } }

        private BancoStore()
        {
            _bancos.Add(new Banco("Banco Provincia", null, null, null));
            _bancos.Add(new Banco("Banco Nación", null, null, null));
            _bancos.Add(new Banco("Banco Hipotecario", null, null, null));
        }

        public void Agregar(Banco banco) { Validar(banco, null); _bancos.Add(banco); Notificar(); }
        public void Modificar(Banco banco)
        {
            Validar(banco, banco.Id);
            int indice = _bancos.FindIndex(b => b.Id == banco.Id);
            if (indice < 0) throw new InvalidOperationException("No se encontró el banco seleccionado.");
            _bancos[indice] = banco; Notificar();
        }
        public void Eliminar(Guid id) { _bancos.RemoveAll(b => b.Id == id); Notificar(); }
        private void Validar(Banco banco, Guid? idIgnorado)
        {
            if (banco == null || string.IsNullOrWhiteSpace(banco.Nombre)) throw new ArgumentException("El nombre del banco es obligatorio.");
            if (_bancos.Any(b => b.Id != idIgnorado && string.Equals(b.Nombre.Trim(), banco.Nombre.Trim(), StringComparison.CurrentCultureIgnoreCase)))
                throw new ArgumentException("Ya existe un banco con ese nombre.");
            foreach (var tasa in new decimal?[] { banco.TnaAnio1, banco.TnaAnio2, banco.TnaAnio3 })
                if (tasa.HasValue && (tasa.Value < 0m || tasa.Value > 1000m)) throw new ArgumentException("Las tasas deben estar entre 0 y 1000 %.");
        }
        private void Notificar() { var evento = BancosCambiaron; if (evento != null) evento(this, EventArgs.Empty); }
    }
}
