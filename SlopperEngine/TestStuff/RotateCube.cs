using SlopperEngine.Graphics;
using SlopperEngine.Core;
using SlopperEngine.Rendering;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics.DefaultResources;
using OpenTK.Mathematics;

namespace SlopperEngine.TestStuff;

/// <summary>
/// A cube that slowly spins in a chaotic fashion.
/// </summary>
public class RotateCube : MeshRenderer
{
    const float _speed = 2f;
    Random _rand = new();
    Vector3 _rotDirection = Vector3.Zero;
    Quaternion _unsmoothedRotation = Quaternion.Identity;
    float _timeRotatedCube = 1;
    static int _number = 0;
    int _myNumber;

    int _updateCount = 0;

    static List<RotateCube> _allOfThem = new();

    public static void ListThem()
    {
        foreach(var j in _allOfThem)
        {
            Console.WriteLine(j);
            j._updateCount = 0;
        }
    }

    [OnRegister]
    void onAdd()
    {
        Mesh = DefaultMeshes.Cube;
        Material = Material.Create(SlopperShader.Create("shaders/phongShader.sesl"));
        _myNumber = _number;
        _number++;
        _allOfThem.Add(this);
    }

    [OnFrameUpdate]
    void Update(FrameUpdateArgs args)
    {
        _updateCount++;
        _timeRotatedCube += args.DeltaTime;
        if (_timeRotatedCube > .8f)
        {
            switch (_rand.Next(0, 6))
            {
                case 0:
                    _rotDirection = new Vector3(_speed, 0, 0);
                    break;
                case 1:
                    _rotDirection = new Vector3(0, _speed, 0);
                    break;
                case 2:
                    _rotDirection = new Vector3(0, 0, _speed);
                    break;
                case 3:
                    _rotDirection = new Vector3(-_speed, 0, 0);
                    break;
                case 4:
                    _rotDirection = new Vector3(0, -_speed, 0);
                    break;
                case 5:
                    _rotDirection = new Vector3(0, 0, -_speed);
                    break;
            }
            _timeRotatedCube = 0;
        }
        _unsmoothedRotation = Quaternion.FromEulerAngles(_rotDirection*args.DeltaTime)*_unsmoothedRotation;
        LocalRotation = Quaternion.Slerp(LocalRotation, _unsmoothedRotation, 0.3f*args.DeltaTime);
    }

    [OnUnregister]
    void OnUnregister(Scene? _)
    {
        _allOfThem.Remove(this);
    }
    public override string ToString()
    {
        return "rotateCube "+_myNumber.ToString()+" got "+_updateCount+" update calls";
    }
}
