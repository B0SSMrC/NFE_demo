using Unity.Entities;

public struct ChampTag : IComponentData{}
public struct NewChampTag : IComponentData{}
public struct MobaTeam : IComponentData
{
    public TeamType Value;
}