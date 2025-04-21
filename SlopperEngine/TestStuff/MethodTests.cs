using System.Reflection;
using SlopperEngine.Engine;
using SlopperEngine.TestStuff;

/// <summary>
/// Quick and dirty test of the relative speeds of function calls.
/// </summary>
 public static class MethodTests
{
    public static unsafe void test()
    {
        PerformanceTester a = new();
        Sl b = new();
        Sl c = new();
        Sl d = new();
        Sl e = new();

        for(int j = 0; j<10; j++)
        {
            int sljjc = 0;
            for(int i = 0; i<1000000; i++)
                sljjc++;

            a.StartTest();
            for(int i = 0; i<100000; i++)
                sljjc++;
            a.EndTest("direct ++ (no call)");

            a.StartTest();
            for(int i = 0; i<100000; i++)
                b.d();
            a.EndTest("direct call");

            MethodInfo inf = typeof(Sl).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)[0];
            var handle = inf.MethodHandle;
            var fnPointer = handle.GetFunctionPointer();
            var fn = (delegate*<object, void>)fnPointer;
            var del = inf.CreateDelegate<Action<Sl>>();
            
            a.StartTest();
            for(int i = 0; i<100000; i++)
                fn(c);
            a.EndTest("delegate from function pointer");

            Action k = d.d;
            a.StartTest();
            for(int i = 0; i<100000; i++)
                k.Invoke();
            a.EndTest("action (direct assignment)");

            a.StartTest();
            for(int i = 0; i<100000; i++)
                del.Invoke(e);
            a.EndTest("delegate (createDelegate)");
        }
    }
    class Sl 
    {
        public int j;
        public void d()
        {
            j++;
        }
    }
}
