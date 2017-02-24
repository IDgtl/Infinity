using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.Creation;

namespace Infinity
{
    public static class ScheduleOrderer
    {
        public static void CreateOrderedSchedule(FilteredElementCollector rebars)
        {
            List<List<Element>> rebarsGroups = new List<List<Element>>();
            foreach (Element rebar in rebars)
            {
                try
                {
                    findHostGroup(rebar, rebarsGroups);
                } catch (ArgumentException ex)
                {
                    TaskDialog.Show("Revit", ex.Message);
                }
            }
            rebarsGroups.Sort(rebarComparer);
            assignNumbersToRebars(rebarsGroups);

            return;
        }
        private static void findHostGroup(Element rebar, List<List<Element>> rebarsGroups)
        {
            if (rebarsGroups.Count == 0)
            {
                List<Element> firstGroup = new List<Element>();
                firstGroup.Add(rebar);
                rebarsGroups.Add(firstGroup);
                return;
            }

            Element comparisonRebar = null;

            foreach (List<Element> rebarsGroup in rebarsGroups)
            {
                comparisonRebar = rebarsGroup[0];

                if (ParametersHolder.CompareRebars(rebar, comparisonRebar))
                {
                    rebarsGroup.Add(rebar);
                    return;
                }
            }

            List<Element> newGroup = new List<Element>();
            newGroup.Add(rebar);
            rebarsGroups.Add(newGroup);
            return;
        }
        private static int rebarComparer(List<Element> first, List<Element> second)
        {
            return ParametersHolder.GetRebarComparisonResult(first[0], second[0]);
        }
        private static void assignNumbersToRebars(List<List<Element>> rebarsGroups)
        {
            List<MarkableRebar> mRebars = new List<MarkableRebar>();
            Autodesk.Revit.DB.Document doc = rebarsGroups[0][0].Document;
            Autodesk.Revit.Creation.Document crDoc = doc.Create;
            int mark = 0;

            foreach (List<Element> rebarsGroup in rebarsGroups)
            {
                mark += 1;
                foreach (Element rebar in rebarsGroup)
                {
                    mRebars.Add(new MarkableRebar(rebar, mark));
                }
            }

            foreach (MarkableRebar mRebar in mRebars)
            {
                if (mRebar.Rebar.GroupId.IntegerValue == -1)
                {
                    mRebar.markRebar();
                }
                else
                {
                    if (!mRebar.IsMarked)
                    {
                        List<ElementId> rebarsIds;
                        Group rebarOwner;
                        Element rebar;

                        rebarOwner = doc.GetElement(mRebar.Rebar.GroupId) as Group;
                        rebarsIds = rebarOwner.UngroupMembers().ToList();
                        foreach (ElementId rebarId in rebarsIds)
                        {
                            rebar = doc.GetElement(rebarId);
                            if (rebar.GetType() == typeof(Rebar) || rebar.GetType() == typeof(FamilyInstance))
                            {
                                foreach (MarkableRebar neededRebar in mRebars)
                                {
                                    if (neededRebar.Id == rebarId)
                                    {
                                        neededRebar.markRebar();
                                    }
                                }
                            }
                        }
                        rebarOwner = crDoc.NewGroup(rebarsIds);
                    }
                }
            }
        }
        public static FilteredElementCollector getRebarsFromDocument(Autodesk.Revit.DB.Document doc)
        {
            FilteredElementCollector rebars = new FilteredElementCollector(doc).OfClass(typeof(Rebar));
            FilteredElementCollector families = new FilteredElementCollector(doc).OfClass(typeof(Family));
            Element family = null;

            foreach (Element elem in families)
            {
                if (elem.Name == "Дистанция")
                {
                    family = elem;
                    break;
                }
            }

            if (family != null)
            {
                //Should try to overwrite this part of code without using Family class, but directly use FamSymbol class.
                ElementId famId = family.Id;
                ElementId famSymId;
                Autodesk.Revit.DB.ElementFilter famInstFilter;
                Autodesk.Revit.DB.ElementFilter famSymFilter = new FamilySymbolFilter(famId) as Autodesk.Revit.DB.ElementFilter;
                FilteredElementCollector familySymbols = new FilteredElementCollector(doc).WherePasses(famSymFilter);

                foreach (Element elem in familySymbols)
                {
                    famSymId = elem.Id;
                    famInstFilter = new FamilyInstanceFilter(doc, famSymId);
                    rebars.UnionWith(new FilteredElementCollector(doc).WherePasses(famInstFilter));
                }
            }
            if (rebars.GetElementCount() == 0)
            {
                throw new EmptyCollectorException("Отсутствует арматура в данном проекте");
            }

            return rebars;
        }
        public static void SyncParameters(Autodesk.Revit.UI.UIDocument uidoc)
        {
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            FilteredElementCollector rebars = getRebarsFromDocument(doc).OfClass(typeof(Rebar));
            FilteredElementCollector rebarTypes = new FilteredElementCollector(doc).OfClass(typeof(RebarBarType));
            DistanceParameterHandler dHandler = new DistanceParameterHandler(uidoc);

            dHandler.HandleDistances();

            foreach (Element rebarType in rebarTypes)
            {
                if (rebarType.LookupParameter("_Арм.Материал").AsElementId() != rebarType.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM).AsElementId())
                {
                    using (Transaction t = new Transaction(doc, "Synchronizing material transaction"))
                    {
                        t.Start();
                        rebarType.LookupParameter("_Арм.Материал").Set(rebarType.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM).AsElementId());
                        t.Commit();
                    }
                }
            }

            foreach (Element rebar in rebars)
            {
                if (rebar.LookupParameter("_Арм.Основа").AsString() != rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOST_MARK).AsString())
                {
                    if (rebar.GroupId.IntegerValue == -1)
                    {
                        using (Transaction t = new Transaction(doc, "Synchronizing host transaction"))
                        {
                            t.Start();
                            rebar.LookupParameter("_Арм.Основа").Set(rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOST_MARK).AsString());
                            t.Commit();
                        }
                    }
                    else
                    {
                        using (Transaction t = new Transaction(doc, "Synchronizing host in group transaction"))
                        {
                            t.Start();
                            List<ElementId> rebarsIds;
                            Group rebarOwner;
                            Element rebarInGroup;

                            rebarOwner = doc.GetElement(rebar.GroupId) as Group;
                            rebarsIds = rebarOwner.UngroupMembers().ToList();

                            foreach (ElementId rebarId in rebarsIds)
                            {
                                rebarInGroup = doc.GetElement(rebarId);
                                if (rebarInGroup.GetType() == typeof(Rebar))
                                {
                                    if (rebarInGroup.LookupParameter("_Арм.Основа").AsString() != rebarInGroup.get_Parameter(BuiltInParameter.REBAR_ELEM_HOST_MARK).AsString())
                                    {
                                        rebarInGroup.LookupParameter("_Арм.Основа").Set(rebarInGroup.get_Parameter(BuiltInParameter.REBAR_ELEM_HOST_MARK).AsString());
                                    }
                                }
                            }
                            rebarOwner = doc.Create.NewGroup(rebarsIds);
                            t.Commit();
                        }
                    }
                }
            }
        }
    }
}
