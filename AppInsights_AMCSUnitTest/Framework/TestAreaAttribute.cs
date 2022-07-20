using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppInsights_AMCSUnitTest.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestAreaAttribute : Attribute
    {
        public TestAreaAttribute(Areas area)
        {
            this.Area = area;
        }
        public Areas Area { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestOwnerAttribute : Attribute
    {
        public TestOwnerAttribute(Owners owner)
        {
            this.Owner = owner;
        }

        public Owners Owner { get; private set; }
    }
}
