using SlopperEngine.TestStuff;
using SlopperEngine.Windowing;

MainContext.Instance.Load += () => new TestGame(800,600, "she sloop,,,, my gloop,,,,,,,");
MainContext.Instance.Run();