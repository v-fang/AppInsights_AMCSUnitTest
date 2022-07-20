using Microsoft.Portal.TestFramework.Core.Controls;
using Microsoft.Portal.TestFramework.Core.Controls.Lists;
using Microsoft.Portal.TestFramework.Core.Shell;
using Microsoft.Selenium.Utilities;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Portal.TestFramework.Core.Controls.Forms;

namespace AppInsights_AMCSUnitTest.Framework.Elements
{
    public class MonitorDCRBlade : MonitorBlade
    {
        /// <summary>
        /// Get and load the blade with the blade title
        /// </summary>
        /// <param name="bladeTitle"></param>
        /// <returns></returns>
        public static new MonitorDCRBlade GetBladeAndValidateComponentsLoaded(string bladeTitle)
        {
            MonitorDCRBlade blade = AMCSTestBase.portal.FindSingleBladeByTitle<MonitorDCRBlade>(bladeTitle);
            blade.ValidateBlade();
            return blade;
        }

        public GridRow GetResourceRowWithName(string resourceName)
        {
            var grid = this.DCRGrid;
            return grid.Rows.FirstOrDefault<GridRow>(t => t.Cells[0].Text.Equals(resourceName));
        }

        public CommandBarItem RefreshButton
        {
            get
            {
                return this.FindElements<CommandBarItem>().FirstOrDefault(t => t.GetAttribute("title") == "Refresh");
            }
        }

        public Grid DCRGrid
        {
            get
            {
                return this.WaitForAndFindElement<Grid>();
            }
        }
    }
}
