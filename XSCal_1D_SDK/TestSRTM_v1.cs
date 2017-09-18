using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using DHI.Mike1D.CrossSectionModule;
using DHI.Mike1D.Generic;
using DHI.Mike1D.Mike1DDataAccess;
using csmatio.io;
using csmatio.types;

namespace XSCal_1D_SDK
{
    class TestSRTM
    {
        static void Main()
        {
            //define cross section filepath
            string xns11Filepath = "C:\\rasch\\GangesBrahmaputraModel\\AutomaticXSCal\\XSCal_1D_SDK\\GB_XS_SRTM_50km_Bonly.xns11";
            //define number of cross sections expected
            int no_cs = 62;

            //Load cross section data
            Diagnostics diagnostics = new Diagnostics("Errors");
            CrossSectionDataFactory csDataFactory = new CrossSectionDataFactory();
            CrossSectionData csData = csDataFactory.Open(Connection.Create(xns11Filepath), diagnostics);

            //Find all Brahmaputra cross sections
            ICrossSection crossSection;
            //LocationSpan is EXCLUDING the borders!
            ICrossSection[] Bcs = csData.FindCrossSectionsForLocationSpan(new LocationSpan("Brahma_SRTM", -10, 3100000), "SRTM");
            Console.WriteLine("Number of CS in Brahmaputra: {0}", Bcs.Length);
            if (Bcs.Length != no_cs) throw new Exception("Expected 62 cross sections for the Brahmaputra (at 50km distance)");
            
            //create array (chainage, datum columns) that will be written to .mat file
            int[] dims = new int[] { no_cs, 2 };
            MLDouble mlDatums = new MLDouble("chaindat", dims);

            //read .mat file with datums +10m
            double[,] datums10 = new Double[no_cs, 2];
            double[] tmp_chain = new Double[no_cs];
            double[] tmp_datums = new Double[no_cs];
            MatFileReader mfr = new MatFileReader("C:\\rasch\\GangesBrahmaputraModel\\AutomaticXSCal\\Scripts\\SRTM_XS_ini+10.mat");
            MLDouble mlDatums10 = (mfr.Content["chaindat"] as MLDouble);
            if (mlDatums10 != null)
            {
                double[][] tmp = mlDatums10.GetArray();
                tmp_chain = tmp[0];
                tmp_datums = tmp[1];
            }
            //combine vectors into array
            var list_tmp = new List<Double>();
            list_tmp.AddRange(tmp_chain);
            list_tmp.AddRange(tmp_datums);
            datums10 = list_tmp.ToArray();

            int i = 0;
            Console.WriteLine("Chainage {0} has datum {1}", datums10[0,0], datums10[0,1]);

            //Loop over all Brahmaputra cross sections
            foreach (ICrossSection cs in Bcs)
            {
                //Check if cross section has raw data:
                XSBaseRaw xsBaseRaw = cs.BaseCrossSection as XSBaseRaw;
                if (xsBaseRaw == null)
                    continue; //It did not have raw data

                //write chainage and datum to array for .mat file
                mlDatums.Set(cs.Location.Chainage, i, 0);
                mlDatums.Set(cs.Location.Z, i, 1);

                //replace with updated datums from .mat file


                i += 1;


                //write data to console
                Console.WriteLine("Chainge {0}. Datum = {1}", cs.Location.Chainage, cs.Location.Z);
                foreach (ICrossSectionPoint xsPoint in xsBaseRaw.Points)
                {
                    Console.WriteLine("X {0}    Z {1}", xsPoint.X, xsPoint.Z);
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
                csData.Connection.FilePath.FileNameWithoutExtension += "_+10";
                Console.WriteLine("before save");
                CrossSectionDataFactory.Save(csData);
                Console.WriteLine("after save");

                //Save .mat files
                List<MLArray> mlList = new List<MLArray>();
                mlList.Add(mlDatums);
                MatFileWriter mfw = new MatFileWriter("C:\\rasch\\GangesBrahmaputraModel\\AutomaticXSCal\\Scripts\\SRTM_XS_ini.mat", mlList, false);
        }
    }
}
    