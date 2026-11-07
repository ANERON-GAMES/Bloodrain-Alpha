// ==== Bloodrain, ANERON GAMES. An excerpt for COMP305 Game Programming I, Part 3-A (alpha). ====
// ==== From MapSurvey.cs in the private Unity 6 project, shortened: it shows how the map finds every roof in the region. ====
// ==== It does not compile on its own; the rest of the class and the project stay private. ====
using Unity.Collections;
using UnityEngine;

// ==== MapSurvey: what the world holds, read from the loaded scene and from nothing else. The roofs are asked the way ====
// ==== SurvivalBody asks (a ray up from a body's height over the ground) on a 512 x 512 grid, 16384 rays a frame in a ====
// ==== batch of raycast jobs, so the map shows the same shelter the needs use ====
public sealed partial class MapSurvey
{
    public const int RoofSize = 512;
    private const int RaysPerStep = 16384;

    private readonly byte[] roofs = new byte[RoofSize * RoofSize];
    private int next = -1;

    // ==== one step of the survey: the first reads the places and exits, each next asks RaysPerStep roofs ====
    public void Step()
    {
        if (Done)
        {
            return;
        }
        if (next < 0)
        {
            ReadSpots();
            next = 0;
            return;
        }
        int count = Mathf.Min(RaysPerStep, roofs.Length - next);
        Ask(next, count);
        next += count;
        if (next >= roofs.Length)
        {
            Finish();
        }
    }

    private void Ask(int first, int count)
    {
        NativeArray<RaycastCommand> asks = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
        NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(count, Allocator.TempJob);
        QueryParameters query = new QueryParameters(Physics.DefaultRaycastLayers, false, QueryTriggerInteraction.Ignore, false);
        for (int i = 0; i < count; i++)
        {
            Vector2 at = Middle(first + i);
            asks[i] = new RaycastCommand(new Vector3(at.x, Base + HeightAt(at) + SurvivalBody.ShelterLift, at.y), Vector3.up, query, SurvivalBody.ShelterReach);
        }
        RaycastCommand.ScheduleBatch(asks, hits, 64, 1).Complete();
        for (int i = 0; i < count; i++)
        {
            int id = hits[i].colliderInstanceID;
            roofs[first + i] = id == 0 ? (byte)0 : Owned(id, hits[i]) ? (byte)2 : (byte)1;
        }
        asks.Dispose();
        hits.Dispose();
        roofsDirty = true;
    }
}
