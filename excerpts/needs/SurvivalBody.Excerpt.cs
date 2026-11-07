// ==== Bloodrain, ANERON GAMES. An excerpt for COMP305 Game Programming I, Part 3-A (alpha). ====
// ==== From SurvivalBody.cs in the private Unity 6 project, shortened: it shows the needs, the roof test and the blood ====
// ==== rain gauge. It does not compile on its own; the rest of the class and the project stay private. ====
using UnityEngine;

// ==== SurvivalBody: a human's needs - hunger, thirst, infection and the blood rain, which stacks in the open and falls ====
// ==== under a roof. Health and stamina belong to the body (one health, one WIND); the needs wear that health down ====
public partial class SurvivalBody : MonoBehaviour
{
    private const float StartHunger = 80f;
    private const float StartThirst = 80f;
    private const float StartInfection = 12f;
    public const float ShelterLift = 3.1f;
    public const float ShelterReach = 40f;

    public float Hunger = StartHunger;
    public float Thirst = StartThirst;
    public float Infection = StartInfection;
    public float Rain;
    public bool Escaped;

    public bool Sheltered { get; private set; }
    public float BloodFill { get; private set; }
    public float WaterFill { get; private set; }
    public OutskirtsMap.Cover Ground { get; private set; }

    private float hungerPerSecond = 100f / (40f * 60f);
    private float thirstPerSecond = 100f / (28f * 60f);
    private float rainOpenPerSecond = 100f / (4f * 60f);
    private float rainRecoverPerSecond = 100f / (3f * 60f);
    private static readonly float[] FillAt = { 0.05f, 0.2f, 0.5f, 0.85f, 1f };
    private static readonly float[] FillPerMinute = { 8f, 16.7f, 25f, 40f, 44f };

    private void Update()
    {
        float dt = Time.deltaTime;
        ReadGround();
        TickNeeds(dt);
    }

    // ==== under a roof: a ray straight up from a body's height meets something within reach (the map asks the same) ====
    private void ReadGround()
    {
        Sheltered = Physics.Raycast(transform.position + Vector3.up * ShelterLift, Vector3.up, ShelterReach, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private void TickNeeds(float dt)
    {
        BloodFill = 0f;
        WaterFill = 0f;
        if (Escaped)
        {
            return;
        }
        Hunger = Mathf.Max(0f, Hunger - hungerPerSecond * dt);
        Thirst = Mathf.Max(0f, Thirst - thirstPerSecond * dt);
        WeatherDirector sky = WeatherDirector.Instance;
        float share = sky != null ? sky.RainAt(transform.position) : 1f;
        float rain = Sheltered || share < FillAt[0] ? -rainRecoverPerSecond : Fill(share);
        if (!Sheltered && Ground == OutskirtsMap.Cover.Blood)
        {
            rain *= 1.8f;
        }
        BloodFill = Mathf.Max(0f, rain);
        WaterFill = Sheltered ? 0f : rainOpenPerSecond * WaterShare(sky);
        if (rain > 0f && wardrobe != null)
        {
            rain *= 1f - wardrobe.RainReduction;
        }
        Rain = Mathf.Clamp(Rain + rain * dt, 0f, 100f);
        float hurt = 0f;
        if (Rain >= 100f)
        {
            Infection = Mathf.Min(100f, Infection + (4f / 60f) * dt);
            hurt += (2f / 60f) * dt;
        }
        if (Hunger <= 0f)
        {
            hurt += 1.2f * dt;
        }
        if (Thirst <= 0f)
        {
            hurt += 1.8f * dt;
        }
        if (hurt > 0f && Human != null && Human.IsInitialized)
        {
            Human.Suffer(hurt, Thirst <= 0f ? "thirst" : Hunger <= 0f ? "hunger" : "the blood rain");
        }
    }
}
