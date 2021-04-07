using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arc = Autodesk.Revit.DB.Arc;
using Transform = Autodesk.Revit.DB.Transform;

namespace ObjectPlacementLandXml
{
    class LandXmlStationingObject
    {
        public double Station { get; set; }
        public double EndStation { get; set; }
        public object AlignmentSegmentElement { get; set; }
        public object RevitSegmentElement { get; set; }

        public Alignment Alignment { get; set; }


        public LandXmlStationingObject(double station, object alignmentElement, Alignment alignment)
        {
            Station = station;
            AlignmentSegmentElement = alignmentElement;
            EndStation = station + this.GetLength();

            CreateRevitElement();
        }

        private void CreateRevitElement()
        {
            if (this.AlignmentSegmentElement is Line)
            {
                Autodesk.Revit.DB.Line L = Autodesk.Revit.DB.Line.CreateBound(this.GetStartPoint().PlacementPoint, this.GetEndPoint().PlacementPoint);
                RevitSegmentElement = L;
            }
            if (this.AlignmentSegmentElement is IrregularLine)
            {
                Autodesk.Revit.DB.Line L = Autodesk.Revit.DB.Line.CreateBound(this.GetStartPoint().PlacementPoint, this.GetEndPoint().PlacementPoint);
                RevitSegmentElement = L;

            }
            if (this.AlignmentSegmentElement is Curve)
            {
                var StartPoint = this.GetStartPoint();
                var EndPoint = this.GetEndPoint();
                var Radius = this.GetCurveRadius();

                Arc C = CreateArc(StartPoint.PlacementPoint, EndPoint.PlacementPoint, Radius, (bool)false);
                RevitSegmentElement = C;

            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                var Sp = (this.AlignmentSegmentElement as Spiral);
                RevitSegmentElement = CreateaSpiral(Sp);

            }
            if (this.AlignmentSegmentElement is Chain)
            {
                //Review 
                //return ExtractPoint((this.AlignmentElement as Chain).Text);
            }

        }

