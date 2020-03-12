using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Id3Algorithm.Services
{
    public class DataPreprocessingService
    {
        private List<int[]> _formattedData = null;
        private int[] _decisions = null;

        public List<int[]> FormattedData
        {
            get => _formattedData;
        }

        public int[] Decisions
        {
            get => _decisions;
        }

        public DataPreprocessingService(string path, int decisionColumn, string missingValueCharacter, string separator = ",")
        {
            ReadDataAndSetFormattedData(path, decisionColumn, missingValueCharacter, separator);
        }

        private void ReadDataAndSetFormattedData(string path, int decisionColumn, string missingValueCharacter, string separator)
        {
            List<string[]> rawData = new List<string[]>();
            string[] headers = null;
            // Czytanie danych
            using (StreamReader sr = new StreamReader(path))
            {
                headers = sr.ReadLine().Split(separator);
                string line = null; 
                while ((line = sr.ReadLine()) != null)
                {
                    rawData.Add(line.Split(separator, StringSplitOptions.RemoveEmptyEntries));
                }
            }

            _formattedData = new List<int[]>();            
            // Przygotowywanie danych
            for (int i = 0; i < headers.Length; i++)
            {
                int[] formattedColumn = null;
                if (headers[i].StartsWith("s", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Przygotowanie danych w przypadku kolumny zawierającej łańcuchy znaków
                    var stringColumn = rawData.Select(x => x[i]).ToList();
                    var options = stringColumn.Distinct().ToArray();
                    formattedColumn = new int[stringColumn.Count()];

                    // Przekształcanie łańcuchów znaków na skale od 1 do n
                    for (int j = 0; j < stringColumn.Count(); j++)
                    {
                        int val = -1;
                        if (stringColumn[j] != missingValueCharacter)                            
                            val = Array.IndexOf(options, stringColumn[j]) + 1;
                        formattedColumn[j] = val;
                    }

                    if (formattedColumn.Any(x => x == -1))
                    {
                        // Uzupełnianie brakujących danych
                        int avg = formattedColumn.Where(x => x > -1).Sum() / formattedColumn.Count(x => x > -1);
                        formattedColumn.Where(x => x <= -1).ToList().ForEach(x => x = avg);
                    }
                }
                else
                {
                    // Przygotowanie danych w przypadku kolumny zawierającej liczby
                    var column = rawData.Select(x => double.TryParse(x[i], out double ret) ? ret : double.MinValue);
                    
                    if (column.Any(x => x == double.MinValue))
                    {
                        // Uzupełnianie brakujących danych
                        double avg = column.Where(x => x > double.MinValue).Sum() / column.Count(x => x > double.MinValue);
                        column.Where(x => x <= double.MinValue).ToList().ForEach(x => x = avg);
                    }

                    int distinctElements = column.Distinct().Count();
                    // Tworzenie przedziałów
                    var intervals = CreateIntervals(column.ToList(), distinctElements < 10 ? distinctElements : 10);
                    formattedColumn = new int[column.Count()];

                    // Przekształcanie liczb na skale od 1 do 10
                    for (int j = 0; j < column.Count(); j++)
                    {
                        int val = 0;
                        try
                        {
                            val = FindIntervalForDouble(intervals, column.ElementAt(j));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Błąd podczas wyszukiwania przedziału dla liczby. {0}", ex.Message);
                        }
                        formattedColumn[j] = val;
                    }                    
                }

                if (i == decisionColumn)
                    _decisions = formattedColumn;
                else
                    _formattedData.Add(formattedColumn);
            }
        }

        private int FindIntervalForDouble(double[] intervals, double number)
        {
            for (int i = 1; i < intervals.Length; i++)
            {
                if (number <= intervals[i])
                    return i;
            }

            throw new ArgumentOutOfRangeException("Number", "Number is not match any of intervals");
        }

        private double[] CreateIntervals(List<double> data, int bins)
        {
            double[] intervals = new double[bins];
            if (data.Count > bins)
            {
                intervals[0] = double.MinValue;
                data.Sort();
                double toOneBin = data.Count / (double)bins;
                int allElements = 0;
                int interval = 1;

                for (int i = 0; i < data.Count;)
                {
                    if (i < (int)toOneBin)
                    {
                        allElements++;
                        int j = i + 1;
                        while (j < data.Count && data.ElementAt(j) == data.ElementAt(i))
                        {
                            allElements++;
                            j++;                                
                        }
                        i = j;
                    }
                    else
                    {
                        intervals[interval] = data[i - 1];

                        toOneBin = allElements + (data.Count - i) / (double)(bins - interval);                   
                        interval++;                        
                    }
                }
            }
            return intervals;
        }
    }
}
