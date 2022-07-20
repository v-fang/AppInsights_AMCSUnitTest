using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppInsights_AMCSUnitTest.Framework
{
    public class Constants
    {
        public static string DCRFullName = "Data Collection Rules";
        public static string DCEFullName = "Data Collection Endpionts";
        public const string DCRValidation = "DCR Validation";
        public static readonly TimeSpan DefaultTimeSpan = TimeSpan.FromSeconds(90);
    }
    /// <summary>
    /// The owner for this class of tests, should be per-team
    /// </summary>
    public enum Owners
    {
        Fang
    }

    /*
     * The area these tests will be testing, ie. ServerOverview for the 
     * Server Overview blade, MetricsExplorer for the ME blade.
     */
    public enum Areas
    {
        DCR,
        DCEOverview,
        DCE,
        DCROverview
    };
}
