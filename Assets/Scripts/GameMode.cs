using UnityEngine;
using Fusion;
using TMPro;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Visuals")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private TextMeshPro _nameLabel;

    [Header("Networked Properties")]
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }
    [Networked] public NetworkString<_16> PlayerName { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            NetworkedPosition = transform.position;
        }
        UpdateName();
        UpdateColor();
        if (HasInputAuthority)
        {
            string myName = NetworkManager.LocalPlayerName;
            Color myColor = NetworkManager.LocalPlayerColor;
            RPC_SetPlayerStats(myName, myColor);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            Vector3 direction = new Vector3(input.InputVector.x, 0, input.InputVector.y);
            transform.position += direction * Runner.DeltaTime * 5f;
            NetworkedPosition = transform.position;
        }
    }

    public override void Render()
    {
        transform.position = Vector3.Lerp(transform.position, NetworkedPosition, Time.deltaTime * 10f);

        if (_meshRenderer != null && _meshRenderer.material.color != PlayerColor)
        {
            _meshRenderer.material.color = PlayerColor;
        }

        if (_nameLabel != null && _nameLabel.text != PlayerName.ToString())
        {
            _nameLabel.text = PlayerName.ToString();
        }

        if (HasInputAuthority)
        {
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y + 10, transform.position.z - 10);
            Camera.main.transform.LookAt(transform.position);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerStats(string name, Color color)
    {
        this.PlayerName = name;
        this.PlayerColor = color;
    }

    private void UpdateName()
    {
        if (_nameLabel != null)
        {
            _nameLabel.text = PlayerName.ToString();
        }
    }

    private void UpdateColor()
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.material.color = PlayerColor;
        }
    }
}