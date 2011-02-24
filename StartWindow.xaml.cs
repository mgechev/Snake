using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Threading;

namespace Snake
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

 

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Controller game = new Controller();
            game.Start();
        }

        private string ViewResults(int limit)
        {
            //all results are going to be here
            List<KeyValuePair<int, KeyValuePair<int, string>>> results =
                (List<KeyValuePair<int, KeyValuePair<int, string>>>)Results.ReadResults();
            //if there are no any results returning error
            if (results == null || results.Count <= 0)
                return string.Format("No games have been played.");

            StringBuilder str = new StringBuilder();
            foreach (GameLevel i in Enum.GetValues(typeof(GameLevel)))
            {
                //sorting the array in each level
                var sorted =
                (from res in results
                 where res.Key.Equals((int)i)
                 orderby res.Value.Key descending
                 select res);
                //if there are any results in the current level, adding them to the string
                if (sorted.Count() > 0)
                {
                    int counter = 1;
                    str.Append(i.ToString() + "\n");
                    foreach (var res in sorted)
                    {
                        str.Append(counter.ToString());
                        str.Append(") ");
                        str.Append(res.Value.Value);
                        str.Append(" ");
                        str.Append(res.Value.Key);
                        str.Append("\n");
                        counter++;

                        if (counter > limit)
                            break;
                    }
                    str.Append("\n");
                }
            }
            return str.ToString();
        }

        private void bntResults_Click(object sender, RoutedEventArgs e)
        {
            ResultsForm resultsForm = new ResultsForm();
            resultsForm.txtResults.Text = "Loading...";
            resultsForm.Show();

            Thread loadResults = new Thread(new ThreadStart(new Action(
                () =>
                {
                    string result = ViewResults(5);
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
                        () =>
                        {
                            resultsForm.txtResults.Text = result;
                        }));
                })));
            loadResults.Start();
        }

        private void bntOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsForm optionsForm = new OptionsForm();
            optionsForm.Show();
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            About form = new About();
            form.Show();            
        }
    }
}
