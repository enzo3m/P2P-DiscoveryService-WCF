using System;
using System.Windows;
using System.Windows.Threading;

namespace CalcDesktopApp
{
    /// <summary>
    /// Questa classe contiene il metodo che permette di aggiornare una vista nello stesso thread della UI.
    /// </summary>
    internal class UiThreadUpdater
    {
        /// <summary>
        /// Questo metodo consente l'aggiornamento di una vista nello stesso thread dell'interfaccia utente.
        /// </summary>
        /// <param name="action">l'azione da invocare per l'aggiornamento</param>
        public void Update(Action action)
        {
            Dispatcher dispatcher = Application.Current.Dispatcher;
            dispatcher.Invoke(action);
        }
    }
}
