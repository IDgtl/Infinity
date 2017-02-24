using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;

namespace Infinity
{
    public partial class Window : System.Windows.Forms.Form
    {
        private Document doc;
        private UIDocument uidoc;
        private string constructionName = null;
        System.Windows.Forms.Form progressWindow;
        public Window(UIDocument uidoc)
        {
            InitializeComponent();
            this.uidoc = uidoc;
            doc = this.uidoc.Document;
        }

        public Document Document
        {
            get
            {
                return doc;
            }
        }
        public string ConstructionName
        {
            get
            {
                return constructionName;
            }
        }
        public UIDocument UIDocument
        {
            get
            {
                return uidoc;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ScheduleOrderer.SyncParameters(uidoc);
            }
            catch (UnHostedDistancesException ex)
            {
                TaskDialog.Show("Revit", ex.Message);
                this.Close();
                this.Dispose();
            }

            try
            {
                comboBox1.Items.AddRange(HostConstructionGetter.getConstructionsNames(ScheduleOrderer.getRebarsFromDocument(doc)));
            }
            catch (EmptyCollectorException ex)
            {
                TaskDialog.Show("Revit", ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                TaskDialog.Show("Revit", ex.Message);
            }
            this.button1.Visible = false;
            this.comboBox1.Visible = true;
            this.button2.Visible = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            constructionName = comboBox1.SelectedItem.ToString();
            if (constructionName != null)
            {
                this.button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            progressWindow = new ProgressWindow();
            progressWindow.Show(this);
            this.Enabled = false;

            FilteredElementCollector newRebarsCollector = ScheduleOrderer.getRebarsFromDocument(doc);
            Element rebar = newRebarsCollector.FirstElement();
            Parameter param = rebar.LookupParameter("_Арм.Основа");
            if (null == param)
            {
                throw new ArgumentNullException("Параметр < _Арм.Основа > отсутствует");
            }
            ElementId paramId = param.Id;
            if (null == constructionName)
            {
                throw new ArgumentNullException("Выберите конструкцию");
            }
            FilterRule rule = ParameterFilterRuleFactory.CreateEqualsRule(paramId, constructionName, true);
            ElementFilter filter = new ElementParameterFilter(rule);
            newRebarsCollector.WherePasses(filter);

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Event");
                ScheduleOrderer.CreateOrderedSchedule(newRebarsCollector);
                //TaskDialog.Show("Revit", "Транзакция");
                t.Commit();
            }

            progressWindow.Hide();
            this.Enabled = true;
            progressWindow.Dispose();
            progressWindow = null;
        }
    }
}
