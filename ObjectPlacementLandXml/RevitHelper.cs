﻿using Autodesk.Revit.DB;
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
            RevitPlacmentPoints.Sort(delegate (RevitPlacmenElement c1, RevitPlacmenElement c2) { return c1.Station.CompareTo(c2.Station); });
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

                    if (transform.StationToStartFrom != default(double))
                    {
                        RevitPlacmentPoints.RemoveAll(E => E.Station < transform.StationToStartFrom);
                    }
                    if (transform.StationToEndAt != default(double))
                    {
                        RevitPlacmentPoints.RemoveAll(E => E.Station > transform.StationToEndAt);
                    }
                    
                    for (int i = 0; i < RevitPlacmentPoints.Count; i++)
                    {
                        FamilyInstance FamIns = uiDoc.Document.Create.NewFamilyInstance(RevitPlacmenElement.ConvertPointToInternal(RevitPlacmentPoints[i].PlacementPoint), Fam, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        RevitPlacmentPoints[i].FillAttributes(FamIns);

                        CreatedInstances.Add(FamIns);
                    }

                }
                catch (Exception)
                {

                }

                T.Commit();
            }



            using (Transaction T = new Transaction(uiDoc.Document, "rotate Element"))
            {
                T.Start();
                for (int i = 0; i < CreatedInstances.Count; i++)
                {
                    var ThisLocation = (CreatedInstances[i].Location as LocationPoint).Point;

                    XYZ NextLocation = GetNextPoint(CreatedInstances, i);

                    Autodesk.Revit.DB.Line NeuRotationLineZ = Autodesk.Revit.DB.Line.CreateUnbound(ThisLocation, XYZ.BasisZ);

                    if (transform.RotateWithAlignment == true)
                    {
                        RotateToAlignment(uiDoc, CreatedInstances, i, ThisLocation, NeuRotationLineZ, NextLocation);
                    }

                    if (transform.RotationAngleInPlane > 0)
                    {
                        Double Angle = UnitUtils.ConvertToInternalUnits(transform.RotationAngleInPlane, DisplayUnitType.DUT_DEGREES_AND_MINUTES);
                        ElementTransformUtils.RotateElement(uiDoc.Document, CreatedInstances[i].Id, NeuRotationLineZ, Angle);
                    }

                }

                T.Commit();
            }
            return CreatedInstances;
        }

        private static XYZ GetNextPoint(List<FamilyInstance> CreatedInstances, int i)
        {
            XYZ NextLocation = null;

            if (i < CreatedInstances.Count - 1)
            {
                NextLocation = (CreatedInstances[i + 1].Location as LocationPoint).Point;
            }
            else
            {
                NextLocation = (CreatedInstances[i - 1].Location as LocationPoint).Point;
            }

            return NextLocation;
        }

        private static void RotateToAlignment(UIDocument uiDoc, List<FamilyInstance> CreatedInstances, int i, XYZ ThisLocation, Autodesk.Revit.DB.Line NeuRotationLineZout, XYZ NextLocation)
        {
            double ModifiedAngle = ModifyRotationAngle(ThisLocation, NextLocation);
            // double ModifiedAngle = ModifyRotationAngle(ThisLocation, NextLocation);
            ElementTransformUtils.RotateElement(uiDoc.Document, CreatedInstances[i].Id, NeuRotationLineZout, ModifiedAngle);
        }

        private static double ModifyRotationAngle(XYZ CurrentPoint, XYZ NextPoint)
        {
            var EndPoint = RevitPlacmenElement.ConvertPointToInternal(NextPoint);
            var StartPoint = RevitPlacmenElement.ConvertPointToInternal(CurrentPoint);
            XYZ NormalVector = (EndPoint - CurrentPoint).Normalize();
            double Angle = (Math.PI / 2) - NormalVector.AngleTo(XYZ.BasisX);
            return Angle;
        }
        private static void TransformFamilyInstances(FamilyInstance famIns, ElementTransformParams transform, Document document, RevitPlacmenElement CurrentPoint, RevitPlacmenElement NextPoint)
        {
            //double Angle = ModifyRotationAngle(transform, CurrentPoint, NextPoint);
            /// var EndPoint = new XYZ(NextPoint.PlacementPoint.X, NextPoint.PlacementPoint.Y, (NextPoint.PlacementPoint.Z + 100));
            // var Line = Autodesk.Revit.DB.Line.CreateBound(NextPoint.PlacementPoint, EndPoint);
            // ElementTransformUtils.RotateElement(document, famIns.Id, Line, Angle);

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
                Double RotationAngle = UnitUtils.ConvertToInternalUnits(transform.RotationAngleInPlane, DisplayUnitType.DUT_DEGREES_AND_MINUTES);
                var LocatioNPoint = (famIns.Location as LocationPoint).Point;
                var Line2 = Autodesk.Revit.DB.Line.CreateBound(LocatioNPoint, LocatioNPoint.Add(new XYZ(LocatioNPoint.X, LocatioNPoint.Y, LocatioNPoint.Z + 100)));
                ElementTransformUtils.RotateElement(document, famIns.Id, Line2, RotationAngle);
                //// XYZ NormalVector = (CurrentPoint.PlacementPoint - NextPoint.PlacementPoint).Normalize();
                //// double Angle = NormalVector.AngleTo(XYZ.BasisX);
                //Double RotationAngle = UnitUtils.ConvertToInternalUnits(transform.RotationAngleInPlane, DisplayUnitType.DUT_DEGREES_AND_MINUTES);
                ////Angle = Angle + RotationAngle;
                ////var Location = (famIns.Location as LocationPoint);
                //var Line2 = Autodesk.Revit.DB.Line.CreateBound(NextPoint.PlacementPoint, NextPoint.PlacementPoint.Add(XYZ.BasisZ));
                //ElementTransformUtils.RotateElement(document, famIns.Id, Line2, RotationAngle);
                ////Location.Rotate(Line, RotationAngle);

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
