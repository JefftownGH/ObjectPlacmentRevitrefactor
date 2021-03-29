#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace ObjectPlacementLandXml
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public static UIApplication uiapp;
        public static UIDocument uidoc;
        public Result Execute( ExternalCommandData commandData,ref string message, ElementSet elements)
        {

            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            ObjectPlacement T = new ObjectPlacement();
            T.ShowDialog();

            return Result.Succeeded;
        }
    }
}
