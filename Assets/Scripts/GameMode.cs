using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;

    #region Fuion Callbacks

    public override void Spawned()
    {

    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {

    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData input))
        {
            this.transform.position += input.InputVector.normalized * Runner.DeltaTime);
        }
    }

    public override void Render()
    {

    }

    #endregion
}
