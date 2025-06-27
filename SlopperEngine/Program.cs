using SlopperEngine.Windowing;

MainContext.Instance.Load += () => new SlopperEngine.TestStuff.TestGame(800, 600, "she sloop,,,, my gloop,,,,,,,");
//MainContext.Instance.Load += () => new SlopperEngine.SillyDemos.Demos(); MainContext.MultithreadedFrameUpdate = false;
MainContext.Instance.Run();