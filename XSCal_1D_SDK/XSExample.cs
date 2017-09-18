using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using DHI.Mike1D.CrossSectionModule;
using DHI.Mike1D.Generic;
using DHI.Mike1D.Mike1DDataAccess;


namespace XSCal_1D_SDK
{
    class XSExample
    {
        public static void testing()
        {
            //define cross section filepath
            string xns11Filepath = "C:\\Users\\ljia\\Desktop\\XSCal_1D_SDK_clean\\CS_example.xns11";
         //Load cross section data
         Diagnostics diagnostics = new Diagnostics("Errors");
            CrossSectionDataFactory csDataFactory = new CrossSectionDataFactory();
            CrossSectionData csData = csDataFactory.Open(Connection.Create(xns11Filepath), diagnostics);

            //Loop over all cross sections
            foreach (ICrossSection cs in csData)
            {
                //Check if cross section has raw data:
                XSBaseRaw xsBaseRaw = cs.BaseCrossSection as XSBaseRaw;
                if (xsBaseRaw == null)
                    continue; //It did not have raw data

                Console.WriteLine("Bottom Level = {0}", cs.Location.Z);
                Console.WriteLine("Press any key to continue!");
                Console.ReadKey();
                cs.Location.Z = -2.321;
                //add additional 0.4 to all z values in the raw points
                foreach (ICrossSectionPoint xsPoint in xsBaseRaw.Points)
                {
                    //Console.WriteLine(xsPoint.Z);
                    Console.WriteLine("Index = {0}", xsPoint.Index);
                    Console.WriteLine("Zone = {0}", xsPoint.Zone);
                    xsPoint.Z += 0.4;
                    Console.WriteLine(xsPoint.Z);
                }

                //Update all markers to default
                xsBaseRaw.UpdateMarkersToDefaults(true, true, true);

                //Calculate processed levels, storage areas, radii, etc,
                //i.e. fill in all ProcessedXXX properties
                xsBaseRaw.CalculateProcessedData();
            }

            //// Validates the data. The constraints are that the levels and the areas after sorting
            //// must be monotonically increasing.
            //IDiagnostics diagnostics_xs = xsBaseRaw.Validate();

            //if (diagnostics_xs.ErrorCountRecursive > 0)
            //{
            //    throw new Exception(String.Format("Number of errors: {0}", diagnostics_xs.Errors.Count));
            //}

            //Save the cross section as a new file name
            csData.Connection.FilePath.FileNameWithoutExtension += "_2";
            Console.WriteLine("before save");
            CrossSectionDataFactory.Save(csData);
            Console.WriteLine("after save");
        }
    }
}
