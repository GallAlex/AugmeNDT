// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Class helps to print a table in the console
    /// </summary>
    public class TablePrint
    {

        /// <summary>
        /// Method returns the formatted string of the 2D string array
        /// Use Monospaced Font in Unity Console for correct spacing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnHeaders"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ToStringTable<T>(string[] columnHeaders, List<T[]> values)
        {
            if (columnHeaders.Length != values.Count)
            {
                Debug.LogError("Not all column headers have values! \n" + "columnHeaders count " + columnHeaders.Length + " , values count " + values.Count);
                return "Error";
            }

            // Write a 2D string array from the columnHeaders and values
            string[,] arrValues = new string[values[0].Length + 1, columnHeaders.Length];
            
            for (int i = 0; i < columnHeaders.Length; i++)
            {
                arrValues[0, i] = columnHeaders[i]; // Header Row

                for (int j = 0; j < values[i].Length; j++)
                {
                    arrValues[j+1, i] = values[i][j].ToString();

                }
            }

            return ToStringTable(arrValues);
        }

        public static string ToStringTable<T>(string[] columnHeaders, T[] values)
        {
            if (columnHeaders.Length != values.Length)
            {
                Debug.LogError("Not all column headers have values! \n" + "columnHeaders count " + columnHeaders.Length + " , values count " + values.Length);
                return "Error";
            }

            // Write a 2D string array from the columnHeaders and values
            string[,] arrValues = new string[values.Length + 1, columnHeaders.Length];

            for (int i = 0; i < columnHeaders.Length; i++)
            {
                arrValues[0, i] = columnHeaders[i]; // Header Row

                for (int j = 0; j < values.Length; j++)
                {
                    arrValues[j + 1, i] = values[j].ToString();

                }
            }

            return ToStringTable(arrValues);
        }

        /// <summary>
        /// Method returns the formatted string of the 2D string array
        /// Debug.Log limited to 64k characters
        /// </summary>
        /// <param name="arrValues"></param>
        /// <returns></returns>
        public static string ToStringTable(string[,] arrValues)
        {
            int[] maxColumnsWidth = GetMaxColumnsWidth(arrValues);
            var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

            var sb = new StringBuilder();
            for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    // Print cell
                    string cell = arrValues[rowIndex, colIndex];
                    cell = cell.PadRight(maxColumnsWidth[colIndex]);
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                // Print end of line
                sb.Append(" | ");
                sb.AppendLine();

                // Print splitter
                if (rowIndex == 0)
                {
                    sb.AppendFormat(" |{0}| ", headerSpliter);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static int[] GetMaxColumnsWidth(string[,] arrValues)
        {
            var maxColumnsWidth = new int[arrValues.GetLength(1)];
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
                {
                    int newLength = arrValues[rowIndex, colIndex].Length;
                    int oldLength = maxColumnsWidth[colIndex];

                    if (newLength > oldLength)
                    {
                        maxColumnsWidth[colIndex] = newLength;
                    }
                }
            }

            return maxColumnsWidth;
        }

        public static string ToStringRow<T>(T[] arrValues)
        {
            int maxColumnsWidth = arrValues.Length;
            var sb = new StringBuilder();

            for (int i = 0; i < arrValues.Length; i++)
            {
                string row = arrValues[i].ToString();
                row = row.PadRight(maxColumnsWidth);

                sb.Append(" | ");
                sb.Append(arrValues[i].ToString());
            }

            return sb.ToString();
        }

    }

}
