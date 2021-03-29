using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ObjectPlacementLandXml
{
    class LandXmlParser
    {
        public static LandXML Deserialize(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlSerializer serializer = new XmlSerializer(typeof(LandXML));
            StreamReader reader = new StreamReader(path);
            LandXML Schema = (LandXML)serializer.Deserialize(reader);
            reader.Close();
            return Schema;
        }
        internal static List<RevitPlacmenElement> ParseLandXml(string LandXmlPath, double StationIncrement)
        {
            LandXML Landx = Deserialize(LandXmlPath);
            return ExtractPointPlacment(Landx, StationIncrement);

        }

        private static List<RevitPlacmenElement> ExtractPointPlacment(LandXML Landx, double StationIncrement)
        {
            List<RevitPlacmenElement> RevitPlacementPoint = new List<RevitPlacmenElement>();

            foreach (Alignments Alignments in Landx.Items.OfType<Alignments>())
            {
                foreach (var Alignment in Alignments.Alignment)
                {
                    //stationing
                    List<double> Stations = CreateStationing(StationIncrement, Alignment);
                    List<LandXmlStationingObject> LandXmlAlignmentObjects = ExtractStartAndEndStationing(Alignment, Stations);
                    //Placment
                    ExtractPlacementPoints(RevitPlacementPoint, Alignment, Stations, LandXmlAlignmentObjects);
                }
            }

            return RevitPlacementPoint;
        }

        private static void ExtractPlacementPoints(List<RevitPlacmenElement> RevitPlacementPoint, Alignment Alignment, List<double> Stations, List<LandXmlStationingObject> LandXmlAlignmentObjects)
        {
            var StationsToStudy = Stations.Distinct().ToList();
            StationsToStudy.Sort();

            foreach (var LandXmlObject in LandXmlAlignmentObjects)
            {
                for (int i = 0; i < StationsToStudy.Count; i++)
                {
                    var Station = StationsToStudy[i];
                    if (Station == LandXmlObject.Station)
                    {
                        RevitPlacementPoint.Add(new RevitPlacmenElement(LandXmlObject.GetStartPoint(), Station));
                        //StationsToStudy.Remove(Station);
                        continue;
                    }
                    else if (Station == (LandXmlObject.Station + LandXmlObject.GetLength()))
                    {
                        RevitPlacementPoint.Add(new RevitPlacmenElement(LandXmlObject.GetEndPoint(), Station));
                        //StationsToStudy.Remove(Station);

                        continue;

                    }
                    else if (Station > LandXmlObject.Station && Station < (LandXmlObject.Station + LandXmlObject.GetLength()))
                    {
                        var PointAtStatation = LandXmlObject.GetPointAtStation(Station);
                        RevitPlacementPoint.Add(PointAtStatation);
                        //StationsToStudy.Remove(Station);

                        continue;
                    }
                    else
                    {
                        //StationsToStudy.Remove(Station);
                        continue;
                    }
                }

            }
            RevitPlacementPoint.Add(new RevitPlacmenElement(LandXmlAlignmentObjects.Last().GetEndPoint(), (Alignment.length + Alignment.staStart)));
        }

        //Stationing 
        private static List<double> CreateStationing(double StationIncrement, Alignment Alignment)
        {
            List<double> Stations = new List<double>();
            var StartStationX = Alignment.staStart;

            for (double i = StartStationX; i <= Alignment.length + StartStationX; i += StationIncrement)
            {
                Stations.Add(i);
            }

            return Stations;
        }
        private static List<LandXmlStationingObject> ExtractStartAndEndStationing(Alignment Alignment, List<double> Stations)
        {
            var ObjectStation = Alignment.staStart;
            List<LandXmlStationingObject> Objects = new List<LandXmlStationingObject>();

            foreach (CoordGeom CoordGeom in Alignment.Items.OfType<CoordGeom>())
            {
                foreach (object CoordGeoItem in CoordGeom.Items)
                {
                    var LandXmlAlignMentObj = new LandXmlStationingObject(ObjectStation, CoordGeoItem);
                    Objects.Add(LandXmlAlignMentObj);

                    ObjectStation = LandXmlAlignMentObj.GetEndStation();
                    Stations.Add(ObjectStation);
                }

            }

            return Objects;
        }


    }
}
