using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace ArchiCoder
{
    public class GhcDivisionUneqully : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GhcDivisionUneqully()
          : base(
                "Divide Unequally",
                "DivUnequally",
                "Divides a curve with a customized differing distances bewtween points",
                "Curve",
                "Division")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.BB
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distances", "D", "Distances between points", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Exclude last", "E", "If True, excludes the point at the end of a curve", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Division Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Parameters", "t", "t parameters at division points", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Succesfull", "S", "True if the points in desired distances were found", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        /// 

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -----------------------------------------------------------------------
            // ----------Creating the variables and reading the input values----------
            // -----------------------------------------------------------------------

            Curve tempCurve = (new Arc()).ToNurbsCurve();
            List<double> iDistances = new List<double>();
            bool iDelete = true; 

            DA.GetData(0, ref tempCurve);
            DA.GetDataList(1, iDistances);
            DA.GetData(2, ref iDelete);

            // -----------------------------------------------------------------------
            // ----- At given curve finds divison points and their t parameters ------
            // -----------------------------------------------------------------------


            NurbsCurve iCurve = tempCurve.ToNurbsCurve();

            iCurve.DivideUneqully(iDistances, iDelete, out List<double> intersectionParameters, out List<Point3d> points, out bool[] pattern);

            // -----------------------------------------------------------------------
            // -------------- Stores the results in output variables -----------------
            // -----------------------------------------------------------------------

            DA.SetDataList(0, points);
            DA.SetDataList(1, intersectionParameters);
            DA.SetDataList(2, pattern);
        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return ArchiCoder.Properties.Resources.aCoder_icon_DivCurve;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d92625f9-37e9-4d04-b4bb-58ee9c2f6be5"); }
        }
    }

    // -----------------------------------------------------------------------
    // ------------ Static methods to find the division points ---------------
    // -----------------------------------------------------------------------

    public static class StaticMethods 
    {
        public static void DivideUneqully(this NurbsCurve curve, List<double> distances, bool delete, out List<double> intersectionParameters, out List<Point3d> points, out bool[] pattern)
        {
            int segments = distances.Count;
            points = new List<Point3d> { curve.PointAtStart }; // list to store the division points
            intersectionParameters = new List<double>{curve.Domain.T0}; // list to store the t parameters of division points
            pattern = new bool[segments]; // list to store booleans, True if divison point was created
            for (int i = 0; i < segments; i++) pattern[i] = false;

            int counter = 0;

            for (int i = 0; i < segments; i++)
            {
                if (distances[i] <= 0) continue; // skips when distance is not bigger then zero

                // Checks the intersections of a curve with a sphere which radius is the given distance
                Brep sphere = (new Sphere(points[counter], distances[i])).ToBrep();
                bool succsess = Rhino.Geometry.Intersect.Intersection.CurveBrep(curve, sphere, 0.01, 0.01, out double[] tempParameters);
                if (!succsess) break;

                // Chooses the first intersection point on the curve
                Array.Sort(tempParameters);
                double previous = intersectionParameters[counter];
                double next = curve.Domain.T1;
                foreach (double t in tempParameters) if (t >= previous) { next = t; break; }

                Point3d intersectionPoint = curve.PointAt(next);

                if  (next == previous ||
                    (next == curve.Domain.T1 &&
                    delete == true &&
                    distances[i] != points[counter].DistanceTo(intersectionPoint)))
                    break;
                
                // Stores the divison point and its t parameter 
                intersectionParameters.Add(next);
                points.Add(intersectionPoint);
                pattern[i] = true;

                counter++;

            }
        }
    }
}
