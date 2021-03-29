using Autodesk.Revit.DB;

namespace ObjectPlacementLandXml
{
    public class RevitPlacmenElement
    {
        public XYZ PlacementPoint { get; set; }
        public double Station { get; set; }

        public RevitPlacmenElement(XYZ placementPoint, double station)
        {
            PlacementPoint = placementPoint;
            Station = station;
        }
        public static XYZ ConvertPointToInternal(XYZ PointToConvert)
        {
            if (PointToConvert != null)
            {
                var ConvetedPoint = new XYZ(UnitUtils.ConvertToInternalUnits(PointToConvert.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(PointToConvert.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(PointToConvert.Z, DisplayUnitType.DUT_METERS));
                return ConvetedPoint;
            }
            return null;
        }
        public static double ConvertDoubleToInternal(double DoubleToConvert)
        {
            if (DoubleToConvert != default(double))
            {
                var ConvetedPoint = UnitUtils.ConvertToInternalUnits(DoubleToConvert, DisplayUnitType.DUT_METERS);
                return ConvetedPoint;
            }
            return default(double);
        }
        public static double ConvertAngleToInternal(double AngleToConvert)
        {
            if (AngleToConvert != default(double))
            {
                var ConvetedPoint = UnitUtils.ConvertToInternalUnits(AngleToConvert, DisplayUnitType.DUT_DEGREES_AND_MINUTES);
                return ConvetedPoint;
            }
            return default(double);
        }
        public void FillAttributes(FamilyInstance FamIns)
        {
            FamIns.LookupParameter("Text").Set(Station.ToString());
        }

    }
}