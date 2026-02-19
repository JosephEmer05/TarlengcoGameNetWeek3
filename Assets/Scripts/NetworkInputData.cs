using UnityEngine;
using Fusion;

namespace Network
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 InputVector;
        public NetworkBool JumpInput;
        public NetworkBool SprintInput;
        public NetworkBool CrouchInput;
    }
}