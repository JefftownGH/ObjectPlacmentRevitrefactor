#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
#endregion

namespace ObjectPlacementLandXml
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            RibbonControl Ribbon = ComponentManager.Ribbon;
            Autodesk.Windows.RibbonTab AfryTab = null;
            Autodesk.Windows.RibbonPanel Alignmentpanel = null;

            foreach (var Tab in Ribbon.Tabs)
            {
                if ("AFRY" == Tab.Title)
                {
                    AfryTab = Tab;
                    foreach (var item in Tab.Panels)
                    {
                        if (item.Source.Title == "Alignment Tools")
                        {
                            Alignmentpanel = item;
                        }
                    }
                }
            }
            if (Alignmentpanel == null)
            {
                if (AfryTab == null)
                {
                    try
                    {
                        a.CreateRibbonTab("AFRY");

                    }
                    catch (Exception)
                    {

                    }
                    try
                    {
                        a.CreateRibbonPanel("AFRY", "Alignment Tools");
                    }
                    catch (Exception)
                    {

                    }

                }
                else
                {
                    a.CreateRibbonPanel("AFRY", "Alignment Tools");

                }

            }


            var Locationath = Assembly.GetExecutingAssembly().Location;
            PushButtonData Create3d = new PushButtonData("Create", "Object Placer", Locationath, "ObjectPlacementLandXml.Command");
            Create3d.ToolTip = "This Command creates 3D text alignments when u select a Model line"; // Can be changed to a more descriptive text.
            Create3d.Image = new BitmapImage(new Uri(Path.GetDirectoryName(Locationath) + "\\OBjAlignment.png"));
            Create3d.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(Locationath) + "\\OBJAlignment.png"));

            a.GetRibbonPanels("AFRY").Find(E => E.Name == "Alignment Tools").AddItem(Create3d);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
