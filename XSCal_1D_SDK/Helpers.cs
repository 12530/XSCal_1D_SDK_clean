using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSCal_1D_SDK
{
    public class Helpers
/*
* just some helper functions
* */
    {
        public struct fnames
        {
            public string matFpath;
            public string xns11Fpath;
            public string xns11FpathUpd;
        }


        public static fnames defFileNames(int evalCount, int runId)
        {
            string matFilepath;
            string xns11Filepath;
            string xns11FilepathUpd;

            var thisPath = System.IO.Directory.GetCurrentDirectory();
            //define .mat filepath with updated cross sections
            matFilepath = thisPath + "\\AutomaticXSCal\\Scripts\\run" + runId.ToString() + "\\Songhua_xsection_upd" + evalCount.ToString() + ".mat";
            //define cross section filepath (template, updated will be stored elsewhere)
            xns11Filepath = thisPath + "\\AutomaticXSCal\\Model\\run" + runId.ToString() + "\\" + evalCount.ToString() + "\\Songhua_xsection.xns11";
            //define new file path
            xns11FilepathUpd = thisPath + "\\AutomaticXSCal\\Model\\run" + runId.ToString() + "\\" + evalCount.ToString() + "\\Songhua_xsection" + evalCount.ToString() + ".xns11";

            fnames filenames;
            filenames.matFpath = matFilepath;
            filenames.xns11Fpath = xns11Filepath;
            filenames.xns11FpathUpd = xns11FilepathUpd;

            return filenames;
        }

        public static void showTips()
        {
            Console.WriteLine("SXCal_1D_SDK should be starting with parameter(s)!        |" +
                "1) The first par should be [read_xs_ini] or [update_all]         |" +
                "if [updata_all] is provided, two more parameters are needed       |" +
                "2) The second par should be the number of expected evaluation        |" +
                "3) The last one should be the run ID        |" +
                "re-launch the excuatable file with correct parameters!");
            Console.WriteLine("Press any key to continue!");
            Console.ReadKey();
        }
    }
}
