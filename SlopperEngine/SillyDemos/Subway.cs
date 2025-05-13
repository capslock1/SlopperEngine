using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Physics;
using SlopperEngine.Graphics;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics.DefaultResources;
using SlopperEngine.SceneObjects.Rendering;
using SlopperEngine.TestStuff;

namespace SlopperEngine.SillyDemos;

/// <summary>
/// Subway service.
/// </summary>
public class Subway : SceneObject
{
    public float SpawnDelay = .75f;
    public float Speed = 3f;
    public PhysicsObject? Player;

    // ill admit, this class is a little... bad. make of it what you will

    // using three different childlists, the three different subway service paths are split up.
    // this both prevents me from having to take care of my own lists for keeping track,
    // and it prevents a little bit of ram from being lost by virtue of only having to store this once
    // making childlists mightve been my best decision on this engine ever
    ChildList<RotateCube>[] _cubes;
    RotateCube[] _closestCubeToPlayer;
    Path _currentSpawn = default;
    float _spawnTimer;
    int _currentCurrentPathSpawns = 0;
    float _playerPosition = 7;
    Vector3 _playerSide = Vector3.UnitX*2;
    Path _playerPath;
    
    public Subway()
    {
        _cubes = new ChildList<RotateCube>[3];
        _closestCubeToPlayer = new RotateCube[3];
        for(int i = 0; i<3; i++)
            _cubes[i] = new(this);

        // add the path railing things
        var mat = Material.Create(SlopperShader.Create(Assets.GetPath("shaders/phongShader.sesl")));
        var scale = new Vector3(.4f,.1f,20f);
        Children.Add(new MeshRenderer()
        {
            LocalPosition = GetPosFromPath(Path.Left) - Vector3.UnitY,
            Material = mat,
            LocalScale = scale,
            Mesh = DefaultMeshes.Cube
        });
        Children.Add(new MeshRenderer()
        {
            LocalPosition = GetPosFromPath(Path.Right) - Vector3.UnitY,
            Material = mat,
            LocalScale = scale,
            Mesh = DefaultMeshes.Cube
        });
        Children.Add(new MeshRenderer()
        {
            LocalPosition = GetPosFromPath(Path.Middle) - Vector3.UnitY,
            Material = mat,
            LocalScale = scale,
            Mesh = DefaultMeshes.Cube
        });
    }

    [OnFrameUpdate] void FrameUpdate(FrameUpdateArgs args)
    {
        var globalPos = GetGlobalTransform().ExtractTranslation();
        if(Player != null)
        {
            // move the player to the desired position
            // considering plimbo is a rigidbody, i had to make the field a rigidbody too instead of a sceneobject3d
            // which is inconvenient. in the near future i definitely hope to implement some sort of IPositionable and IRotateable
            var jumpy = Vector3.UnitY * MathF.Abs(MathF.Sin(Scene!.UpdateHandler!.TimeMilliseconds*.005f));
            var backoffset = Vector3.UnitZ * _playerPosition;
            _playerSide -= (_playerSide - GetPosFromPath(_playerPath)) * (1-MathF.Exp(-5*args.DeltaTime));
            Player.Position = globalPos + jumpy + backoffset + _playerSide;

            // idk how i came up with this but like, it works flawlessly
            // i think it reads in front of the player to see what cube is there?
            var closest = _closestCubeToPlayer[(int)_playerPath+1];
            if(closest != null && closest.LocalPosition.Z < _playerPosition)
            {
                Random r = new();
                _playerPath = _playerPath switch
                {
                    Path.Left or Path.Right => Path.Middle,
                    _ => r.NextSingle() > .5f ? Path.Left : Path.Right,
                };
            }
        }

        _spawnTimer += args.DeltaTime;
        if(_spawnTimer > SpawnDelay)
        {
            _spawnTimer = 0;
            OnSubwayTick();
        }
        
        // update cubes and delete ones out of frame
        RotateCube? toDelete = default;
        for(int i = 0; i<_cubes.Length; i++)
        {
            foreach(var c in _cubes[i].All)
            {
                c.LocalPosition.Z += args.DeltaTime * Speed;
                if(c.LocalPosition.Z > _playerPosition-2.5f)
                {
                    if(_closestCubeToPlayer[i] == null)
                        _closestCubeToPlayer[i] = c;
                    else if(_closestCubeToPlayer[i].LocalPosition.Z > c.LocalPosition.Z)
                        _closestCubeToPlayer[i] = c;
                } 
                if(c.LocalPosition.Z > 10)
                    toDelete = c;
            }
        }
        toDelete?.Destroy();
    }

    void OnSubwayTick()
    {
        // advanced spawning algorithm
        var pos = GetPosFromPath(_currentSpawn);
        pos.Z = -10;
        _cubes[(int)_currentSpawn + 1].Add(new(){
            LocalPosition = pos,
            LocalScale = new(.75f),
        });
        _currentCurrentPathSpawns++;

        Random rand = new();
        if(rand.NextSingle() < (_currentCurrentPathSpawns < 3 ? .2f : .7f))
        {
            _currentCurrentPathSpawns = 0;
            bool coinflip = rand.NextSingle() < .5f;
            _spawnTimer -= .5f*SpawnDelay;
            switch(_currentSpawn)
            {
                case Path.Middle:
                if (coinflip)
                    _currentSpawn = Path.Left;
                else _currentSpawn = Path.Right;
                break;
                case Path.Left:
                if(coinflip)
                    _currentSpawn = Path.Middle;
                else _currentSpawn = Path.Right;
                break;
                default:
                if(coinflip)
                    _currentSpawn = Path.Middle;
                else _currentSpawn = Path.Left;
                break;
            }
        }
    }

    static Vector3 GetPosFromPath(Path p) => p switch{
        Path.Left => Vector3.UnitX*2,
        Path.Middle => Vector3.Zero,
        Path.Right => -Vector3.UnitX*2,
        _ => default,
    };

    enum Path : sbyte
    {
        Left = -1,
        Middle = 0,
        Right = 1
    }
}