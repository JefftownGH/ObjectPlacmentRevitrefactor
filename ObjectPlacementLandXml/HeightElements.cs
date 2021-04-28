using Autodesk.Revit.DB;

namespace ObjectPlacementLandXml
{
    internal class HeightElements
    {
        public (double, double) Range { get; set; }
        public Autodesk.Revit.DB.Curve SegmentElement { get; set; }

        public double GetHeightAtPoint(double station)
        {
            double Height = default(double);
            if (SegmentElement is Autodesk.Revit.DB.Line)
            {
                Height = (SegmentElement as Autodesk.Revit.DB.Line).Evaluate(station,false).Z;
            }
            else if (SegmentElement is Arc)
            {
                Height = (SegmentElement as Autodesk.Revit.DB.Arc).Evaluate(station,false).Z;
            }
            return Height;
        }
        public HeightElements((double, double) range, Autodesk.Revit.DB.Curve segmentElement)
        {
            Range = range;
            SegmentElement = segmentElement;
        }
    }
}