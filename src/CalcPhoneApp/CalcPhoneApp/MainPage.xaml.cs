using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace CalcPhoneApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();

            this.textBox1.FontSize *= 0.75;
            this.textBox2.FontSize *= 0.75;
            this.textBox3.FontSize *= 0.75;
            this.textBox4.FontSize *= 0.75;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Aggiorna la sorgente del binding.
            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpr != null)
            {
                bindingExpr.UpdateSource();
            }
        }
    }
}