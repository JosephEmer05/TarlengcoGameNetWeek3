using UnityEngine;
using Fusion;
using TMPro;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private TextMeshPro playerNameTxt;
    [SerializeField] private Transform cameraPos;

    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }
    [Networked] public NetworkString<_32> PlayerName { get; set; }
    [Networked] public int TeamID { get; set; }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            SetupCamera();
            RPC_SetPlayerCustoms(
                NetworkSessionManager.Instance.localPlayerColor,
                NetworkSessionManager.Instance.localPlayerName,
                NetworkSessionManager.Instance.localTeamID
            );
        }
    }

    private void SetupCamera()
    {
        GameObject camera = GameObject.FindWithTag("MainCamera");
        if (camera != null)
        {
            camera.transform.SetParent(cameraPos);
            camera.transform.localPosition = Vector3.zero;
            camera.transform.localRotation = Quaternion.identity;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (GetInput(out NetworkInputData input))
        {
            Vector3 moveDirection = new Vector3(input.InputVector.x, 0, input.InputVector.y).normalized;
            transform.position += moveDirection * Runner.DeltaTime * 5f;
            NetworkedPosition = transform.position;
        }
    }
    public override void Render()
    {
        transform.position = NetworkedPosition;

        if (_meshRenderer != null && _meshRenderer.material.color != PlayerColor)
        {
            _meshRenderer.material.color = PlayerColor;
        }

        if (playerNameTxt != null && playerNameTxt.text != PlayerName.ToString())
        {
            playerNameTxt.text = PlayerName.ToString();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerCustoms(Color color, string name, int team)
    {
        PlayerColor = color;
        PlayerName = name;
        TeamID = team;
    }
}