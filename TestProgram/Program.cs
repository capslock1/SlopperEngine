using System;
using System.Reflection;
using SlopperEngine.Core.SceneComponents;
using SlopperEngine.Rendering;
using SlopperEngine.SceneObjects;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Interaction;
using SlopperEngine.UI.Layout;
using SlopperEngine.Windowing;

namespace TestProgram;

public class Program : SlopperEngine.Core.Mods.ISlopModEvents
{
    /// <summary>
    /// Makes a new window with a button to start each demo.
    /// </summary>
    public static void OnModLoad()
    {
        try
        {
            // went on a wild goose chase trying to fix the issues this was causing earlier. 
            // multithreading should reaaaallly be fixed, but im not sure what even is the issue...
            MainContext.MultithreadedFrameUpdate = false;

            Scene mainScene = Scene.CreateEmpty();
            UIRenderer rend = new();
            rend.Resize(new(500, 375));
            mainScene.Renderers.Add(rend);
            mainScene.Components.Add(new UpdateHandler());

            Window w = Window.Create(new(new(500, 375), Title: "Sloppy Demos"));
            w.CenterWindow();
            w.Scene = mainScene;
            w.WindowTexture = mainScene.Renderers.FirstOfType<UIRenderer>()!.GetOutputTexture();
            w.Closing += c =>
            {
                // just nuke the whole program if main gets closed to be honest
                for(int i = Scene.ActiveScenes.Count-1; i >= 0; i--)
                    Scene.ActiveScenes[i].Destroy();
                for(int i = Window.AllWindows.Count-1; i >= 0; i--)
                    if(Window.AllWindows[i] != w)
                        Window.AllWindows[i].Close();
            };

            var uiContainer = new Spacer();
            mainScene.Children.Add(uiContainer);
            var buttonContainer = new ScrollableArea(new(0,0,1,1));
            uiContainer.UIChildren.Add(buttonContainer);
            buttonContainer.Layout.Value = new LinearArrangedLayout
            {
                ChildAlignment = Alignment.Min,
                StartAtMax = true,
                IsLayoutHorizontal = false,
            };

            foreach(var t in Assembly.GetExecutingAssembly().GetTypes())
            {
                var interfaces = t.GetInterfaces();
                if(interfaces.Length == 0) continue;

                foreach(var i in interfaces)
                {
                    if(i != typeof(IDemo)) continue;
                    
                    var map = t.GetInterfaceMap(i);
                    MethodInfo? createScene = null;
                    string demoName = t.Name;
                    string demoDescription = "No description.";
                    foreach(var m in map.TargetMethods)
                    {
                        if(m.Name.EndsWith(nameof(IDemo.CreateDemoScene)))
                            createScene = m;
                        if(m.Name.EndsWith(nameof(IDemo.GetName)))
                            demoName = m.Invoke(null, null) as string ?? demoName;
                        if(m.Name.EndsWith(nameof(IDemo.GetDescription)))
                            demoDescription = m.Invoke(null, null) as string ?? demoDescription;
                    }
                    buttonContainer.UIChildren.Add(new DemoButton(uiContainer, createScene!, demoName, demoDescription));
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine($"TestProgram could not run due to an unexpected error: {e.Message}");
            Console.WriteLine("Press 'Enter' to give up.");
            Console.ReadLine();
        }
    }
}