using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Infinity
{
    class ParametersHolder
    {
        private static string[] rebarParameters = null;
        private static string[] commonRebarParameters = new string[] { "_Арм.Стандарт",
                                                                 "_Арм.Материал",
                                                                 "_Арм.Погонные_Метры",
                                                                 "_Арм.Диаметр",
                                                                 "_Арм.Форма_Стержня",
                                                                 "_Арм.Длина",
                                                                 "_Арм.А",
                                                                 "_Арм.Б",
                                                                 "_Арм.В",
                                                                 "_Арм.Г",
                                                                 "_Арм.Д",
                                                                 "_Арм.Е",
                                                                 "_Арм.Ж"};

        private static string[] linearMetersRebarParameters = new string[] { "_Арм.Стандарт",
                                                                 "_Арм.Материал",
                                                                 "_Арм.Погонные_Метры",
                                                                 "_Арм.Диаметр",
                                                                 "_Арм.Форма_Стержня"};
        public static bool CompareRebars(Element first, Element second)
        {
            if (first.LookupParameter("_Арм.Погонные_Метры").AsValueString() == second.LookupParameter("_Арм.Погонные_Метры").AsValueString())
            {
                if (first.LookupParameter("_Арм.Погонные_Метры").AsValueString() == "Да" || first.LookupParameter("_Арм.Погонные_Метры").AsValueString() == "Yes")
                {
                    rebarParameters = linearMetersRebarParameters;
                }
                else
                {
                    rebarParameters = commonRebarParameters;
                }
            }
            else
            {
                return false;
            }

            foreach (string rebarParameter in rebarParameters)
            {
                if (first.LookupParameter(rebarParameter) == null)
                {
                    Element firstType = first.Document.GetElement(first.GetTypeId());
                    Element secondType = second.Document.GetElement(second.GetTypeId());
                    if (CompareRebars(firstType, secondType, rebarParameter) == false)
                    {
                        return false;
                    }
                }
                else if (first.LookupParameter(rebarParameter).StorageType == StorageType.None)
                {
                    throw new ArgumentException("Не поддерживаемый тип данных параметра");
                }
                else if (first.LookupParameter(rebarParameter).StorageType == StorageType.String)
                {
                    if (first.LookupParameter(rebarParameter).AsString() != second.LookupParameter(rebarParameter).AsString())
                    {
                        return false;
                    }
                }
                else
                {
                    if (first.LookupParameter(rebarParameter).AsValueString() != second.LookupParameter(rebarParameter).AsValueString())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CompareRebars(Element first, Element second, string rebarParameter)
        {
            if (first.LookupParameter(rebarParameter) == null)
            {
                throw new ArgumentNullException("Указанный параметр не существует");
            }
            else if (first.LookupParameter(rebarParameter).StorageType == StorageType.None)
            {
                throw new ArgumentException("Не поддерживаемый тип данных параметра");
            }
            else if (first.LookupParameter(rebarParameter).StorageType == StorageType.String)
            {
                if (first.LookupParameter(rebarParameter).AsString() != second.LookupParameter(rebarParameter).AsString())
                {
                    return false;
                }
            }
            else
            {
                if (first.LookupParameter(rebarParameter).AsValueString() != second.LookupParameter(rebarParameter).AsValueString())
                {
                    return false;
                }
            }
            return true;
        }

        public static int GetRebarComparisonResult(Element first, Element second)
        {
            rebarParameters = null;
            rebarParameters = commonRebarParameters;

            foreach (string rebarParameter in rebarParameters)
            {
                if (first.LookupParameter(rebarParameter) == null)
                {
                    Element firstType = first.Document.GetElement(first.GetTypeId());
                    Element secondType = second.Document.GetElement(second.GetTypeId());
                    if (GetRebarComparisonResult(firstType, secondType, rebarParameter) != 0)
                    {
                        return GetRebarComparisonResult(firstType, secondType, rebarParameter);
                    }
                }
                else if (first.LookupParameter(rebarParameter).StorageType == StorageType.None)
                {
                    throw new ArgumentException("Не поддерживаемый тип данных параметра");
                }
                else if (first.LookupParameter(rebarParameter).StorageType == StorageType.String &&
                    first.LookupParameter(rebarParameter).AsString().CompareTo(second.LookupParameter(rebarParameter).AsString()) != 0)
                {
                    return first.LookupParameter(rebarParameter).AsString().CompareTo(second.LookupParameter(rebarParameter).AsString());
                }
                else if (first.LookupParameter(rebarParameter).StorageType != StorageType.String &&
                         first.LookupParameter(rebarParameter).AsValueString().CompareTo(second.LookupParameter(rebarParameter).AsValueString()) != 0)
                {
                    return first.LookupParameter(rebarParameter).AsValueString().CompareTo(second.LookupParameter(rebarParameter).AsValueString());
                }
            }
            return 0;
        }
        private static int GetRebarComparisonResult(Element first, Element second, string rebarParameter)
        {
            if (first.LookupParameter(rebarParameter) == null)
            {
                throw new ArgumentNullException("Указанный параметр не существует");
            }
            else if (first.LookupParameter(rebarParameter).StorageType == StorageType.None)
            {
                throw new ArgumentException("Не поддерживаемый тип данных параметра");
            }
            else if (first.LookupParameter(rebarParameter).StorageType == StorageType.String &&
                first.LookupParameter(rebarParameter).AsString().CompareTo(second.LookupParameter(rebarParameter).AsString()) != 0)
            {
                return first.LookupParameter(rebarParameter).AsString().CompareTo(second.LookupParameter(rebarParameter).AsString());
            }
            else if (first.LookupParameter(rebarParameter).StorageType != StorageType.String &&
                     first.LookupParameter(rebarParameter).AsValueString().CompareTo(second.LookupParameter(rebarParameter).AsValueString()) != 0)
            {
                return first.LookupParameter(rebarParameter).AsValueString().CompareTo(second.LookupParameter(rebarParameter).AsValueString());
            }
            return 0;
        }
    }
}
