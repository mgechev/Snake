using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Linq;

namespace Snake
{
    public class Options
    {
        public const string OPTIONS_FILE = "options.dat";
        private static string[] defaultOptions = { "Green", "Green", "800", "600", "5",
                                                   "White", "DarkGray", 
                                                   "True", "False"};
        static public void FixOptionsFile()
        {
            try
            {
                FileStream file = new FileStream(Results.RESULTS_FILE, FileMode.Create);
                StreamWriter write = new StreamWriter(file);
                for (int i = 0; i < defaultOptions.Length; i++)
                    write.WriteLine(defaultOptions[i]);                    

                write.Flush();
                write.Close();
                file.Close();
            }
            catch (Exception)
            {
            }
        }

        static public List<string> LoadOptions()
        {
            List<string> options = new List<string>();
            try
            {
                FileStream file = new FileStream(Options.OPTIONS_FILE, FileMode.Open);
                StreamReader readFile = new StreamReader(file);
                string option = null;
                while ((option = readFile.ReadLine()) != null && option.Length > 0)
                {
                    if (option.Length > 0)
                        options.Add(option);
                }
                readFile.Close();
                file.Close();

                if (options.Count != defaultOptions.Length)
                {
                    throw new Exception("Options file is broken!");
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                FixOptionsFile();
                options = defaultOptions.ToList<string>();
            }
            return options;
        }
    }
}
