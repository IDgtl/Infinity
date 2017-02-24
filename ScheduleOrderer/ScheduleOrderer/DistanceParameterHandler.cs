using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Creation;

namespace Infinity
{
    class DistanceParameterHandler
    {
        private Autodesk.Revit.UI.Selection.Selection selection;
        private Autodesk.Revit.DB.Document doc;

        public DistanceParameterHandler(Autodesk.Revit.UI.UIDocument uidoc)
        {
            doc = uidoc.Document;
            selection = uidoc.Selection;
        }

        public void HandleDistances()
        {
            FilteredElementCollector distances = ScheduleOrderer.getRebarsFromDocument(doc).OfClass(typeof(FamilyInstance));
            List<ElementId> unHosted = new List<ElementId>();

            foreach (Element distance in distances)
            {
                FilteredElementCollector floors = new FilteredElementCollector(doc).OfClass(typeof(Floor));
                GeometryElement gElem = distance.get_Geometry(new Options());
                Solid solid = null;

                foreach (GeometryObject obj in gElem)
                {
                    GeometryElement gElemIn = (obj as GeometryInstance).GetInstanceGeometry();

                    foreach (GeometryObject objIn in gElemIn)
                    {
                        solid = objIn as Solid;
                    }
                }

                floors.WherePasses(new ElementIntersectsSolidFilter(solid));

                if (floors.GetElementCount() > 0)
                {
                    if (distance.LookupParameter("_Арм.Основа").AsString() != floors.FirstElement().get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString())
                    {
                        using (Transaction t = new Transaction(doc, "Synchronizing host transaction"))
                        {
                            t.Start();
                            distance.LookupParameter("_Арм.Основа").Set(floors.FirstElement().get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString());
                            t.Commit();
                        }
                    }
                }
                else
                {
                    using (Transaction t = new Transaction(doc, "Synchronizing host transaction"))
                    {
                        t.Start();
                        distance.LookupParameter("_Арм.Основа").Set("");
                        t.Commit();
                    }

                    unHosted.Add(distance.Id);
                }
            }
            if (unHosted.Count != 0)
            {
                selection.SetElementIds(unHosted);
                throw new UnHostedDistancesException("Выделенные элементы не принадлежат ни одному ж/б элементу");
            }
        }

        //public void HandleDistances()
        //{
        //this.findUnhostedDistances();
        //this.findWrongNamedDistances();
        //}
        //private void findUnhostedDistances()
        //{
        //    FilteredElementCollector distances = ScheduleOrderer.getRebarsFromDocument(doc).OfClass(typeof(FamilyInstance));
        //    List<ElementId> unnamedDistanceIds = new List<ElementId>();

        //    foreach (Element distance in distances)
        //    {
        //        if (distance.LookupParameter("_Арм.Основа").AsString().Length == 0)
        //        {
        //            unnamedDistanceIds.Add(distance.Id);
        //        }
        //    }

        //    if (unnamedDistanceIds.Count != 0)
        //    {
        //        selection.SetElementIds(unnamedDistanceIds);
        //        throw new UnHostedDistancesException("В выделенных элементах необходимо задать значение параметра <_Арм.Основа>");
        //    }
        //}

        //private void findWrongNamedDistances()
        //{
        //    FilteredElementCollector floors = new FilteredElementCollector(doc).OfClass(typeof(Floor));
        //    List<ElementId> wrongNamedDistancesIds = new List<ElementId>();

        //    foreach (Element floor in floors)
        //    {
        //        BoundingBoxXYZ bbox = floor.get_Geometry(new Options()).GetBoundingBox();
        //        BoundingBoxIsInsideFilter filter = new BoundingBoxIsInsideFilter(new Outline(bbox.Min, bbox.Max));
        //        FilteredElementCollector distances = new FilteredElementCollector(doc).WherePasses(filter).OfClass(typeof(FamilyInstance));

        //        foreach (Element distance in distances)
        //        {
        //            HostedRebar rebar = new HostedRebar(distance, floor);
        //            HostedRebar sameRebar = FindTheSame(rebar);

        //            if (sameRebar != null)
        //            {
        //                sameRebar.MultipleHosted();
        //                rebar.MultipleHosted();
        //                if (!multipleHostedRebars.Contains(rebar))
        //                {
        //                    multipleHostedRebars.Add(rebar);
        //                }
        //            }
        //            else
        //            {
        //                uniqueRebars.Add(rebar);
        //            }
        //        }
        //    }

        //    foreach (HostedRebar rebar in uniqueRebars)
        //    {
        //        if (!rebar.HasMultipleHosts && rebar.Floor.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString() != rebar.Rebar.LookupParameter("_Арм.Основа").AsString())
        //        {
        //            wrongNamedDistancesIds.Add(rebar.Id);
        //        }
        //    }

        //    if (wrongNamedDistancesIds.Count != 0)
        //    {
        //        selection.SetElementIds(wrongNamedDistancesIds);
        //        throw new UnHostedDistancesException("В выделенных элементах неверное значение параметра <_Арм.Основа>, необходимо исправить!");
        //    }

        //    foreach (HostedRebar rebar in multipleHostedRebars)
        //    {
        //        wrongNamedDistancesIds.Add(rebar.Id);
        //    }

        //    if (wrongNamedDistancesIds.Count != 0)
        //    {
        //        TaskDialog dialog = new TaskDialog("Infinity");

        //        dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Продолжить выполнение программы");
        //        dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Прерваться и проверить параметр <_Арм.Основа>");
        //        dialog.MainInstruction = "В некоторых элементах возможно неверное значение параметра <_Арм.Основа>.";
        //        TaskDialogResult dialogResult = dialog.Show();
        //        if (dialogResult == TaskDialogResult.CommandLink2)
        //        {
        //            selection.SetElementIds(wrongNamedDistancesIds);
        //            throw new UnHostedDistancesException("Проблемные элементы будут выделены!");
        //        }
        //    }
        //}
        //private HostedRebar FindTheSame(HostedRebar xrebar)
        //{
        //    foreach (HostedRebar rebar in uniqueRebars)
        //    {
        //        if (rebar.Id == xrebar.Id)
        //        {
        //            return rebar;
        //        }
        //    }
        //    return null;
        //}
    }
}
