using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Develop
{
    public class UnitTest : Dictionary<string, object>
    {
        private static readonly Dictionary<string, UnitTest> instances = new Dictionary<string, UnitTest>();

        private UnitTest()
        {
        }

        public void Add(string key)
        {
            Add(key, null);
        }

        public static UnitTest Instance()
        {
            return Instance("_default_");
        }

        public static UnitTest Instance(string module)
        {
            if (!instances.ContainsKey(module))
                instances.Add(module, new UnitTest());

            return instances[module];
        }

        public static void Test(string key, Action<object> testFunction)
        {
            Test("_default_", key, testFunction);
        }

        public static void Test(string module, string key, Action<object> testFunction)
        {
            var unitTest = Instance(module);
            if (unitTest != null && unitTest.ContainsKey(key))
                testFunction.Invoke(unitTest[key]);
        }
    }
}