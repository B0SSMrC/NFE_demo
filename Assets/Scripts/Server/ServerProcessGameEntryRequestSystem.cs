using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
namespace TMG.NFE_Tutorial
{
    public partial struct ServerProcessGameEntryRequestSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MobaPrefabs>();
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<MobaTeamRequest, ReceiveRpcCommandRequest>();
            state.RequireForUpdate(state.GetEntityQuery(builder));
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var championPrefab = SystemAPI.GetSingleton<MobaPrefabs>().Champion;
            foreach (var (teamRequest, requestSource, requestEntity) in 
            SystemAPI.Query<MobaTeamRequest, ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                ecb.DestroyEntity(requestEntity);
                ecb.AddComponent<NetworkStreamInGame>(requestSource.SourceConnection);

                var requestedTeamtype = teamRequest.Value;

                if(requestedTeamtype == TeamType.AutoAssign)
                {
                    requestedTeamtype = TeamType.Blue;
                }

                var clientId = SystemAPI.GetComponent<NetworkId>(requestSource.SourceConnection).Value;

                Debug.Log($"Server is assigning Client ID: {clientId} to the {requestedTeamtype.ToString()} team.");

                var newChamp = ecb.Instantiate(championPrefab);
                ecb.SetName(newChamp, "Champion");
                var spawnPosition = new float3(0, 1, 0);
                var newTransfrom = LocalTransform.FromPosition(spawnPosition);
                ecb.SetComponent(newChamp, newTransfrom);
                ecb.SetComponent(newChamp, new GhostOwner{NetworkId = clientId});
                ecb.SetComponent(newChamp, new MobaTeam{Value = requestedTeamtype});

                ecb.AppendToBuffer(requestSource.SourceConnection, new LinkedEntityGroup{Value = newChamp});
            }

            ecb.Playback(state.EntityManager);
        }
    }

}
