
using System.Diagnostics;
/**
* \file About.xaml.cs
* \version PROG2120 - Assignment 4
* \author Shawn Coverini
* \date 2016-11-16
* \brief Interaction logic for About.xaml
*/
using System.Windows;

namespace SETPaint
{

    public partial class About : Window
    {
        
        public About()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okay_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Open link in web browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
