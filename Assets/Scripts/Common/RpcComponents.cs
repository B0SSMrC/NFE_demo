using Unity.NetCode;

namespace TMG.NFE_Tutorial
{
    public struct MobaTeamRequest : IRpcCommand
    {
        public TeamType Value;
    }
}