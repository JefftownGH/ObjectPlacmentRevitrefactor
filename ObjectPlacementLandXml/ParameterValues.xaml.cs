using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace ObjectPlacementLandXml
{
    /// <summary>
    /// Interaction logic for ParameterValues.xaml
    /// </summary>
    public partial class ParameterValues : Window
    {
        ElementTransformParams Transform;
         public static List<ParameterElement> ParamNames;
        public ParameterValues(List<RevitPlacmenElement> revitPlaceMentPoints, string familypath, ElementTransformParams transForm)
        {
            InitializeComponent();
            RevitPlaceMentPoints = revitPlaceMentPoints;
            FamilyPath = familypath;
            Transform = transForm;

            var FamDoc = Command.uiapp.Application.OpenDocumentFile(FamilyPath).FamilyManager;
            List<string> TypeNames = new List<string>();
            foreach (FamilyType item in FamDoc.Types)
            {
                if (!String.IsNullOrWhiteSpace(item.Name))
                {
                    TypeNames.Add(item.Name);
                }
            }
            TypesCmb.ItemsSource = TypeNames;
            TypesCmb.SelectedIndex = 0;

            ParamNames = new List<ParameterElement>();
            foreach (FamilyParameter Famparam in FamDoc.Parameters)
            {
                //var Param = new (string, string)(Famparam.Definition.Name,"");
                ParamNames.Add(new ParameterElement(Famparam.Definition.Name, ""));
            }
            ParamValsDG.ItemsSource = ParamNames;
        }

        public List<RevitPlacmenElement> RevitPlaceMentPoints { get; }
        public string FamilyPath { get; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var FamilyList = RevitHelper.PlaceRevitFamilies(RevitPlaceMentPoints, Command.uidoc, FamilyPath, (string)TypesCmb.SelectedItem, Transform);
            
          

        }
    }
}
