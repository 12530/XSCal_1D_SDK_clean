using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DHI.Mike1D.CrossSectionModule;
using DHI.Mike1D.Generic;
using DHI.Mike1D.Mike1DDataAccess;
using csmatio.io;
using csmatio.types;

namespace XSCal_1D_SDK
{
    public class DotMatIO
    /*
     * everything related to "talking to MATLAB" via .mat files using the 
     * csmatio C# library and manipulating the xns11 files (i.e. the main functions)
     * */
    /* structure of xns11 file
     * -----cross section------------------------points of cross section----------------------
     *  chainage    Z                         chainage    X       Z
     *  --------------------------------------------------------------------
     *                                           1        0       15
     *     1        7                            1        10      7
     *                                           1        20      15
     *  --------------------------------------------------------------------   
     *                                           2        0       12
     *     2        4                            2        10      4
     *                                           2        20      12
     *  --------------------------------------------------------------------
     *                                           *        *       *
     *     *        *                            *        *       *
     *                                           *        *       *
     *  --------------------------------------------------------------------     
     * */
    {
        public static void read_ini()
        /*
         * function to read the initial cross section datums and x,z coordinates from the file defined in xns11Filepath. 
         * The data is then written to the .mat file defined in matFilepath, which afterwards can be used in MATLAB to
         * initiate the optimization
         * */
        {
            //Console.WriteLine("Begin DotMatIO.read_ini()");
            #region definitions

            //define cross section file to be read 
            var thisPath = System.IO.Directory.GetCurrentDirectory();
            string xns11Fpath = thisPath + "\\AutomaticXSCal\\Model\\" + "Songhua_xsection.xns11";
            //define .mat file to save the output 
            string matFpath = thisPath + "\\AutomaticXSCal\\Model\\" + "Songhua_xsection_ini.mat";

            //define number of cross sections expected 
            int no_cs = 45;
            //create array (chainage, datum and chainage, x, z) that will be written to .mat file 
            int[] dim_d = new int[] { no_cs, 2 };
            MLDouble mlDatums = new MLDouble("chaindat", dim_d);
            int[] dim_x = new int[] { no_cs * 4, 3 }; // no_cs*4 for each cross-section, there are four points representing a trapezoidal shape
            MLDouble mlXZ = new MLDouble("xzdat", dim_x); 

            #endregion

            #region read xns stuff

            //Load cross section data
            Diagnostics diagnostics = new Diagnostics("Errors");
            CrossSectionDataFactory csDataFactory = new CrossSectionDataFactory();
            CrossSectionData csData = csDataFactory.Open(Connection.Create(xns11Fpath), diagnostics);
            Console.WriteLine("Number of all CS: {0}",csData.Count);
            //Find all cross sections
            //LocationSpan is EXCLUDING the borders!
            //public IList<ICrossSection> FindCrossSectionsForLocationSpan(ILocationSpan locationSpan, string topoID, bool excludeEnds);
            //public LocationSpan(string River/Reach id, double startChainage, double endChainage);
            Location LL = new Location("Main", 0);
            ICrossSection CSS = csData.FindCrossSection(LL, "SH_interpolation");
            LocationSpan LS = new LocationSpan("Main", 0, 437300);
            IList<ICrossSection> Bcs = csData.FindCrossSectionsForLocationSpan(LS, "SH_interpolation", false);
            
            Console.WriteLine("Number of CS: {0}", Bcs.Count);
            if (Bcs.Count != no_cs) throw new Exception("Expected 45 cross sections for the 433 km reach");

            //Loop over all cross sections
            int idx = 0;
            int idx2 = 0;
            foreach (ICrossSection cs in Bcs)
            {
                //Check if cross section has raw data:
                XSBaseRaw xsBaseRaw = cs.BaseCrossSection as XSBaseRaw;
                if (xsBaseRaw == null)
                    continue; //It did not have raw data

                //write chainage and datum to array for .mat file for datum and chainge
                mlDatums.Set(cs.Location.Chainage, idx, 0);
                mlDatums.Set(cs.Location.Z, idx, 1);

                idx++;

                //write data to console
                //Console.WriteLine("Chainge {0}. Datum = {1}", cs.Location.Chainage, cs.Location.Z);
                // loop each point in one section, 4 points for trapezoidal shape. Each point has Chainage, X and Z value.
                foreach (ICrossSectionPoint xsPoint in xsBaseRaw.Points)
                {
                    //Console.WriteLine("X {0}    Z {1}", xsPoint.X, xsPoint.Z);
                    mlXZ.Set(cs.Location.Chainage, idx2, 0);
                    mlXZ.Set(xsPoint.X, idx2, 1);
                    mlXZ.Set(xsPoint.Z, idx2, 2);
                    idx2++;
                }
            }

            #endregion

            #region save csmatio stuff

            //Save .mat files
            List<MLArray> mlList = new List<MLArray>();
            mlList.Add(mlDatums);
            mlList.Add(mlXZ);
            MatFileWriter mfw = new MatFileWriter(matFpath, mlList, false);
            #endregion

            //Console.WriteLine("End DotMatIO.read_ini()");
        }


        public static void write_update_all(int evalCount, int runId)
        /*
         * function to read a .mat file that stores the updated cross section 
         * datums as determined by the Matlab GA. Those values are then 
         * written to an updated version of the .xns11 file which will be 
         * used in the model run
         * */
        {
            //Console.WriteLine("Begin DotMatIO.write_update()");
            #region definitions

            //get filenames
            XSCal_1D_SDK.Helpers.fnames filenames; //declare struct holding filenames
            filenames = XSCal_1D_SDK.Helpers.defFileNames(evalCount, runId);


            //define number of cross sections expected
            int no_cs;
            no_cs = 45; //how many cross sections are expected?
            //arrays to hold values from .mat file
            double[,] datums_upd = new Double[no_cs, 2];
            double[] tmp_chain = new Double[no_cs];
            double[] tmp_datums = new Double[no_cs];
            double[,] xz_upd = new Double[no_cs, 3];
            //double[] tmp_x = new Double[no_cs];
            //double[] tmp_z = new Double[no_cs];
            double[] tmp_b = new Double[no_cs];
            double[] tmp_tan = new Double[no_cs];
            #endregion

            # region csmatio stuff

            MatFileReader mfr = new MatFileReader(filenames.matFpath);
            MLDouble mlall_upd = (mfr.Content["chaindat_new"] as MLDouble); // .mat structure content [chaindat_new] including four cols, i.e.
                                                                            // chinage, datum, x and z of cross-sectional points
            if (mlall_upd != null)
            {
                double[][] tmp = mlall_upd.GetArray();
                for (int i = 0; i < no_cs; i++)
                {
                    tmp_chain[i] = tmp[i][0];
                    tmp_datums[i] = tmp[i][1];
                    tmp_b[i] = tmp[i][2];
                    tmp_tan[i] = tmp[i][3];
                }
            }
            //combine vectors into array
            for (int i = 0; i < no_cs; i++)
            {
                datums_upd[i, 0] = tmp_chain[i];
                datums_upd[i, 1] = tmp_datums[i];
                xz_upd[i, 0] = tmp_chain[i];
                xz_upd[i, 1] = tmp_b[i]; // bottom width
                xz_upd[i, 2] = tmp_tan[i]; // tan(alpha)
            }


            #endregion

            #region xns updte stuff

            //Load cross section data
            Diagnostics diagnostics = new Diagnostics("Errors");
            CrossSectionDataFactory csDataFactory = new CrossSectionDataFactory();
            CrossSectionData csData = csDataFactory.Open(Connection.Create(filenames.xns11Fpath), diagnostics); //all cross sections
            
            //Find all relevant cross sections
            //LocationSpan is EXCLUDING the borders!
            IList<ICrossSection> Bcs = csData.FindCrossSectionsForLocationSpan(new LocationSpan("Main", 0, 437300), "SH_interpolation", false);

            //Console.WriteLine("Number of CS in Brahmaputra: {0}", Bcs.Length);
            if (Bcs.Count != no_cs)
            {
                throw new Exception("Expected number of cross sections not matching!" );
            } 

            //Loop over all cross sections
            int idx = 0;
            foreach (ICrossSection cs in Bcs)
            {
                //Check if cross section has raw data:
                XSBaseRaw xsBaseRaw = cs.BaseCrossSection as XSBaseRaw;
                if (xsBaseRaw == null)
                    continue; //It did not have raw data

                //replace with updated datums and angles from .mat file
                cs.Location.Z = datums_upd[idx, 1]; //datums

                foreach (ICrossSectionPoint xsPoint in xsBaseRaw.Points) //angles
                {
                    if (xsPoint.Index == 0) //point 1
                    {
                        //x1 remains
                       
                    }
                    else if (xsPoint.Index == 1) //point 2
                    {
                        xsPoint.X = 500 / xz_upd[idx,2]; // 500 is the full bank hight in .xns11 file
                        //z2 remains at old value (lowest point of cross section, i.e. Z0)
                    }
                    else if (xsPoint.Index == 2) //point 3
                    {
                        xsPoint.X = xz_upd[idx, 1] + 500 / xz_upd[idx, 2];
                        //z3 remains at old value (lowest point of cross section, i.e. Z0)
                    }
                    else if (xsPoint.Index == 3) //point 4
                    {
                        xsPoint.X = xz_upd[idx, 1] + 500 / xz_upd[idx, 2] * 2;
                        //xsPoint.Z = xz_upd[idx, 3]; //z4 from GA i.e. d
                    }
                }



                idx++; // move to next cross section

                //Update all markers to default
                xsBaseRaw.UpdateMarkersToDefaults(false, false, false); // maintain 

                //Calculate processed levels, storage areas, radii, etc,
                //i.e. fill in all ProcessedXXX properties
                xsBaseRaw.CalculateProcessedData();
            }

            //Save the cross section as a new file name
            //csData.Connection.FilePath.FileNameWithoutExtension += xnsAdd;
            csData.Connection.FilePath.Path = filenames.xns11FpathUpd;
            CrossSectionDataFactory.Save(csData);

            #endregion

            //Console.WriteLine("End DotMatIO.write_update()");
        }

    }
}
