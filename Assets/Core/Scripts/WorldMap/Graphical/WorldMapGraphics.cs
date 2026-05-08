
using UnityEngine;

/// <summary>
/// Entry point for graphical rendering of world map data.  
/// </summary> 


public class WorldMapGraphics : MonoBehaviour
{

    //several render steps
    //some tick loop
    WorldMapMeshManager worldMeshManager;

    public void Initialize(WorldMapGenData data)
    {
        worldMeshManager = this.AddComponentAsObject<WorldMapMeshManager>();
    }

    public void SupplyData(WorldMapGenData data)
    {
        worldMeshManager.Initialize(data);
    }

    public void GenerateVisuals()
    {
        worldMeshManager.Generate();
    }

    public void TickVisuals()
    {
        worldMeshManager.Tick();
    }

}