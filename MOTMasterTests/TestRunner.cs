using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MOTMasterTests
{
    public static class Assert
    {
        public static void True(bool condition, string what)
        {
            if (!condition) throw new Exception("Assertion failed: " + what);
        }

        public static void Equal<T>(T expected, T actual, string what = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception("Expected <" + expected + "> but got <" + actual + ">"
                    + (what == null ? "" : " (" + what + ")"));
        }

        public static TException Throws<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException e)
            {
                return e;
            }
            catch (Exception e)
            {
                throw new Exception("Expected " + typeof(TException).Name + " but got " + e.GetType().Name + ": " + e.Message);
            }
            throw new Exception("Expected " + typeof(TException).Name + " but nothing was thrown");
        }
    }

    public static class TestRunner
    {
        /// <summary>Asserts that every part occurs in msg.</summary>
        public static void StringAssert(string msg, params string[] parts)
        {
            foreach (string part in parts)
                if (msg == null || !msg.Contains(part))
                    throw new Exception("Expected message to contain \"" + part + "\" but was: " + msg);
        }

        public static int Main(string[] args)
        {
            var tests = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "MOTMasterTests" && t.IsClass)
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(m => m.Name.StartsWith("Test") && m.ReturnType == typeof(void))
                .OrderBy(m => m.DeclaringType.Name).ThenBy(m => m.Name)
                .ToList();

            int failed = 0;
            foreach (MethodInfo test in tests)
            {
                try
                {
                    var parameters = test.GetParameters();
                    if (parameters.Length == 0) test.Invoke(null, null);
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]))
                        test.Invoke(null, new object[] { args });
                    else throw new Exception("Unsupported test signature");
                    Console.WriteLine("PASS " + test.Name);
                }
                catch (Exception e)
                {
                    failed++;
                    Exception inner = e is TargetInvocationException && e.InnerException != null ? e.InnerException : e;
                    Console.WriteLine("FAIL " + test.Name + ": " + inner);
                }
            }
            Console.WriteLine((tests.Count - failed) + "/" + tests.Count + " passed, " + failed + " failed");
            return failed == 0 ? 0 : 1;
        }
    }
}
