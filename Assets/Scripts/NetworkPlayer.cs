using UnityEngine;
using Fusion;
using TMPro;

namespace Network
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [Header("Model References")]
        [SerializeField] private SkinnedMeshRenderer _characterRenderer;
        [SerializeField] private int hairMaterialIndex = 4;

        [SerializeField] private TextMeshPro playerNameTxt;
        [SerializeField] private Transform cameraPos;
        [SerializeField] private Animator _animator;

        [Header("Networked Properties")]
        [Networked] public Vector3 NetworkedPosition { get; set; }
        [Networked] public Quaternion NetworkedRotation { get; set; }
        [Networked] public Color PlayerColor { get; set; }
        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public int TeamID { get; set; }
        [Networked] public NetworkAnimatorData AnimatorData { get; set; }

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int CrouchHash = Animator.StringToHash("IsCrouching");

        private float _lerpSpeed = 15f;
        private bool _wasJumping;
        private Transform _mainCameraTransform;

        public override void Spawned()
        {
            _mainCameraTransform = Camera.main.transform;

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
                Vector3 camForward = _mainCameraTransform.forward;
                Vector3 camRight = _mainCameraTransform.right;
                camForward.y = 0; camRight.y = 0;
                camForward.Normalize(); camRight.Normalize();

                Vector3 moveDirection = (camForward * input.InputVector.y + camRight * input.InputVector.x).normalized;

                float moveSpeed = input.SprintInput ? 8f : (input.CrouchInput ? 2.5f : 5f);

                transform.position += moveDirection * moveSpeed * Runner.DeltaTime;
                NetworkedPosition = transform.position;

                if (moveDirection.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(moveDirection);
                    NetworkedRotation = transform.rotation;
                }

                float animValue = input.InputVector.magnitude * (input.SprintInput ? 1f : 0.5f);
                AnimatorData = new NetworkAnimatorData { Speed = animValue, Jump = input.JumpInput, IsCrouching = input.CrouchInput };
            }
        }

        public override void Render()
        {
            transform.position = Vector3.Lerp(transform.position, NetworkedPosition, Runner.DeltaTime * _lerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, NetworkedRotation, Runner.DeltaTime * _lerpSpeed);

            if (_characterRenderer != null && _characterRenderer.materials.Length > hairMaterialIndex)
            {
                Material[] mats = _characterRenderer.materials;
                if (mats[hairMaterialIndex].GetColor("_BaseColor") != PlayerColor)
                {
                    mats[hairMaterialIndex].SetColor("_BaseColor", PlayerColor);
                    _characterRenderer.materials = mats;
                }
            }

            if (playerNameTxt != null) playerNameTxt.text = PlayerName.ToString();

            if (_animator != null)
            {
                _animator.SetFloat(SpeedHash, AnimatorData.Speed);
                _animator.SetBool(CrouchHash, AnimatorData.IsCrouching);
                if (AnimatorData.Jump && !_wasJumping)
                {
                    _animator.SetTrigger(JumpHash);
                }
                _wasJumping = (bool)AnimatorData.Jump;
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
}