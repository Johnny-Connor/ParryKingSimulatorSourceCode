using System;

public readonly struct HitInformation : IEquatable<HitInformation>
{
    public enum HitDirection
    {
        Front,
        Back
    }

    public int hitDamage { get; }
    public HitDirection hitDirection { get; }
    
    public HitInformation(int hitDamage, HitDirection hitDirection)
    {
        this.hitDamage = hitDamage;
        this.hitDirection = hitDirection;
    }

    public override string ToString()
    {
        return "hitDamage: " + hitDamage + "| hitDirection: " + hitDirection;
    }

    public override bool Equals(object obj)
    {
        return obj is HitInformation hitInformation &&
                hitDamage == hitInformation.hitDamage &&
                hitDirection == hitInformation.hitDirection;
    }

    public bool Equals(HitInformation other)
    {
        return this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(hitDamage, hitDirection);
    }

    public static bool operator ==(HitInformation a, HitInformation b)
    {
        return a.hitDamage == b.hitDamage && a.hitDirection == b.hitDirection;
    }

    public static bool operator !=(HitInformation a, HitInformation b)
    {
        return !(a == b);
    }
}
