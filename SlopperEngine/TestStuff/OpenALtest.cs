using SlopperEngine.Core;
using SlopperEngine.SceneObjects;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace SlopperEngine.TestStuff;

public class OpenALtest : SceneObject
{
    int _source;
    int _buffer;
    ALContext _context;
    ALDevice _device;
    float[] _orientation = {0,0,-1,0,1,0};
    bool _done = false;
    Vector3 _previousPosition = (0,0,0);

    [OnRegister]
    void CreateContext()
    {
        //list all available devices
        var devices = ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);
        Console.WriteLine("Devices: ");
        foreach(var j in devices)
            Console.WriteLine(j);
        Console.WriteLine();

        _device = ALC.OpenDevice(ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier));
        Console.WriteLine("Using device: "+ALC.GetString(_device, AlcGetString.DeviceSpecifier));
        _context = ALC.CreateContext(_device, new ALContextAttributes());
        if(!ALC.MakeContextCurrent(_context))
        {
            Console.WriteLine($"Couldnt make ALContext {_context} current.");
            return;
        }
        CheckError("on setting device");

        AL.Listener(ALListener3f.Position, 0, 0, 1);
        AL.Listener(ALListener3f.Velocity, 0, 0, 0);
        AL.Listener(ALListenerfv.Orientation, _orientation);
        CheckError("on setting listener");

        AL.GenSource(out _source);
        AL.Source(_source, ALSource3f.Position, 0,0,0);
        AL.Source(_source, ALSourcef.Gain, .5f);
        AL.Source(_source, ALSourcef.Pitch, 1);
        AL.Source(_source, ALSource3f.Velocity, 0,0,0);
        AL.Source(_source, ALSourceb.Looping, true);
        CheckError("on setting source");

        AL.GenBuffer(out _buffer);
        Random rand = new();
        short[] data = new short[4401]; 
        for(int i = 0; i<data.Length; i++)
        {
            data[i] = (short)rand.Next();
           // if((i & 1) == 0) continue;
           // data[i] = short.MaxValue;
        }
        AL.BufferData(_buffer, ALFormat.Mono16, data, 600);
        AL.Source(_source, ALSourcei.Buffer, _buffer);
        CheckError("on buffer");

        AL.SourcePlay(_source);
        CheckError("on play");
    }

    [OnFrameUpdate]
    void CheckExistence(FrameUpdateArgs args)
    {
        if(_done) return;

        var transform = GetGlobalTransform();
        var velocity = transform.Row3.Xyz - _previousPosition;
        velocity /= args.DeltaTime;
        _previousPosition = transform.Row3.Xyz;
        AL.Listener(ALListener3f.Position, transform.Row3.X, transform.Row3.Y, transform.Row3.Z);
        var upward = transform.Row1.Xyz;
        var forward = -transform.Row2.Xyz; //negative because opengl
        _orientation[0] = forward.X;
        _orientation[1] = forward.Y;
        _orientation[2] = forward.Z;
        _orientation[3] = upward.X;
        _orientation[4] = upward.Y;
        _orientation[5] = upward.Z;
        AL.Listener(ALListenerfv.Orientation, _orientation);
        AL.Listener(ALListener3f.Velocity, velocity.X, velocity.Y, velocity.Z);

        AL.GetSource(_source, ALGetSourcei.SourceState, out int state);
        if((ALSourceState)state == ALSourceState.Playing) return;

        AL.DeleteSource(_source);
        CheckError("on delete source");
        AL.DeleteBuffer(_buffer);
        CheckError("on delete buffer");
        ALC.MakeContextCurrent(ALContext.Null);
        CheckError("on make context null");
        ALC.DestroyContext(_context);
        CheckError("on destroy context");
        var err = ALC.GetError(_device);
        Console.WriteLine("WAAAHHH!!!!"+err);
        Console.WriteLine(ALC.CloseDevice(_device));
        _done = true;
    }

    void CheckError(string error)
    {
        ALError err = AL.GetError();
        if(err != ALError.NoError)
            Console.WriteLine($"ALError at {error}: {AL.GetErrorString(err)}");
    }
}