using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPlacementLandXml
{
    class RevitHelper
    {
        internal static List<FamilyInstance> PlaceRevitFamilies(List<RevitPlacmenElement> RevitPlacmentPoints, UIDocument uiDoc, string FamilyPath, string TypeName, ElementTransformParams transform)
        {
            List<FamilyInstance> CreatedInstances = new List<FamilyInstance>();
            string FamilyName = string.Empty;
            RevitPlacmentPoints = RevitPlacmentPoints.Distinct(new ComparePlacmentPoints()).ToList();
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
                    FamilySymbol Fam = new FilteredElementCollector(uiDoc.Document).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().FirstOrDefault(F => F.Name == TypeName && F.FamilyName == FamilyName);
                    Fam.Activate();

                    for (int i = 0; i < RevitPlacmentPoints.Count; i++)
                    {
                        FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(RevitPlacmenElement.ConvertPointToInternal(RevitPlacmentPoints[i].PlacementPoint), Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        RevitPlacmentPoints[i].FillAttributes(FamIns);

                        CreatedInstances.Add(FamIns);


                        if (i == RevitPlacmentPoints.Count - 1)
                        {
                            TransformFamilyInstances(FamIns, transform, uiDoc.Document, RevitPlacmentPoints[i], RevitPlacmentPoints[i - 1]);

                        }
                        else
                        {
                            TransformFamilyInstances(FamIns, transform, uiDoc.Document, RevitPlacmentPoints[i], RevitPlacmentPoints[i + 1]);
                        }

                    }

                }
                catch (Exception)
                {

                }

                T.Commit();
            }
            return CreatedInstances;
        }

        private static void TransformFamilyInstances(FamilyInstance famIns, ElementTransformParams transform, Document document, RevitPlacmenElement CurrentPoint, RevitPlacmenElement NextPoint)
        {
            if (transform.HorizontalDistance != default(double))
            {
                // XYZ NormalVector = (CurrentPoint.PlacementPoint - NextPoint.PlacementPoint).Normalize();
                // double AngleOnX = NormalVector.AngleTo(XYZ.BasisX);
                // double AngleOnZ = NormalVector.AngleTo(XYZ.BasisZ);

                // var HorizontalDistanceTransform = UnitUtils.ConvertToInternalUnits(transform.HorizontalDistance, DisplayUnitType.DUT_MILLIMETERS);
                // var ElevationDistanceTransform = UnitUtils.ConvertToInternalUnits(transform.ElevationFromAlignment, DisplayUnitType.DUT_MILLIMETERS);
                // XYZ MovePoint = new XYZ(0, HorizontalDistanceTransform, 0);
                // ElementTransformUtils.MoveElement(document, famIns.Id, MovePoint);

                //Autodesk.Revit.DB.Line RotationAxis = Autodesk.Revit.DB.Line.CreateBound(CurrentPoint.PlacementPoint, new XYZ(CurrentPoint.PlacementPoint.X, CurrentPoint.PlacementPoint.Y, CurrentPoint.PlacementPoint.Z + 100));
                //ElementTransformUtils.RotateElement(document, famIns.Id, RotationAxis, AngleOnX);
            }
            if (transform.RotationAngleInPlane != default(double))
            {
                XYZ NormalVector = (NextPoint.PlacementPoint - CurrentPoint.PlacementPoint).Normalize();
                double Angle = (Math.PI / 2) - NormalVector.AngleTo(XYZ.BasisX);


                Double RotationAngle = UnitUtils.ConvertToInternalUnits(transform.RotationAngleInPlane, DisplayUnitType.DUT_DEGREES_AND_MINUTES);
                Angle = Angle + RotationAngle;
                var Location = (famIns.Location as LocationPoint);
                var Line = Autodesk.Revit.DB.Line.CreateBound(Location.Point, Location.Point.Add(XYZ.BasisZ));
               // ElementTransformUtils.RotateElement(document, famIns.Id, Line, Angle);
                Location.Rotate(Line, RotationAngle);

            }
            //move Horizontal

        }

        public class ComparePlacmentPoints : IEqualityComparer<RevitPlacmenElement>
        {
            public new bool Equals(RevitPlacmenElement x, RevitPlacmenElement y)
            {
                if (x.PlacementPoint.X == y.PlacementPoint.X && x.PlacementPoint.Y == y.PlacementPoint.Y && x.PlacementPoint.Z == y.PlacementPoint.Z)
                {
                    return true;
                }
                return false;
            }

            public int GetHashCode(RevitPlacmenElement obj)
            {
                double Hcode = obj.PlacementPoint.X * obj.PlacementPoint.Y * obj.PlacementPoint.Z;
                return Hcode.GetHashCode();
            }
        }
    }
}
