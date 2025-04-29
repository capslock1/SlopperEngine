using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.TestStuff;

/// <summary>
/// Really bad tester for how many updates the engine can handle.
/// </summary>
public class AddRemoveSpeedTester : SceneObject
{
    static int _coolNumber = 0;
    float _perFrame = 10;
    List<Guy> _guys = new();
    bool disableSpawning = false;
    bool disableIncrease = false;
    float cooldownTillNext = 0;

    [OnInputUpdate]
    void Update(InputUpdateArgs args)
    {
        if(args.KeyboardState.IsKeyPressed(Keys.P))
            Console.WriteLine($"Can deal with about {_guys.Count} guys.");
        
        disableSpawning = args.KeyboardState.IsKeyDown(Keys.L);

        if(args.KeyboardState.IsKeyPressed(Keys.O))
        {
            disableIncrease = !disableIncrease;
            Console.WriteLine(disableIncrease ? "disabled increasing guy spawn rate." : "enabled increasing guy spawn rate!");
        }
    }

    [OnFrameUpdate]
    void FrameUpdate(FrameUpdateArgs args)
    {
        cooldownTillNext += args.DeltaTime;
        if(cooldownTillNext > 2 + args.DeltaTime && !disableIncrease)
        {
            cooldownTillNext = 0;
            _perFrame *= 1.5f;
            for(int i = 0; i<_perFrame; i++)
            {
                //SceneObject parent = guys.Count != 0 ? guys[rand.Next(guys.Count)] : this;
                Guy friend = new();
                Children.Add(friend);
                _guys.Add(friend);
            }
        }

        if(disableSpawning) return;

        for(int i = _guys.Count-1; i>-1; i--)
            _guys[i].Remove();

        foreach(Guy guy in _guys)
            Children.Add(guy);        
    }

    class Guy : SceneObject
    {
        [OnFrameUpdate]
        void Update(FrameUpdateArgs args)
        {
            _coolNumber++;
        }
    }
}