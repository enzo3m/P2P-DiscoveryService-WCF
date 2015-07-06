using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CalcPhoneApp
{
    /// <summary>
    /// La classe di base per ogni classe che implementa un ViewModel.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler m_PropertyChanged;

        /// <summary>
        /// Dichiara l'evento che verrà lanciato quando cambia una proprietà.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { m_PropertyChanged += value; }
            remove { m_PropertyChanged -= value; }
        }

        /// <summary>
        /// Permette di lanciare un evento per segnalare il cambiamento di una proprietà.
        /// </summary>
        /// <param name="propertyName">il nome della proprietà cambiata</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Permette di lanciare un evento per segnalare il cambiamento di una proprietà.
        /// </summary>
        /// <param name="e">le informazioni relative alla proprietà cambiata</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (m_PropertyChanged != null)
            {
                m_PropertyChanged(this, e);
            }
        }
    }
}
