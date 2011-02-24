using System;
using System.Collections.Generic;
using System.IO;

namespace Snake
{
    public class Results
    {
        public const string RESULTS_FILE = "results.dat";
        public static IEnumerable<KeyValuePair<int, KeyValuePair<int, string>>> ReadResults()
        {
            FileStream file = new FileStream(RESULTS_FILE, FileMode.OpenOrCreate);
            StreamReader readFile = new StreamReader(file);
            try
            {
                //opening the file which is containing the results

                string level;//going to contain game levels
                string score;//going to contain game score
                string name;//going to contain player's name

                List<KeyValuePair<int, KeyValuePair<int, string>>> results =
                    new List<KeyValuePair<int, KeyValuePair<int, string>>>();
                //reading the file
                while ((level = readFile.ReadLine()) != null)
                {
                    name = readFile.ReadLine();
                    score = readFile.ReadLine();
                    if (score != null)
                    {
                        //reading the user's name and scores
                        KeyValuePair<int, string> temp =
                            new KeyValuePair<int, string>(Convert.ToInt32(score), name);
                        //reading game's level
                        KeyValuePair<int, KeyValuePair<int, string>> element =
                        new KeyValuePair<int, KeyValuePair<int, string>>(Convert.ToInt16(level), temp);

                        results.Add(element);
                    }
                }

                readFile.Close();
                file.Close();
                return results;
            }
            catch (Exception)
            {
                readFile.Close();
                file.Close();
                FixResults();
            }
            return null;
        }

        private static void FixResults()
        {
            try
            {
                FileStream stream = new FileStream(RESULTS_FILE, FileMode.Truncate);
                stream.Close();
            }
            catch (Exception) { }
        }

        public static void AddResult(int level, string username, int score)
        {
            FileStream resultsFile = new FileStream(RESULTS_FILE, FileMode.Append);
            StreamWriter writeToFile = new StreamWriter(resultsFile);
            writeToFile.WriteLine(level + "\n" + username + "\n" + score);
            writeToFile.Flush();
            writeToFile.Close();
            resultsFile.Close();
        }
    }
}
