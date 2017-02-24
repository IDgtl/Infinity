using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace Infinity
{
    class MarkableRebar
    {
        private Element rebar;
        private int mark;
        private bool isMarked;
        private ElementId rebarId;

        public MarkableRebar(Element rebar, int mark)
        {
            this.rebar = rebar;
            this.rebarId = rebar.Id;
            this.mark = mark;
            isMarked = false;
        }

        public Element Rebar
        {
            get
            {
                return rebar;
            }
        }
        public ElementId Id
        {
            get
            {
                return rebarId;
            }
        }
        public bool IsMarked
        {
            get
            {
                return isMarked;
            }
        }
        public void markRebar()
        {
            rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_SCHEDULE_MARK).Set(mark.ToString());
            isMarked = true;
        }
    }
}
