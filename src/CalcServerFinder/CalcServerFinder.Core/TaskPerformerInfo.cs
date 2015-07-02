using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalcServerFinder.Core
{
    /// <summary>
    /// Definisce le informazioni relative ad un componente di elaborazione task.
    /// </summary>
    public class TaskPerformerInfo : IEquatable<TaskPerformerInfo>
    {
        #region Fields

        private readonly string m_TaskPerformerName;
        private readonly string m_TaskPerformerVersion;
        private readonly int m_HashCode;

        #endregion

        #region Properties

        /// <summary>
        /// Il nome completo della classe che implementa la risorsa di calcolo.
        /// </summary>
        public string Name { get { return m_TaskPerformerName; } }

        /// <summary>
        /// La versione della classe che implementa la risorsa di calcolo.
        /// </summary>
        public string Version { get { return m_TaskPerformerVersion; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di questa classe utilizzando l'identificatore specificato come stringa contenente
        /// il nome completo e la versione della classe che implementa la risorsa di calcolo, separate dal carattere
        /// trattino-meno.
        /// </summary>
        /// <param name="identifier">
        /// La stringa contenente il nome completo e la versione della classe che implementa la risorsa di calcolo.
        /// </param>
        /// <remarks>
        /// Se si specifica un identificatore null oppure una stringa vuota o costituita soltanto da spazi bianchi,
        /// il nome e la versione vengono impostati entrambi come stringhe vuote. In caso contrario, viene cercata
        /// la prima occorrenza del carattere trattino-meno all'interno dell'identificatore, per poter separare il
        /// nome della classe dalla versione: se tale carattere separatore non viene trovato, il nome della classe
        /// coinciderà con l'identificatore specificato e la versione sarà impostata come stringa vuota; se invece
        /// il carattere separatore viene trovato, il nome viene impostato con la sottostringa dell'identificatore
        /// che lo precede, mentre la versione con la sottostringa successiva al separatore.
        /// </remarks>
        public TaskPerformerInfo(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                m_TaskPerformerName = string.Empty;
                m_TaskPerformerVersion = string.Empty;
            }
            else
            {
                string[] split = identifier.Split(new char[] { '-' }, 2);

                m_TaskPerformerName = (split.Length < 1 ? string.Empty : split[0]);
                m_TaskPerformerVersion = (split.Length < 2 ? string.Empty : split[1]);
            }
            m_HashCode = (m_TaskPerformerName + m_TaskPerformerVersion).GetHashCode();
        }

        #endregion

        #region IEquatable Implementation

        /// <summary>
        /// Determina se questa istanza e un altro oggetto TaskPerformerInfo specificato hanno lo stesso valore.
        /// </summary>
        /// <param name="other">l'oggetto con cui confrontare questa istanza</param>
        /// <returns>true se l'oggetto corrente è uguale al parametro other; in caso contrario, false.</returns>
        public bool Equals(TaskPerformerInfo other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;

            return (Name.Equals(other.Name) && Version.Equals(other.Version));
        }

        #endregion

        #region Override

        /// <summary>
        /// Restituisce la rappresentazione in formato stringa di questo oggetto, ottenuta concatenando
        /// le proprietà Name e Version e separandole con un singolo carattere trattino-meno.
        /// </summary>
        /// <returns>la rappresentazione in formato stringa di questo oggetto</returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}", Name, Version);
        }

        /// <summary>
        /// Restituisce il codice hash di questo oggetto.
        /// </summary>
        /// <returns>il codice hash di questo oggetto</returns>
        public override int GetHashCode()
        {
            return m_HashCode;
        }

        /// <summary>
        /// Determina se questa istanza e un oggetto specificato, che deve essere anche un oggetto TaskPerformerInfo,
        /// hanno lo stesso valore.
        /// </summary>
        /// <param name="obj">oggetto TaskPerformerInfo da confrontare con questa istanza</param>
        /// <returns>
        /// true se il parametro obj è un oggetto TaskPerformerInfo e il relativo valore corrisponde a quello
        /// di questa istanza; in caso contrario false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is TaskPerformerInfo)) return false;
            return Equals((TaskPerformerInfo)obj);
        }

        #endregion

        #region Overloading

        public static bool operator ==(TaskPerformerInfo left, TaskPerformerInfo right)
        {
            if (object.ReferenceEquals(left, right)) return true;
            if (object.ReferenceEquals(left, null)) return false;
            if (object.ReferenceEquals(right, null)) return false;

            return left.Equals(right);
        }

        public static bool operator !=(TaskPerformerInfo left, TaskPerformerInfo right)
        {
            if (object.ReferenceEquals(left, right)) return false;
            if (object.ReferenceEquals(left, null)) return true;
            if (object.ReferenceEquals(right, null)) return true;

            return !left.Equals(right);
        }

        #endregion
    }
}
