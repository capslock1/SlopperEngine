using SlopperEngine.SillyDemos;
using SlopperEngine.Windowing;

MainContext.Instance.Load += () => new Demos();
MainContext.Instance.Run();