using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace Infinity
{
    public static class HostConstructionGetter
    {
        public static string[] getConstructionsNames(FilteredElementCollector rebars)
        {
            string constructionName = null;
            List<string> constructionsNames = new List<string>();

            foreach (Element rebar in rebars)
            {
                constructionName = rebar.LookupParameter("_Арм.Основа").AsString();
                if (constructionName == null)
                {
                    throw new ArgumentNullException("Параметр <_Арм.Основа> отсутствует в экземпляре арматуры");
                }
                if (!constructionsNames.Contains(constructionName))
                {
                    constructionsNames.Add(constructionName);
                }
                constructionName = null;
            }
            constructionsNames.Sort(nameComparer);

            return constructionsNames.ToArray();
        }
        private static int nameComparer(string first, string second)
        {
            int counter = Math.Min(first.Length, second.Length);

            for (int i = 0; i < counter; i++)
            {
                if (char.IsDigit(first[i]) && char.IsDigit(second[i]))
                {
                    return compareByNumericValue(first, second, i);
                } else if (first[i] != second[i])
                {
                    return first[i].CompareTo(second[i]);
                }
            }
            if (first.Length > second.Length)
            {
                return 1;
            } else if (first.Length < second.Length)
            {
                return -1;
            }
            return 0;
        }
        private static int compareByNumericValue(string first, string second, int iterator)
        {
            string firstSubstr = first.Substring(iterator);
            string secondSubstr = second.Substring(iterator);
            int firstCounter = firstSubstr.Length;
            int secondCounter = secondSubstr.Length;
            char[] firstArray = new char[firstCounter];
            char[] secondArray = new char[secondCounter];
            int firstNumber;
            int secondNumber;
            int i = 0;
            int j = 0;

            for ( ; i < firstCounter && char.IsDigit(firstSubstr[i]); i++)
            {
                firstArray[i] = firstSubstr[i];
            }
            for ( ; j < secondCounter && char.IsDigit(secondSubstr[j]); j++)
            {
                secondArray[j] = secondSubstr[j];
            }

            firstNumber = Int32.Parse(new string(firstArray));
            secondNumber = Int32.Parse(new string(secondArray));
            if (firstNumber > secondNumber)
            {
                return 1;
            } else if (firstNumber < secondNumber)
            {
                return -1;
            } else
            {
                return nameComparer(firstSubstr.Substring(i), secondSubstr.Substring(j));
            }
        }
    }
}
