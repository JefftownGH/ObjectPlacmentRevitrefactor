﻿using Autodesk.Revit.UI;
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
            var RevitPlaceMentPoints = LandXmlParser.ParseLandXml(LandXmlPath.Text, ExtractStationDisntace());

            ParameterValues W = new ParameterValues(RevitPlaceMentPoints, FamilyPath.Text);
            W.ShowDialog();
            //RevitHelper.PlaceRevitFamilies(RevitPlaceMentPoints, uiDoc, FamilyPath.Text);
            this.Close();



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