        private Autodesk.Revit.DB.Curve CreateaSpiral(Spiral Sp)
        {
            var Splength = Sp.length;
            var spEndRadius = Sp.radiusEnd;
            var SpStartRad = Sp.radiusStart;
            var SpType = Sp.spiType;
            var SpTheta = Sp.theta;
            var SpTotalx = Sp.totalX;
            var SpTotaly = Sp.totalY;
            var spTanLong = Sp.tanLong;
            var spTanShort = Sp.tanShort;
            var Rot = Sp.rot;

            var startPoint = ExtractPoint(Sp.Items[0]);
            var EndPoint = ExtractPoint(Sp.Items[2]);
            var PiPoint = ExtractPoint(Sp.Items[1]);

            double Radius = default(double);

            bool StraightPartAtStart = false;
            if (double.IsInfinity(SpStartRad))
            {
                Radius = spEndRadius;
            }
            else
            {
                Radius = SpStartRad;
                StraightPartAtStart = true;
            }

            var A = Math.Sqrt(Radius * Splength);
            var tao = Math.Pow(A, 2) / (2 * Math.Pow(Radius, 2));

            // double SubDivisions = Math.Round((Splength / ObjectPlacement.Stationincrement));
            //Change
            double SubDivisions = Math.Round((Splength * 20));
            var step = tao / SubDivisions;

            List<XYZ> ControlPoints = new List<XYZ>();

          

            for (double i = 0.0; i < tao; i = i + step)
            {
                var x = A * Math.Sqrt(2 * i) * (1 - (Math.Pow(i, 2) / 10) + (Math.Pow(i, 4) / 216));
                var y = A * Math.Sqrt(2 * i) * ((i / 3) - (Math.Pow(i, 3) / 42) + (Math.Pow(i, 5) / 1320));

                //var Point = new XYZ(x, y, 0);           
                if (Rot == clockwise.ccw)
                {
                    var Point = new XYZ(y, x, 0) + EndPoint;
                    ControlPoints.Add(Point);

                }
                else
                {
                    var Point = new XYZ(y, x, 0) + startPoint;
                    ControlPoints.Add(Point);
                }
            }

            var V1 = (ControlPoints.Last() - ControlPoints.First()).Normalize();
            var V2 = (EndPoint - startPoint).Normalize();
            var Angle = V1.AngleTo(V2);

            List<double> Weights = Enumerable.Repeat(1.0, ControlPoints.Count).ToList();
            NurbSpline P = (NurbSpline)NurbSpline.CreateCurve(ControlPoints, Weights);
            
           
            var TransForm = Transform.CreateRotationAtPoint(XYZ.BasisZ,Angle, startPoint);
            NurbSpline RotatedCurve = (NurbSpline) P.CreateTransformed(TransForm);
            // var dir = (EndPoint - PiPoint).Normalize();
            // var AngleRotation = dir.AngleOnPlaneTo(XYZ.BasisX, XYZ.BasisZ);
            // var aXIS = (startPoint - XYZ.BasisZ).Normalize();

            //var RotatedCurve = (NurbSpline)P.CreateTransformed(Transform.CreateRotation(aXIS, Math.PI/4));





            List<XYZ> ConvertedPoints = new List<XYZ>();
            //Will Be deleted Crap
            using (Transaction T = new Transaction(Command.uidoc.Document, "Create Spiral"))
            {
                T.Start();
                foreach (var item in RotatedCurve.CtrlPoints)
                {
                    ConvertedPoints.Add(RevitPlacmenElement.ConvertPointToInternal(item));
                }


                var P2 = NurbSpline.CreateCurve(ConvertedPoints, Weights);
                DetailCurve D = Command.uidoc.Document.Create.NewDetailCurve(Command.uidoc.Document.ActiveView, P2);

                //if (Rot == clockwise.ccw)
                //{
                //    var plAn = Plane.CreateByNormalAndOrigin(XYZ.BasisY, XYZ.Zero);
                //    ElementTransformUtils.MirrorElement(Command.uiapp.ActiveUIDocument.Document, D.Id, plAn);
                //}
                //if (StraightPartAtStart == true)
                //{
                //    var plAn = Plane.CreateByNormalAndOrigin(XYZ.BasisX, XYZ.Zero);
                //    ElementTransformUtils.MirrorElement(Command.uiapp.ActiveUIDocument.Document, D.Id, plAn);
                //    //ElementTransformUtils.MirrorElement(Command.uiapp.ActiveUIDocument.Document,P,Plane.)
                //}


                ////maybe flip 
                //XYZ dir = (startPoint - PiPoint).Normalize();
                //var AngleRotation = dir.AngleOnPlaneTo(XYZ.BasisX, XYZ.BasisZ);

                //if (StraightPartAtStart)
                //{
                //    dir = (EndPoint - PiPoint).Normalize();
                //    AngleRotation = dir.AngleOnPlaneTo(XYZ.BasisX, XYZ.BasisZ);
                //    ElementTransformUtils.MoveElement(Command.uidoc.Document, D.Id, EndPoint);
                //    Autodesk.Revit.DB.Line L = Autodesk.Revit.DB.Line.CreateBound(new XYZ(0, 0, 1), new XYZ(0, 0, 100));

                //    ElementTransformUtils.RotateElement(Command.uidoc.Document, D.Id, L, ((Math.PI / 2) - AngleRotation));
                //}
                //else
                //{
                //    dir = (startPoint - PiPoint).Normalize();
                //    AngleRotation = dir.AngleOnPlaneTo(XYZ.BasisX, XYZ.BasisZ);
                //    ElementTransformUtils.MoveElement(Command.uidoc.Document, D.Id, startPoint);
                //    Autodesk.Revit.DB.Line L = Autodesk.Revit.DB.Line.CreateBound(new XYZ(0, 0, 1), new XYZ(0, 0, 100));
                //    ElementTransformUtils.RotateElement(Command.uidoc.Document, D.Id, L, -AngleRotation);
                //}
                T.Commit();
            }


            RevitSegmentElement = P;
            return P;
        }

