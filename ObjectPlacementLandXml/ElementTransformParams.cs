namespace ObjectPlacementLandXml
{
   public class ElementTransformParams
    {
        public ElementTransformParams(int horizontalDistance, int elevationFromAlignment, int rotationAngleInPlane, int inclinationAngleInXZPlane)
        {
            HorizontalDistance = horizontalDistance;
            ElevationFromAlignment = elevationFromAlignment;
            RotationAngleInPlane = rotationAngleInPlane;
            InclinationAngleInXZPlane = inclinationAngleInXZPlane;
        }
        public ElementTransformParams()
        {

        }
        public double HorizontalDistance { get; set; }
        public double DistanceBetweenStations { get; set; }
        public double ElevationFromAlignment { get; set; }
        public double RotationAngleInPlane { get; set; }
        public double InclinationAngleInXZPlane { get; set; }
        public bool? RotateWithAlignment { get; internal set; }
        public double? StationToStartFrom { get; internal set; }
        public double? StationToEndAt { get; internal set; }
        public bool? CreateAlignment { get; internal set; }
        public bool? CreateStationsAtEndAndStartCheck { get; internal set; }
    }
}