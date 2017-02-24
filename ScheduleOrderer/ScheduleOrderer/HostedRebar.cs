using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace Infinity
{
    class HostedRebar : IEquatable<HostedRebar>
    {
        private Element rebar;
        private Element floor;
        private ElementId rebarId;
        private bool hasMultipleHosts;

        public HostedRebar(Element rebar, Element floor)
        {
            this.rebar = rebar;
            this.floor = floor;
            this.rebarId = rebar.Id;
            hasMultipleHosts = false;
        }

        public void MultipleHosted()
        {
            hasMultipleHosts = true;
        }

        public bool Equals(HostedRebar other)
        {
            return rebarId == other.Id;
        }

        public ElementId Id
        {
            get
            {
                return rebarId;
            }
        }

        public bool HasMultipleHosts
        {
            get
            {
                return hasMultipleHosts;
            }
        }
        public Element Rebar
        {
            get
            {
                return rebar;
            }
        }
        public Element Floor
        {
            get
            {
                return floor;
            }
        }
    }
}
