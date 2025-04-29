using SlopperEngine.Core;
using SlopperEngine.Physics;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.TestStuff;

/// <summary>
/// An object that spawns a great number of RotateCubes in two rings around itself.
/// </summary>
public class RotateCubeAdder : SceneObject3D
{
    float _time;
    float _threshold;
    RotateCube?[] _cubes;
    System.Random _rand = new();

    public RotateCubeAdder(int count, float cubesPerSecond)
    {
        _cubes = new RotateCube?[count];
        _threshold = 1f/cubesPerSecond;
    }

    [OnRegister]
    void OnRegister()
    {
        LocalScale = (3,3,3);
    }

    [OnFrameUpdate]
    void Update(FrameUpdateArgs args)
    {
        _time += args.DeltaTime;
        if(_time > _threshold)
        {
            _time = 0;
            addNewCube();
        }
    }

    void addNewCube()
    {
        int index = _rand.Next(_cubes.Length);
        //System.Console.WriteLine($"cube {index}:");
        if(_cubes[index] != null)
        {
            if(_rand.NextSingle() < .5f || _cubes[index]!.Parent != Scene)
            {
                _cubes[index]!.Destroy();
                //System.Console.WriteLine("destroyed");
            }
            else
            {
                Children.Add(_cubes![index]!);
                //System.Console.WriteLine("reparented");
                return;
            }
        }

        //System.Console.WriteLine("newly added");
        RotateCube friend = new();
        _cubes[index] = friend;
        Scene!.Children.Add(friend);
        float r = _rand.NextSingle() * MathF.Tau;
        friend.LocalPosition.X = MathF.Cos(r)*6f;
        friend.LocalPosition.Z = MathF.Sin(r)*6f;
    }
}