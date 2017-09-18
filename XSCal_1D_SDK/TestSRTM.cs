//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using DHI.Mike1D.CrossSectionModule;
//using DHI.Mike1D.Generic;
//using DHI.Mike1D.Mike1DDataAccess;
//using csmatio.io;
//using csmatio.types;

//namespace XSCal_1D_SDK
//{
//    class TestSRTM
//    {
//        static void read_xns11()
//            //reads cross section data from .xns11 file and writes it to .mat
//        {
//            //define cross section filepath
//            string xns11Filepath = "C:\\rasch\\GangesBrahmaputraModel\\AutomaticXSCal\\XSCal_1D_SDK\\GB_XS_SRTM_50km_Bonly.xns11";
//            //define number of cross sections expected
//            int no_cs = 62;

//            //Load cross section data
//            Diagnostics diagnostics = new Diagnostics("Errors");
//            CrossSectionDataFactory csDataFactory = new CrossSectionDataFactory();
//            CrossSectionData csData = csDataFactory.Open(Connection.Create(xns11Filepath), diagnostics);

//            //Find all Brahmaputra cross sections
//            ICrossSection crossSection;
//            //LocationSpan is EXCLUDING the borders!
//            ICrossSection[] Bcs = csData.FindCrossSectionsForLocationSpan(new LocationSpan("Brahma_SRTM", -10, 3100000), "SRTM");
//            Console.WriteLine("Number of CS in Brahmaputra: {0}", Bcs.Length);
//            if (Bcs.Length != no_cs) throw new Exception("Expected 62 cross sections for the Brahmaputra (at 50km distance)");

//            //create array (chainage, datum and chainage, x, z) that will be written to .mat file
//            int[] dim_d = new int[] { no_cs, 2 };
//            MLDouble mlDatums = new MLDouble("chaindat", dim_d);
//            int[] dim_x = new int[] { no_cs * 3, 3 };
//            MLDouble mlXZ = new MLDouble("xzdat", dim_x);

//            //read .mat file with datums +10m
//            double[,] datums10 = new Double[no_cs, 2];
//            double[] tmp_chain = new Double[no_cs];
//            double[] tmp_datums = new Double[no_cs];
//            MatFileReader mfr = new MatFileReader("C:\\rasch\\GangesBrahmaputraModel\\AutomaticXSCal\\Scripts\\SRMT_XS_inip10.mat");
//            MLDouble mlDatums10 = (mfr.Content["chaindat"] as MLDouble);
//            if (mlDatums10 != null)
//            {
//                double[][] tmp = mlDatums10.GetArray();
//                //tmp_chain = tmp[0];
//                //tmp_datums = tmp[1];
//                for (int i = 0; i < no_cs; i++)
//                {
//                    Console.WriteLine("tmp[{0}][0] = {1}", i, tmp[i][0]);
//                    tmp_chain[i] = tmp[i][0];
//                    tmp_datums[i] = tmp[i][1];
//                }
//            }
//            //combine vectors into array
//            for (int i = 0; i < no_cs; i++)
//            {
//                Console.WriteLine("i = {0}, tmp_chain = {1}, tmp_datums = {2}", i, tmp_chain[i], tmp_datums[i]);
//                datums10[i, 0] = tmp_chain[i];
//                datums10[i, 1] = tmp_datums[i];
//            }

//            //Loop over all Brahmaputra cross sections
//            int idx = 0;
//            int idx2 = 0;
//            foreach (ICrossSection cs in Bcs)
//            {
//                //Check if cross section has raw data:
//                XSBaseRaw xsBaseRaw = cs.BaseCrossSection as XSBaseRaw;
//                if (xsBaseRaw == null)
//                    continue; //It did not have raw data

//                //write chainage and datum to array for .mat file for datum and chainge
//                mlDatums.Set(cs.Location.Chainage, idx, 0);
//                mlDatums.Set(cs.Location.Z, idx, 1);

//                //replace with updated datums from .mat file
//                cs.Location.Z = datums10[idx, 1];
//                idx++;

//                //write data to console
//                Console.WriteLine("Chainge {0}. Datum = {1}", cs.Location.Chainage, cs.Location.Z);
//                foreach (ICrossSectionPoint xsPoint in xsBaseRaw.Points)
//                {
//                    Console.WriteLine("X {0}    Z {1}", xsPoint.X, xsPoint.Z);
//                    mlXZ.Set(cs.Location.Chainage, idx2, 0);
//                    mlXZ.Set(xsPoint.X, idx2, 1);
//                    mlXZ.Set(xsPoint.Z, idx2, 2);
//                    idx2++;
//                }

//                //Update all markers to default
//                xsBaseRaw.UpdateMarkersToDefaults(true, true, true);

//                //Calculate processed levels, storage areas, radii, etc,
//                //i.e. fill in all ProcessedXXX properties
//                xsBaseRaw.CalculateProcessedData();
//            }

//            //Save the cross section as a new file name
//            csData.Connection.FilePath.FileNameWithoutExtension += "_+10";
//            CrossSectionDataFactory.Save(csData);

//            //Save .mat files
//            List<MLArray> mlList = new List<MLArray>();
//            mlList.Add(mlDatums);
//            mlList.Add(mlXZ);
//            MatFileWriter mfw = new MatFileWriter("C:\\rasch\\GangesBrahmaputraModel\\AutomaticXSCal\\Scripts\\SRTM_XS_ini.mat", mlList, false);
//        }
//    }
//}
