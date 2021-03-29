﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ObjectPlacementLandXml
{
    class LandXmlStationingObject
    {
        public double Station { get; set; }
        public double EndStation { get; set; }
        public object AlignmentElement { get; set; }
        public object RevitElement { get; set; }

        public LandXmlStationingObject(double station, object alignmentElement)
        {
            Station = station;
            AlignmentElement = alignmentElement;
            EndStation = station + this.GetLength();

            CreateRevitElement();
        }

        private void CreateRevitElement()
        {
            if (this.AlignmentElement is Line)
            {
                Autodesk.Revit.DB.Line L = Autodesk.Revit.DB.Line.CreateBound(this.GetStartPoint(), this.GetEndPoint());
                RevitElement = L;
            }
            if (this.AlignmentElement is IrregularLine)
            {
                Autodesk.Revit.DB.Line L = Autodesk.Revit.DB.Line.CreateBound(this.GetStartPoint(), this.GetEndPoint());
                RevitElement = L;

            }
            if (this.AlignmentElement is Curve)
            {
                var StartPoint = this.GetStartPoint();
                var EndPoint = this.GetEndPoint();
                var Radius = this.GetCurveRadius();

                Arc C = CreateArc(StartPoint, EndPoint, Radius, (bool)false);
                RevitElement = C;

            }
            if (this.AlignmentElement is Spiral)
            {
                //PolyLine P = PolyLine.Create()
                Arc HS = Arc.Create(this.GetStartPoint(), this.GetEndPoint(), this.GetPointPI());
                RevitElement = HS;


            }
            if (this.AlignmentElement is Chain)
            {
                //Review 
                //return ExtractPoint((this.AlignmentElement as Chain).Text);
            }

        }

        public XYZ GetStartPoint()
        {
            XYZ StartPoint = null;
            if (this.AlignmentElement is Line)
            {
                StartPoint = ExtractPoint((this.AlignmentElement as Line).Start);
            }
            if (this.AlignmentElement is IrregularLine)
            {
                StartPoint = ExtractPoint((this.AlignmentElement as IrregularLine).Start);
            }
            if (this.AlignmentElement is Curve)
            {
                StartPoint = ExtractPoint((this.AlignmentElement as Curve).Items[0] as PointType);
            }
            if (this.AlignmentElement is Spiral)
            {
                //review    
                StartPoint = ExtractPoint((this.AlignmentElement as Spiral).Items[0] as PointType);
            }
            if (this.AlignmentElement is Chain)
            {
                //Review 
                StartPoint = ExtractPoint((this.AlignmentElement as Chain).Text);
            }
            ExtractHeightForPoint(new RevitPlacmenElement (StartPoint,Station));

            return StartPoint;
        }
        public XYZ GetEndPoint()
        {
            XYZ EndPoint = null;

            if (this.AlignmentElement is Line)
            {
                EndPoint = ExtractPoint((this.AlignmentElement as Line).End);
            }
            if (this.AlignmentElement is IrregularLine)
            {
                EndPoint = ExtractPoint((this.AlignmentElement as IrregularLine).End);
            }
            if (this.AlignmentElement is Curve)
            {
                EndPoint = ExtractPoint((this.AlignmentElement as Curve).Items[2] as PointType);
            }
            if (this.AlignmentElement is Spiral)
            {
                //review    
                EndPoint = ExtractPoint((this.AlignmentElement as Spiral).Items[2] as PointType);
            }
            if (this.AlignmentElement is Chain)
            {
                //Review 
                EndPoint = ExtractPoint((this.AlignmentElement as Chain).Text);
            }

            ExtractHeightForPoint(new RevitPlacmenElement(EndPoint,Station));

            return EndPoint;
        }
        public double GetEndStation()
        {
            if (this.AlignmentElement is Line)
            {
                return (this.Station + (this.AlignmentElement as Line).length);
            }
            if (this.AlignmentElement is IrregularLine)
            {
                return (this.Station + (this.AlignmentElement as IrregularLine).length);
            }
            if (this.AlignmentElement is Curve)
            {
                return (this.Station + (this.AlignmentElement as Curve).length);
            }
            if (this.AlignmentElement is Spiral)
            {
                return (this.Station + (this.AlignmentElement as Spiral).length);
            }
            if (this.AlignmentElement is Chain)
            {
                //Review 
                return (this.Station + (this.AlignmentElement as Chain).station);
            }

            return default(double);
        }
        private static XYZ ExtractPoint(PointType PointType)
        {
            var Point = PointType.Text[0].Split(' ');
            Double X;
            Double Y;
            double.TryParse(Point[0], out X);
            double.TryParse(Point[1], out Y);
            XYZ PointStart = new XYZ(Y, X, 0);
            return PointStart;
        }
        private static XYZ ExtractPoint(string[] PointText)
        {
            var Point = PointText[0].Split(' ');
            Double X;
            Double Y;
            double.TryParse(Point[0], out X);
            double.TryParse(Point[1], out Y);
            XYZ PointStart = new XYZ(Y, X, 0);
            return PointStart;
        }
        public RevitPlacmenElement GetPointAtStation(double StationToStudy)
        {
            RevitPlacmenElement PointElement = null;
            if (this.AlignmentElement is Line)
            {
                XYZ Point  = (this.RevitElement as Autodesk.Revit.DB.Line).Evaluate(StationToStudy - Station, false);
                PointElement = new RevitPlacmenElement(Point ,StationToStudy);

            }
            if (this.AlignmentElement is IrregularLine)
            {
                XYZ Point  = (this.RevitElement as Autodesk.Revit.DB.Line).Evaluate(StationToStudy - Station, false);
                PointElement = new RevitPlacmenElement(Point ,StationToStudy);

            }
            if (this.AlignmentElement is Curve)
            {
                double StationParam; 
               
                //if ((bool)this.GetArcRotationAntiClockWise())
                {
                    StationParam = 1 - (((StationToStudy - this.Station)) / this.GetLength());
                }
                //else
                //{
                //    StationParam = ((StationToStudy - this.Station)/ this.GetLength());
                //}

                XYZ Point =  (this.RevitElement as Autodesk.Revit.DB.Arc).Evaluate(StationParam, true);
                PointElement = new RevitPlacmenElement(Point, StationToStudy);

            }
            if (this.AlignmentElement is Spiral)
            {
                //PolyLine P = PolyLine.Create()
                //Arc HS = Arc.Create(this.GetStartPoint(), this.GetEndPoint(), this.GetPointPI());
                //Point = HS.Evaluate(StationToStudy - Station, false);
                XYZ Point = this.GetStartPoint();
                PointElement = new RevitPlacmenElement(Point, StationToStudy);

            }
            if (this.AlignmentElement is Chain)
            {
                //Review 
                //return ExtractPoint((this.AlignmentElement as Chain).Text);
            }

            ExtractHeightForPoint(PointElement);
            return PointElement;
        }

        private Arc CreateArc(XYZ PointStart, XYZ PointEnd, double radius, bool largeSagitta)
        {
            XYZ midPointChord = 0.5 * (PointStart + PointEnd);

            XYZ v = null;
            if (!(bool)this.GetArcRotationAntiClockWise())
            {
                v = PointEnd - PointStart;
            }
            else
            {
                v = PointStart - PointEnd;
            }
            double d = 0.5 * v.GetLength(); // half chord length

            // Small and large circle sagitta:
            // http://www.mathopenref.com/sagitta.html

            double s = largeSagitta
              ? radius + Math.Sqrt(radius * radius - d * d) // sagitta large
              : radius - Math.Sqrt(radius * radius - d * d); // sagitta small

            var PX = Transform.CreateRotation(XYZ.BasisZ, 0.5 * Math.PI);
            var PX2 = v.Normalize();
            var PX3 = v.Normalize().Multiply(s);
            XYZ midPointArc = midPointChord + Transform.CreateRotation(XYZ.BasisZ, 0.5 * Math.PI).OfVector(v.Normalize().Multiply(s));


            return Arc.Create(PointEnd, PointStart, midPointArc);

        }
        private void ExtractHeightForPoint(RevitPlacmenElement point)
        {


            //LineX LL = LineX.CreateBound(PointBeforeIt, HeightPoint);
            //XYZ Vector = HeightPoint - PointBeforeIt;
            //var Angle = XYZ.BasisX.AngleTo(Vector) * 180 / Math.PI;
            //var XPoint = (StationToStudy - PointBeforeIt.X);
            //var point = LL.Evaluate((XPoint / (HeightPoint.X - PointBeforeIt.X)), true);
            //point = new XYZ(point.X, point.Y, point.Z);
        }
        public double GetLength()
        {
            double Length = 0;
            if (this.AlignmentElement is Line)
            {
                Length = (this.AlignmentElement as Line).length;
            }
            if (this.AlignmentElement is IrregularLine)
            {
                Length = (this.AlignmentElement as IrregularLine).length;

            }
            if (this.AlignmentElement is Curve)
            {
                Length = (this.AlignmentElement as Curve).length;

            }
            if (this.AlignmentElement is Spiral)
            {
                //review    
                Length = (this.AlignmentElement as Spiral).length;

            }
            if (this.AlignmentElement is Chain)
            {
                //Review 
                //Length = (this.AlignmentElement as Chain).length;
            }
            return Length;
        }

        public XYZ GetPointPI()
        {
            if (this.AlignmentElement is Curve)
            {
                return ExtractPoint((this.AlignmentElement as Curve).Items[3] as PointType);
            }
            if (this.AlignmentElement is Spiral)
            {
                return ExtractPoint((this.AlignmentElement as Spiral).Items[1] as PointType);
            }
            return null;
        }
        public XYZ GetPointAtCurveCenter()
        {
            if (this.AlignmentElement is Curve)
            {
                return ExtractPoint((this.AlignmentElement as Curve).Items[1] as PointType);
            }
            if (this.AlignmentElement is Spiral)
            {
                //return ExtractPoint((this.AlignmentElement as Spiral).Items[1] as PointType);
            }
            return null;
        }
        public double GetCurveRadius()
        {
            if (this.AlignmentElement is Curve)
            {
                return (this.AlignmentElement as Curve).radius;
            }
            if (this.AlignmentElement is Spiral)
            {
                //return ExtractPoint((this.AlignmentElement as Spiral).Items[1] as PointType);
            }
            return default(double);
        }
        public double GetCurveStartAngle()
        {
            if (this.AlignmentElement is Curve)
            {
                return (this.AlignmentElement as Curve).dirStart;
            }
            if (this.AlignmentElement is Spiral)
            {
                //return ExtractPoint((this.AlignmentElement as Spiral).Items[1] as PointType);
            }
            return default(double);
        }
        public double GetCurveEndAngle()
        {
            if (this.AlignmentElement is Curve)
            {
                return (this.AlignmentElement as Curve).dirEnd;
            }
            if (this.AlignmentElement is Spiral)
            {
                //return ExtractPoint((this.AlignmentElement as Spiral).Items[1] as PointType);
            }
            return default(double);
        }


        public bool? GetArcRotationAntiClockWise()
        {
            if (this.AlignmentElement is Curve)
            {
                var ArcRot = (this.AlignmentElement as Curve).rot;
                if (ArcRot == clockwise.cw)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return null;
        }
    }
}
