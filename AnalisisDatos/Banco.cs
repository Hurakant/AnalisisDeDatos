using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalisisDatos
{
    public class Banco
    {
        public Guid Id { get; internal set; }
        public string Nombre { get; set; }
        public decimal? TnaAnio1 { get; set; }
        public decimal? TnaAnio2 { get; set; }
        public decimal? TnaAnio3 { get; set; }

        public Banco(string nombre, decimal? tnaAnio1, decimal? tnaAnio2, decimal? tnaAnio3)
        {
            Id = Guid.NewGuid(); Nombre = nombre; TnaAnio1 = tnaAnio1; TnaAnio2 = tnaAnio2; TnaAnio3 = tnaAnio3;
        }

        public decimal? TnaPromedio
        {
            get { return TnaAnio1.HasValue && TnaAnio2.HasValue && TnaAnio3.HasValue ? (TnaAnio1.Value + TnaAnio2.Value + TnaAnio3.Value) / 3m : (decimal?)null; }
        }
    }

    public enum ModalidadInversion { Anual, Trimestral, Mensual }

    public class ResultadoInversion
    {
        public Banco Banco { get; private set; }
        public ModalidadInversion Modalidad { get; private set; }
        public decimal TnaPromedio { get; private set; }
        public decimal Tea { get; private set; }
        public decimal Ganancia { get; private set; }
        public decimal CapitalFinal { get; private set; }

        public ResultadoInversion(Banco banco, ModalidadInversion modalidad, decimal tnaPromedio, decimal tea, decimal ganancia, decimal capitalFinal)
        { Banco = banco; Modalidad = modalidad; TnaPromedio = tnaPromedio; Tea = tea; Ganancia = ganancia; CapitalFinal = capitalFinal; }
    }

    public static class CalculadoraInversiones
    {
        private static decimal _capitalInicial = 850000m;
        public static decimal CapitalInicial
        {
            get { return _capitalInicial; }
            set { _capitalInicial = value; }
        }

        public static ResultadoInversion Calcular(Banco banco, ModalidadInversion modalidad)
        {
            if (banco == null || !banco.TnaPromedio.HasValue) return null;
            int n = modalidad == ModalidadInversion.Anual ? 1 : modalidad == ModalidadInversion.Trimestral ? 4 : 12;
            decimal t = banco.TnaPromedio.Value / 100m;
            double factor = Math.Pow((double)(1m + t / n), n);
            decimal capital = CapitalInicial * (decimal)factor;
            decimal ganancia = capital - CapitalInicial;
            decimal tea = (capital / CapitalInicial - 1m) * 100m;
            return new ResultadoInversion(banco, modalidad, banco.TnaPromedio.Value, tea, ganancia, capital);
        }

        public static List<ResultadoInversion> CalcularTodos(IEnumerable<Banco> bancos)
        {
            var resultados = new List<ResultadoInversion>();
            foreach (var banco in bancos.Where(b => b.TnaPromedio.HasValue))
                foreach (ModalidadInversion modalidad in Enum.GetValues(typeof(ModalidadInversion)))
                    resultados.Add(Calcular(banco, modalidad));
            return resultados;
        }
    }
}