        public RevitPlacmenElement GetStartPoint()
        {
            XYZ StartPoint = null;
            if (this.AlignmentSegmentElement is Line)
            {
                StartPoint = ExtractPoint((this.AlignmentSegmentElement as Line).Start);
            }
            if (this.AlignmentSegmentElement is IrregularLine)
            {
                StartPoint = ExtractPoint((this.AlignmentSegmentElement as IrregularLine).Start);
            }
            if (this.AlignmentSegmentElement is Curve)
            {
                StartPoint = ExtractPoint((this.AlignmentSegmentElement as Curve).Items[0] as PointType);
            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                //review    
                StartPoint = ExtractPoint((this.AlignmentSegmentElement as Spiral).Items[0] as PointType);
            }
            if (this.AlignmentSegmentElement is Chain)
            {
                //Review 
                StartPoint = ExtractPoint((this.AlignmentSegmentElement as Chain).Text);
            }
            var StartPointPlacement = ExtractHeightForPoint(new RevitPlacmenElement(StartPoint, Station));

            return StartPointPlacement;
        }
        public RevitPlacmenElement GetEndPoint()
        {
            XYZ EndPoint = null;

            if (this.AlignmentSegmentElement is Line)
            {
                EndPoint = ExtractPoint((this.AlignmentSegmentElement as Line).End);
            }
            if (this.AlignmentSegmentElement is IrregularLine)
            {
                EndPoint = ExtractPoint((this.AlignmentSegmentElement as IrregularLine).End);
            }
            if (this.AlignmentSegmentElement is Curve)
            {
                EndPoint = ExtractPoint((this.AlignmentSegmentElement as Curve).Items[2] as PointType);
            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                //review    
                EndPoint = ExtractPoint((this.AlignmentSegmentElement as Spiral).Items[2] as PointType);
            }
            if (this.AlignmentSegmentElement is Chain)
            {
                //Review 
                EndPoint = ExtractPoint((this.AlignmentSegmentElement as Chain).Text);
            }

            var EndPointPlacment = ExtractHeightForPoint(new RevitPlacmenElement(EndPoint, EndStation));

            return EndPointPlacment;
        }
        public double GetEndStation()
        {
            if (this.AlignmentSegmentElement is Line)
            {
                return (this.Station + (this.AlignmentSegmentElement as Line).length);
            }
            if (this.AlignmentSegmentElement is IrregularLine)
            {
                return (this.Station + (this.AlignmentSegmentElement as IrregularLine).length);
            }
            if (this.AlignmentSegmentElement is Curve)
            {
                return (this.Station + (this.AlignmentSegmentElement as Curve).length);
            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                return (this.Station + (this.AlignmentSegmentElement as Spiral).length);
            }
            if (this.AlignmentSegmentElement is Chain)
            {
                //Review 
                return (this.Station + (this.AlignmentSegmentElement as Chain).station);
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
            if (this.AlignmentSegmentElement is Line)
            {
                XYZ Point = (this.RevitSegmentElement as Autodesk.Revit.DB.Line).Evaluate(StationToStudy - Station, false);
                PointElement = new RevitPlacmenElement(Point, StationToStudy);

            }
            if (this.AlignmentSegmentElement is IrregularLine)
            {
                XYZ Point = (this.RevitSegmentElement as Autodesk.Revit.DB.Line).Evaluate(StationToStudy - Station, false);
                PointElement = new RevitPlacmenElement(Point, StationToStudy);

            }
            if (this.AlignmentSegmentElement is Curve)
            {
                double StationParam;
                StationParam = 1 - (((StationToStudy - this.Station)) / this.GetLength());
                XYZ Point = (this.RevitSegmentElement as Autodesk.Revit.DB.Arc).Evaluate(StationParam, true);
                PointElement = new RevitPlacmenElement(Point, StationToStudy);

            }
            if (this.AlignmentSegmentElement is Spiral)
            {

                //PolyLine P = PolyLine.Create()
                //Arc HS = Arc.Create(this.GetStartPoint(), this.GetEndPoint(), this.GetPointPI());
                //Point = HS.Evaluate(StationToStudy - Station, false);
                PointElement = this.GetStartPoint();

            }
            if (this.AlignmentSegmentElement is Chain)
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
        private RevitPlacmenElement ExtractHeightForPoint(RevitPlacmenElement point)
        {
            if (this.Alignment != null)
            {
                foreach (Profile Prof in this.Alignment.Items.OfType<Profile>())
                {
                    foreach (var Profilealign in Prof.Items.OfType<ProfAlign>())
                    {
                        foreach (var PVI in Profilealign.Items.OfType<PVI>())
                        {
                            //ExtractHeightPoint(PVI);
                        }
                    }
                }

            }
            return point;

            //LineX LL = LineX.CreateBound(PointBeforeIt, HeightPoint);
            //XYZ Vector = HeightPoint - PointBeforeIt;
            //var Angle = XYZ.BasisX.AngleTo(Vector) * 180 / Math.PI;
            //var XPoint = (StationToStudy - PointBeforeIt.X);
            //var point = LL.Evaluate((XPoint / (HeightPoint.X - PointBeforeIt.X)), true);
            //point = new XYZ(point.X, point.Y, point.Z);
        }

        private static XYZ ExtractHeightPoint(PVI PVI)
        {
            var Point = PVI.Text;
            var Tx = PVI.Text[0].Split(' ');
            Double PVIX;
            Double PVIZ;

            double.TryParse(Tx[0], out PVIX);
            double.TryParse(Tx[1], out PVIZ);

            XYZ PointStart = new XYZ(PVIX, 0, PVIZ);
            return PointStart;
        }

        public double GetLength()
        {
            double Length = 0;
            if (this.AlignmentSegmentElement is Line)
            {
                Length = (this.AlignmentSegmentElement as Line).length;
            }
            if (this.AlignmentSegmentElement is IrregularLine)
            {
                Length = (this.AlignmentSegmentElement as IrregularLine).length;

            }
            if (this.AlignmentSegmentElement is Curve)
            {
                Length = (this.AlignmentSegmentElement as Curve).length;

            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                //review    
                Length = (this.AlignmentSegmentElement as Spiral).length;

            }
            if (this.AlignmentSegmentElement is Chain)
            {
                //Review 
                //Length = (this.AlignmentElement as Chain).length;
            }
            return Length;
        }

        public XYZ GetPointPI()
        {
            if (this.AlignmentSegmentElement is Curve)
            {
                return ExtractPoint((this.AlignmentSegmentElement as Curve).Items[3] as PointType);
            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                return ExtractPoint((this.AlignmentSegmentElement as Spiral).Items[1] as PointType);
            }
            return null;
        }
        public XYZ GetPointAtCurveCenter()
        {
            if (this.AlignmentSegmentElement is Curve)
            {
                return ExtractPoint((this.AlignmentSegmentElement as Curve).Items[1] as PointType);
            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                //return ExtractPoint((this.AlignmentElement as Spiral).Items[1] as PointType);
            }
            return null;
        }
        public double GetCurveRadius()
        {
            if (this.AlignmentSegmentElement is Curve)
            {
                return (this.AlignmentSegmentElement as Curve).radius;
            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                //return ExtractPoint((this.AlignmentElement as Spiral).Items[1] as PointType);
            }
            return default(double);
        }
        public double GetCurveStartAngle()
        {
            if (this.AlignmentSegmentElement is Curve)
            {
                return (this.AlignmentSegmentElement as Curve).dirStart;
            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                //return ExtractPoint((this.AlignmentElement as Spiral).Items[1] as PointType);
            }
            return default(double);
        }
        public double GetCurveEndAngle()
        {
            if (this.AlignmentSegmentElement is Curve)
            {
                return (this.AlignmentSegmentElement as Curve).dirEnd;
            }
            if (this.AlignmentSegmentElement is Spiral)
            {
                //return ExtractPoint((this.AlignmentElement as Spiral).Items[1] as PointType);
            }
            return default(double);
        }


        public bool? GetArcRotationAntiClockWise()
        {
            if (this.AlignmentSegmentElement is Curve)
            {
                var ArcRot = (this.AlignmentSegmentElement as Curve).rot;
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
