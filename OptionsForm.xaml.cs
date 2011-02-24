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

namespace Snake
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class OptionsForm : Window
    {
        public OptionsForm()
        {
            InitializeComponent();
            SelectCurrentOptions();
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileStream file = new FileStream(Options.OPTIONS_FILE, FileMode.OpenOrCreate);
                StreamWriter writeFile = new StreamWriter(file);

                GameLevel level =
                    (GameLevel)Enum.Parse(typeof(GameLevel), gameLevel.Text, true);

                char[] separator = new char[] { 'x' };
                string[] resolution = screenSize.Text.Split(separator);

                writeFile.WriteLine(bodyColor.Text);
                writeFile.WriteLine(headColor.Text);
                writeFile.WriteLine(resolution[0]);
                writeFile.WriteLine(resolution[1]);
                writeFile.WriteLine((int)level);
                writeFile.WriteLine(fieldColor.Text);
                writeFile.WriteLine(obstacleColor.Text);
                writeFile.WriteLine(mouseEnabled.IsChecked.ToString());
                writeFile.WriteLine(fullScreen.IsChecked.ToString());
                writeFile.Flush();
                writeFile.Close();
                file.Close();
                Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void SelectCurrentOptions()
        {
            try
            {
                List<string> options = Options.LoadOptions();
                bodyColor.Text = options[0];
                headColor.Text = options[1];
                screenSize.Text = options[2] + "x" + options[3];
                
                string level;
                level = ((GameLevel)Convert.ToInt16(options[4])).ToString();
                level = level[0] + level.Substring(1).ToLower();
                gameLevel.Text = level;

                fieldColor.Text = options[5];
                obstacleColor.Text = options[6];
                if (Convert.ToBoolean(options[7]))
                {
                    mouseEnabled.IsChecked = true;
                }
                if (Convert.ToBoolean(options[8]))
                {
                    fullScreen.IsChecked = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
