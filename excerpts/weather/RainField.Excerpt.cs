// ==== Bloodrain, ANERON GAMES. An excerpt for COMP305 Game Programming I, Part 3-A (alpha). ====
// ==== From RainField.cs in the private Unity 6 project, shortened: it shows how the blood rain is computed at a point. ====
// ==== It does not compile on its own; the rest of the class and the project stay private. ====
using System;
using UnityEngine;

// ==== RainField: the blood rain as weather. A pure function of the run's seed, the weather clock and a point. Showers, ====
// ==== storms (with a second cell on the right flank) and fronts are drawn from a hash of the seed and carried by a ====
// ==== wandering wind, so every machine builds the same sky from the same seed ====
public sealed partial class RainField
{
    // ==== where the wind comes from at t, degrees (215 to 290 plus a slow and a fast swing) ====
    public double WindFrom(double t)
    {
        return windFrom + SwingSlow * Math.Sin(2.0 * Math.PI * t / PeriodSlow + phaseSlow) + SwingFast * Math.Sin(2.0 * Math.PI * t / PeriodFast + phaseFast);
    }

    // ==== the rain at a point (x, z units) at t, 0 to 1: the cells together, broken by a noise carried with the wind ====
    public float At(Vector2 at, double t)
    {
        Prepare(t);
        double x = at.x;
        double z = at.y;
        double dry = 1.0;
        for (int i = 0; i < alive.Count; i++)
        {
            dry *= 1.0 - Clamp01(Rain(alive[i], x, z));
        }
        double qx = (x - nowDriftX) / NoiseCell;
        double qz = (z - nowDriftZ) / NoiseCell;
        double n = 0.65 * Noise(qx + t / 900.0, qz, Seed) + 0.35 * Noise(qx * 2.3, qz * 2.3 - t / 700.0, unchecked(Seed + 7u));
        return (float)Clamp01((1.0 - dry) * (0.72 + 0.56 * n));
    }

    // ==== the life cycle: growing to 25 %, full to 70 %, fading (the core rains out first); a +-8 % pulse every 4 min ====
    private static void Grow(ref RainCell cell, double t)
    {
        double tau = (t - cell.Born) / cell.Life;
        double grow = Edge(0.0, GrowEnd, tau);
        double fade = 1.0 - Edge(FadeStart, 1.0, tau);
        double pulse = 1.0 + 0.08 * Math.Sin(2.0 * Math.PI * (t - cell.Born) / 240.0 + cell.Pulse);
        cell.Tau = tau;
        cell.ShieldNow = cell.Shield * grow * fade * pulse;
        cell.CoreNow = cell.Core * grow * (1.0 - Edge(FadeStart - 0.1, 0.9, tau)) * pulse;
        cell.Size = 0.45 + 0.55 * Edge(0.0, GrowEnd + 0.05, tau) + 0.25 * Edge(FadeStart, 1.0, tau);
    }

    // ==== one cell's rain at a point: an ellipse 1.2 R along its motion and 0.85 R across, flat inside a quarter; the ====
    // ==== heavy core 0.6 R x 0.5 R, 0.3 R ahead of the middle; a front: the heavy line ahead, the light shield behind ====
    private double Rain(RainCell cell, double x, double z)
    {
        if (cell.ShieldNow <= 0.0 && cell.CoreNow <= 0.0)
        {
            return 0.0;
        }
        double rx = x - cell.CentreX;
        double rz = z - cell.CentreZ;
        if (cell.Kind == RainKind.Front)
        {
            double q = -(rx * cell.AheadX + rz * cell.AheadZ);
            double side = rx * cell.AheadZ + rz * -cell.AheadX;
            double gaps = Edge(0.32, 0.62, Noise(side / FrontGaps, cell.Noise * 0.001, unchecked(Seed + (uint)cell.Noise)));
            double lead = q >= 0.0 && q < FrontLead ? Math.Pow(Math.Sin(Math.PI * Clamp01(q / FrontLead)), 0.7) : 0.0;
            double trail = q >= 0.0 ? Math.Pow(Clamp01(1.0 - q / FrontTrail), 1.5) * Edge(0.0, 24.0, q) : 0.0;
            return cell.CoreNow * lead * gaps + cell.ShieldNow * trail * (0.55 + 0.45 * gaps);
        }
        double r = cell.Radius * cell.Size;
        if (rx * rx + rz * rz >= 1.44 * r * r)
        {
            return 0.0;
        }
        double speed = Math.Sqrt(cell.HeadingX * cell.HeadingX + cell.HeadingZ * cell.HeadingZ) + 1e-9;
        double ax = cell.HeadingX / speed;
        double az = cell.HeadingZ / speed;
        double along = rx * ax + rz * az;
        double across = rx * az + rz * -ax;
        double a = along / (1.2 * r);
        double b = across / (0.85 * r);
        double rain = cell.ShieldNow * Edge(1.0, 0.25, Math.Sqrt(a * a + b * b));
        if (cell.CoreNow > 0.0)
        {
            double c = (along - 0.3 * r) / (CoreAlong * r);
            double d = across / (CoreAcross * r);
            rain += cell.CoreNow * Edge(1.0, CoreFlat, Math.Sqrt(c * c + d * d));
        }
        return rain;
    }

    // ==== the wind: where it blows at t (units a second), and how far it has carried everything since 0 (5 s steps) ====
    private void Wind(double t, out double x, out double z)
    {
        double to = (WindFrom(t) + 180.0) * (Math.PI / 180.0);
        double speed = WindSpeed * (1 + SpeedSwing * Math.Sin(2.0 * Math.PI * t / PeriodSpeed + phaseSpeed));
        x = Math.Sin(to) * speed;
        z = Math.Cos(to) * speed;
    }

    private static double Edge(double from, double to, double x)
    {
        double t = Clamp01((x - from) / (to - from));
        return t * t * (3.0 - 2.0 * t);
    }
}
