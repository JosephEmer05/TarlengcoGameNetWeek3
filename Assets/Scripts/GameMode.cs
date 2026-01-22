using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    #region Fusion Callbacks
    //relevant to the network, do it in spawned (initialization)

    [Header("Networked Properties")]

    [Networked]  public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {

        }

        if (HasStateAuthority)
        {
            PlayerColor = Random.ColorHSV();
        }
    }

    //On destroy
    public override void Despawned(NetworkRunner runner, bool hasState)
    {

    }

    //update function
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData input))
        {
            this.transform.position += 
                new Vector3(input.InputVector.normalized.x, input.InputVector.normalized.y) 
                * Runner.DeltaTime;

            NetworkedPosition = this.transform.position;
        }
    }

    //happens after fixedupdatenetwork, for nonserver objects
    public override void Render()
    {
        this.transform.position = NetworkedPosition;
        if (_meshRenderer != null && _meshRenderer.material.color != PlayerColor)
        {
            _meshRenderer.material.color = PlayerColor;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerColor(Color color)
    {
        if (HasStateAuthority)
        {
            this.PlayerColor = color;
        }
    }
    #endregion

    #region Unity Callbacks
    private void Update()
    {
        if (!HasInputAuthority) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var randColor = Random.ColorHSV();
            RPC_SetPlayerColor(randColor);
        }
    }
    #endregion
}