using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalcServerFinder.Core.Extensions
{
    /// <summary>
    /// Metodi di estensione per oggetti DateTime.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Prova a sottrarre l'intervallo di tempo specificato e restituisce true se la sottrazione ha prodotto
        /// un risultato valido; in caso contrario, restituisce false. L'eventuale risultato valido ottenuto con
        /// la sottrazione viene impostato in output.
        /// </summary>
        /// <param name="dateTime">Questa istanza di DateTime a cui sottrarre l'intervallo di tempo specificato.</param>
        /// <param name="value">L'intervallo di tempo da sottrarre a questa istanza di DateTime.</param>
        /// <param name="result">Un oggetto DateTime contenente l'eventuale risultato della sottrazione.</param>
        /// <returns>true se la sottrazione ha prodotto un risultato valido; in caso contrario, false.</returns>
        /// <remarks>
        /// L'oggetto DateTime impostato in output contiene l'eventuale risultato della sottrazione solamente se
        /// questo metodo ha restituito true, altrimenti vuol dire che la sottrazione non può essere effettuata.
        /// </remarks>
        public static bool TrySubtract(this DateTime dateTime, TimeSpan value, out DateTime result)
        {
            if (CanSubtractTimeSpan(dateTime, value))
            {
                result = dateTime.Subtract(value);
                return true;
            }
            else
            {
                result = default(DateTime);
                return false;
            }
        }

        #region Private Methods

        /// <summary>
        /// Verifica se all'istante di tempo specificato è possibile sottrarre l'intervallo di tempo specificato.
        /// </summary>
        /// <param name="dt">L'istante di tempo da verificare.</param>
        /// <param name="ts">L'intervallo di tempo da verificare.</param>
        /// <returns>true se è possibile effettuare la sottrazione; in caso contrario, false.</returns>
        private static bool CanSubtractTimeSpan(DateTime dt, TimeSpan ts)
        {
            TimeSpan maxTimeSpanToSubtract = dt - DateTime.MinValue;   // positivo o zero
            TimeSpan minTimeSpanToSubtract = dt - DateTime.MaxValue;   // negativo o zero

            //return (maxTimeSpanToSubtract >= ts);
            return (minTimeSpanToSubtract <= ts && ts <= maxTimeSpanToSubtract);
        }

        #endregion
    }
}
