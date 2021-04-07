using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPlacementLandXml
{
    class RevitHelper
    {
        internal static void PlaceRevitFamilies(List<RevitPlacmenElement> RevitPlacmentPoints, UIDocument uiDoc, string FamilyPath)
        {
            string FamilyName = "3DAlignment_Tool";

            using (Transaction T = new Transaction(uiDoc.Document, "Place Objects"))
            {
                T.Start();
                try
                {
                    uiDoc.Document.LoadFamily(FamilyPath);
                    FamilyName = System.IO.Path.GetFileNameWithoutExtension(FamilyPath);
                }
                catch (Exception) { }
                try
                {
                    FamilySymbol Fam = (FamilySymbol)new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).FirstOrDefault(F => F.Name == FamilyName);
                    Fam.Activate();

                    foreach (RevitPlacmenElement RevitPlacementEle in RevitPlacmentPoints)
                    {
                        FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(RevitPlacmenElement.ConvertPointToInternal(RevitPlacementEle.PlacementPoint), Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        RevitPlacementEle.FillAttributes(FamIns);
                    }
                }
                catch (Exception)
                {

                }
               
                T.Commit();
            }
        }
    }
}
