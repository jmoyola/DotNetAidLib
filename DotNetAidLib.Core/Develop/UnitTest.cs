using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Develop
{
    public class UnitTest : Dictionary<String, Object>
    {
        private static Dictionary<String, UnitTest> instances = new Dictionary<string, UnitTest>();
        private UnitTest(){}

        public void Add(String key) {
            this.Add(key, null);
        }

        public static UnitTest Instance(){
            return Instance("_default_");
        }

        public static UnitTest Instance(String module) {
            if (!instances.ContainsKey(module))
                instances.Add(module, new UnitTest());

            return instances[module];
        }

        public static void Test(String key, Action<Object> testFunction){
            Test("_default_", key, testFunction);
        }

        public static void Test(String module, String key, Action<Object> testFunction) {
            UnitTest unitTest = Instance(module);
            if (unitTest != null && unitTest.ContainsKey(key))
                testFunction.Invoke(unitTest[key]);
        }
    }
}
