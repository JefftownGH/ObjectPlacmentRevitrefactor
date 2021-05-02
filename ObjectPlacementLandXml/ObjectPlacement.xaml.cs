using Autodesk.Revit.UI;
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
    /// Interaction logic for ObjectPlacement.xaml
    /// </summary>
    public partial class ObjectPlacement : Window
    {
        public static double Stationincrement { get; set; }
        public ObjectPlacement()
        {
            InitializeComponent();
        }

        private void Run_click(object sender, RoutedEventArgs e)
        {
            var RevitPlaceMentPoints = LandXmlParser.ParseLandXml(LandXmlPath.Text, ExtractStationDisntace(), (double)ExtractStationPlacmentStart(), (double)ExtractStationPlacmentEnd());

            ElementTransformParams TransForm = ExtractTransformParameters();
           
            ParameterValues W = new ParameterValues(RevitPlaceMentPoints, FamilyPath.Text, TransForm);
            W.ShowDialog();
            //RevitHelper.PlaceRevitFamilies(RevitPlaceMentPoints, uiDoc, FamilyPath.Text);
        }

        private ElementTransformParams ExtractTransformParameters()
        {
            ElementTransformParams TransForm = new ElementTransformParams();
            if (!string.IsNullOrEmpty(this.HorizontalDistancetext.Text))
            {
                TransForm.HorizontalDistance = double.Parse(this.HorizontalDistancetext.Text);
            }
            if (!string.IsNullOrEmpty(this.ElevationTxt.Text))
            {
                TransForm.ElevationFromAlignment = double.Parse(this.ElevationTxt.Text);
            }
            if (!string.IsNullOrEmpty(this.DegreesTxt.Text))
            {
                TransForm.RotationAngleInPlane = double.Parse(this.DegreesTxt.Text);
            }
            if (!string.IsNullOrEmpty(this.InclinationTxt.Text))
            {
                TransForm.InclinationAngleInXZPlane = double.Parse(this.InclinationTxt.Text);
            }
            TransForm.RotateWithAlignment = RotateWithAlignment.IsChecked;
            return TransForm;
        }

        public double ExtractStationDisntace()
        {
            if (string.IsNullOrEmpty(this.StationDistanceTxt.Text))
            {
                this.StationDistanceTxt.Text = 0.ToString();
            }
            Stationincrement = double.Parse(this.StationDistanceTxt.Text);

            return Stationincrement;
        }
        public double? ExtractStationPlacmentStart()
        {
            double StationPlaceMentStart = default(double);
            if (!string.IsNullOrEmpty(this.PlacmentStartStationText.Text))
            {
                StationPlaceMentStart = double.Parse(this.PlacmentStartStationText.Text);
            }

            return StationPlaceMentStart;
        }
        public double? ExtractStationPlacmentEnd()
        {
            double StationPlaceMentEnd = default(double);
            if (!string.IsNullOrEmpty(this.PlacmentEndStationText.Text))
            {
                StationPlaceMentEnd = double.Parse(this.PlacmentEndStationText.Text);
            }
            return StationPlaceMentEnd;
        }
        private void LandXmlPathBut(object sender, RoutedEventArgs e)
        {
            WindowDialogs.LandXmlOpenDialog(LandXmlPath);
        }

        private void RevitBrowserClick(object sender, RoutedEventArgs e)
        {
            WindowDialogs.OpenDialogRev(FamilyPath);
        }
    }
}
