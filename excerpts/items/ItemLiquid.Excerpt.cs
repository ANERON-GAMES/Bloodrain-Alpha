// ==== Bloodrain, ANERON GAMES. An excerpt for COMP305 Game Programming I, Part 3-A (alpha). ====
// ==== From ItemLiquid.cs in the private Unity 6 project, shortened: it shows how a liquid's surface stays level and sloshes. ====
// ==== It does not compile on its own; the rest of the class and the project stay private. ====
using UnityEngine;

// ==== ItemLiquid: the liquid inside an item's model. Its surface stays level at any tilt, leans when the item is pushed ====
// ==== or turned and settles again; parts that float ride the surface. An open vessel never shows liquid above its rim ====
public sealed partial class ItemLiquid : MonoBehaviour
{
    // ==== the surface is a damped pendulum: a change of the item's velocity pushes it (the liquid lags behind, a stop ====
    // ==== throws it forward), a turn drags it with the walls, gravity brings it back level; the floaters follow it ====
    private void LateUpdate()
    {
        float frame = Time.deltaTime;
        Vector3 centre = transform.TransformPoint(cavity.center);
        Quaternion turn = transform.rotation;
        if (!primed)
        {
            lastCentre = centre;
            lastTurn = turn;
            primed = true;
            Level();
            Carry(0f);
            return;
        }
        if (frame <= 0f)
        {
            return;
        }
        Vector3 velocity = (centre - lastCentre) / frame;
        float angle;
        Vector3 axis;
        (turn * Quaternion.Inverse(lastTurn)).ToAngleAxis(out angle, out axis);
        if (angle > 180f)
        {
            angle -= 360f;
        }
        bool moved = (centre - lastCentre).sqrMagnitude > 1e-10f || Mathf.Abs(angle) > 0.01f || lastVelocity.sqrMagnitude > 1e-8f;
        lastCentre = centre;
        lastTurn = turn;
        if (!moved && still)
        {
            return;
        }
        Vector3 change = velocity - lastVelocity;
        lastVelocity = velocity;
        swing += new Vector2(change.x, change.z) * (pulse * pulse / Gravity) * 0.6f;
        if (Mathf.Abs(angle) > 0.01f)
        {
            Vector3 rolled = axis * (angle * Mathf.Deg2Rad);
            swing += new Vector2(-rolled.z, rolled.x) * (Drag * pulse);
        }
        float span = Mathf.Min(frame, 0.1f);
        int count = Mathf.Max(1, Mathf.CeilToInt(span * 120f));
        float step = span / count;
        for (int i = 0; i < count; i++)
        {
            swing += (-pulse * pulse * tilt - 2f * damping * pulse * swing) * step;
            tilt += swing * step;
        }
        if (tilt.magnitude > reach)
        {
            Vector2 outward = tilt.normalized;
            tilt = outward * reach;
            swing -= outward * Mathf.Max(0f, Vector2.Dot(swing, outward));
        }
        bool settled = !moved && tilt.sqrMagnitude < Rest * Rest && swing.sqrMagnitude < Rest * Rest;
        if (settled)
        {
            tilt = Vector2.zero;
            swing = Vector2.zero;
        }
        Level();
        bool drifting = Carry(span);
        still = settled && !drifting;
    }
}
