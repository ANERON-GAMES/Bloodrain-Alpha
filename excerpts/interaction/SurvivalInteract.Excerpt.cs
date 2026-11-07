// ==== Bloodrain, ANERON GAMES. An excerpt for COMP305 Game Programming I, Part 3-A (alpha). ====
// ==== From SurvivalInteract.cs in the private Unity 6 project, shortened: it shows the eyes' ray: a tap inspects, a hold takes, a hold on a crate searches it. ====
// ==== It does not compile on its own; the rest of the class and the project stay private. ====
using UnityEngine;
using UnityEngine.InputSystem;

// ==== SurvivalInteract: the eye's ray and E: a short press on an item looks at it (INSPECT), E held takes it at once, ====
// ==== E held on a container searches it; the ray leaves the head's eyes in the first-person and the F3 view alike ====
public sealed partial class SurvivalInteract : MonoBehaviour
{
    private const float TakeHold = 0.35f;

    public string Prompt = "";
    public float Hold01;
    public LootContainer OpenContainer;

    private float held;
    private ItemPickup pickup;
    private ItemPickup pressed;
    private LootContainer container;
    private SurvivalHud hud;
    private ItemInspector inspector;
    private Humanoid human;

    private void Update()
    {
        hud = hud != null ? hud : GetComponent<SurvivalHud>();
        inspector = inspector != null ? inspector : GetComponent<ItemInspector>();
        Prompt = "";
        Hold01 = 0f;
        pickup = null;
        if (OpenContainer == null)
        {
            container = null;
        }
        if (OpenContainer != null && Vector3.Distance(transform.position, OpenContainer.transform.position) > 4f)
        {
            OpenContainer = null;
        }
        if ((hud != null && hud.IsPackOpen) || (inspector != null && inspector.IsOpen))
        {
            held = 0f;
            pressed = null;
            return;
        }
        human = human != null ? human : GetComponent<Humanoid>();
        HumanoidHeadCamera eyes = human != null ? human.HeadCamera : null;
        if (eyes != null && Physics.Raycast(eyes.Pose.position, eyes.Pose.forward, out RaycastHit hit, 4f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            pickup = hit.collider.GetComponentInParent<ItemPickup>();
            if (OpenContainer == null)
            {
                container = hit.collider.GetComponentInParent<LootContainer>();
            }
        }
        Keyboard keyboard = Keyboard.current;
        bool use = keyboard != null && keyboard.eKey.isPressed;
        bool down = keyboard != null && keyboard.eKey.wasPressedThisFrame;
        if (pickup != null)
        {
            ItemDef def = ItemCatalog.Get(pickup.ItemId);
            string name = def != null ? def.DisplayName : pickup.ItemId;
            Prompt = "E  INSPECT " + name.ToUpperInvariant() + "   HOLD TO TAKE";
            Press(pickup, down, use);
            return;
        }
        pressed = null;
        if (container != null && !container.Searched)
        {
            Prompt = "HOLD E  SEARCH " + container.Title;
            if (use)
            {
                held += Time.deltaTime;
                Hold01 = Mathf.Clamp01(held / container.HoldSeconds);
                if (held >= container.HoldSeconds)
                {
                    container.Searched = true;
                    OpenContainer = container;
                    held = 0f;
                    TintSearched(container);
                }
            }
            else
            {
                held = 0f;
            }
        }
        else
        {
            held = 0f;
        }
    }
}
