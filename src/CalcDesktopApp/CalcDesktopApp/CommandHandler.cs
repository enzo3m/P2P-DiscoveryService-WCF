using System;
using System.Globalization;
using System.Windows.Input;

namespace CalcDesktopApp
{
    /// <summary>
    /// Questa classe semplifica la definizione di un comando da associare ad un controllo dell'interfaccia grafica.
    /// </summary>
    /// <typeparam name="T">Il tipo di parametro passato al comando.</typeparam>
    public class CommandHandler<T> : ICommand
    {
        #region Members

        private readonly Action<T> m_Execute;
        private readonly Predicate<T> m_Condition;

        #endregion

        #region Constructor

        /// <summary>
        /// Permette di istanziare un nuovo gestore del comando e di associarvi l'azione specificata.
        /// </summary>
        /// <param name="execute">l'azione da associare al gestore del comando</param>
        public CommandHandler(Action<T> execute)
            : this(execute, null)
        {
            ;
        }

        /// <summary>
        /// Permette di istanziare un nuovo gestore del comando e di associarvi l'azione specificata
        /// e la condizione di attivazione del comando stesso.
        /// </summary>
        /// <param name="execute">l'azione da associare al gestore del comando</param>
        /// <param name="condition">la condizione di attivazione del comando</param>
        public CommandHandler(Action<T> execute, Predicate<T> condition)
        {
            m_Execute = execute;
            m_Condition = condition;
        }

        #endregion

        #region ICommand Implementation

        /// <summary>
        /// Verifica la condizione di attivazione del comando e restituisce true se il comando deve essere attivato,
        /// altrimenti false se deve essere disattivato.
        /// </summary>
        /// <param name="parameter">l'eventuale parametro passato al comando</param>
        /// <returns>true, se il comando deve essere attivato; in caso contrario, false.</returns>
        public bool CanExecute(object parameter)
        {
            return (m_Condition != null ? m_Condition(GetParameter(parameter)) : true);
        }

        /// <summary>
        /// Si verifica quando si avvengono modifiche che influiscono sull'attivazione del comando.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Esegue l'azione associata a questo gestore del comando.
        /// </summary>
        /// <param name="parameter">l'eventuale parametro passato al comando</param>
        public void Execute(object parameter)
        {
            m_Execute(GetParameter(parameter));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Permette di lanciare l'evento che segnala le modifiche che influiscono sull'attivazione del comando.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converte il parametro specificato nel tipo con cui è stato parametrizzato questo oggetto.
        /// </summary>
        /// <param name="parameter">l'eventuale parametro passato al comando</param>
        /// <returns>il parametro convertito nel tipo con cui è stato parametrizzato questo oggetto</returns>
        private T GetParameter(object parameter)
        {
            return (parameter != null ? (T)Convert.ChangeType(parameter, typeof(T), CultureInfo.CurrentCulture) : default(T));
        }

        #endregion
    }
}
