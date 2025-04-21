
using SlopperEngine.Engine.SceneData;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Engine.SceneComponents;

/// <summary>
/// Handles registration in Scenes.
/// </summary>
public class RegisterHandler
{
    public bool QueueFinished => _registryQueue.Count == 0;

    readonly Scene _scene;
    readonly List<(OnRegister, OnUnregister)> _registryQueue = new();
    
    public RegisterHandler(Scene owner)
    {
        _scene = owner;
        _scene!.AddDataContainer(new RegisterContainer(this));
        _scene.AddDataContainer(new UnregisterContainer(this));
    }

    public void Resolve()
    {
        //not using foreach here - OnRegister() (or technically OnUnregister()) could add to the queue, which would throw an error
        for(int i = 0; i<_registryQueue.Count; i++)
        {
            var reg = _registryQueue[i];
            if(reg.Item1.Owner is not null) reg.Item1.Invoke();
            if(reg.Item2.Owner is not null) reg.Item2.Invoke(_scene);
        }
        _registryQueue.Clear();
    }


    private class UnregisterContainer : SceneDataContainer<OnUnregister>
    {
        RegisterHandler owner;
        public UnregisterContainer(RegisterHandler owner)
        {
            this.owner = owner;
        }
        public override void FinalizeQueue(){}
        public override SceneDataHandle QueueAdd(OnUnregister Data) => new(0);
        public override void QueueRemove(SceneDataHandle Handle, OnUnregister Data) => owner._registryQueue.Add((new(), Data));
        public override IEnumerator<OnUnregister> GetEnumerator() => null!;
    }
    private class RegisterContainer : SceneDataContainer<OnRegister>
    {
        RegisterHandler owner;
        public RegisterContainer(RegisterHandler owner)
        {
            this.owner = owner;
        }
        public override void FinalizeQueue(){}
        public override SceneDataHandle QueueAdd(OnRegister Data)
        {
            owner._registryQueue.Add((Data, new()));
            return new(0);
        }
        public override void QueueRemove(SceneDataHandle Handle, OnRegister Data) {}
        public override IEnumerator<OnRegister> GetEnumerator() => null!;
    }
}