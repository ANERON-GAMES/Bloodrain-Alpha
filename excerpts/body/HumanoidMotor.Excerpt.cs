// ==== Bloodrain, ANERON GAMES. An excerpt for COMP305 Game Programming I, Part 3-A (alpha). ====
// ==== From HumanoidStateMachine.cs and HumanoidMotor.cs in the private Unity 6 project, shortened: it shows the crouch ====
// ==== toggle and why the body stays down under a low roof. It does not compile on its own. ====
using UnityEngine;

public sealed partial class HumanoidStateMachine
{
    // ==== C pressed on firm ground toggles the crouch; standing up is refused when there is no room above ====
    private void Stance(in InputFrame input, int tick)
    {
        if (input.WasDown(HumanoidButtons.Crouch) && State == HumanoidState.Grounded)
        {
            bool wanted = !motor.Crouched;
            if (motor.TrySetCrouched(wanted))
            {
                events.Raise(HumanoidEventKind.CrouchChanged, tick, root.position, wanted ? 1f : 0f);
            }
        }
    }
}

public sealed partial class HumanoidMotor
{
    public bool TrySetCrouched(bool crouched)
    {
        if (crouched == Crouched)
        {
            return true;
        }
        if (!crouched && !HasHeadroom())
        {
            return false;
        }
        Crouched = crouched;
        ApplyCapsule(crouched ? profile.CrouchHeight : profile.Height);
        return true;
    }

    // ==== a sphere cast up from the top of the crouched capsule, as far as standing would rise ====
    private bool HasHeadroom()
    {
        Transform root = capsule.transform;
        float radius = capsule.radius * 0.95f;
        Vector3 origin = root.position + Vector3.up * (capsule.height - capsule.radius);
        float rise = (profile.Height - profile.CrouchHeight) * scale;
        int count = Physics.SphereCastNonAlloc(origin, radius, Vector3.up, probeHits, rise, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            if (probeHits[i].collider != capsule)
            {
                return false;
            }
        }
        return true;
    }
}
