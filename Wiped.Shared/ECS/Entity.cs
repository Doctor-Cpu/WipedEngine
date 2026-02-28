namespace Wiped.Shared.ECS;

public readonly record struct Entity<T>
	where T : Component?
{
	public readonly EntityUid Uid;
	public readonly T Comp;

	public Entity(EntityUid uid, T comp)
	{
		Uid = uid;
		Comp = comp;
	}

    public static implicit operator Entity<T>((EntityUid Uid, T Comp) tuple)
    {
        return new Entity<T>(tuple.Uid, tuple.Comp);
    }

    public static implicit operator Entity<T?>(EntityUid uid)
    {
        return new Entity<T?>(uid, default);
    }

    public readonly void Deconstruct(out EntityUid uid, out T comp)
    {
        uid = Uid;
        comp = Comp;
    }


    public override readonly int GetHashCode() => Uid.GetHashCode();
    public readonly Entity<T?> AsNullable() => new(Uid, Comp);
}

public readonly record struct Entity<T1, T2>
    where T1 : Component? 
    where T2 : Component?
{
    public readonly EntityUid Uid;
    public readonly T1 Comp1;
    public readonly T2 Comp2;

    public Entity(EntityUid uid, T1 comp1, T2 comp2)
    {
        Uid = uid;
        Comp1 = comp1;
        Comp2 = comp2;
    }

    public static implicit operator Entity<T1, T2>((EntityUid Uid, T1 Comp1, T2 Comp2) tuple)
        => new(tuple.Uid, tuple.Comp1, tuple.Comp2);

    public static implicit operator Entity<T1?, T2?>(EntityUid uid)
        => new(uid, default, default);

    public readonly void Deconstruct(out EntityUid uid, out T1 comp1, out T2 comp2)
    {
        uid = Uid;
        comp1 = Comp1;
        comp2 = Comp2;
    }

    public override readonly int GetHashCode() => Uid.GetHashCode();

    public readonly Entity<T1?, T2> AsNullableT1() => new(Uid, Comp1, Comp2);
    public readonly Entity<T1, T2?> AsNullableT2() => new(Uid, Comp1, Comp2);
    public readonly Entity<T1?, T2?> AsNullableAll() => new(Uid, Comp1, Comp2);
}

public readonly record struct Entity<T1, T2, T3>
    where T1 : Component? 
    where T2 : Component? 
    where T3 : Component?
{
    public readonly EntityUid Uid;
    public readonly T1 Comp1;
    public readonly T2 Comp2;
    public readonly T3 Comp3;

    public Entity(EntityUid uid, T1 comp1, T2 comp2, T3 comp3)
    {
        Uid = uid;
        Comp1 = comp1;
        Comp2 = comp2;
        Comp3 = comp3;
    }

    public static implicit operator Entity<T1, T2, T3>((EntityUid Uid, T1 Comp1, T2 Comp2, T3 Comp3) tuple)
        => new(tuple.Uid, tuple.Comp1, tuple.Comp2, tuple.Comp3);

    public static implicit operator Entity<T1?, T2?, T3?>(EntityUid uid)
        => new(uid, default, default, default);

    public readonly void Deconstruct(out EntityUid uid, out T1 comp1, out T2 comp2, out T3 comp3)
    {
        uid = Uid;
        comp1 = Comp1;
        comp2 = Comp2;
        comp3 = Comp3;
    }

    public override readonly int GetHashCode() => Uid.GetHashCode();

    public readonly Entity<T1?, T2, T3> AsNullableT1() => new(Uid, Comp1, Comp2, Comp3);
    public readonly Entity<T1, T2?, T3> AsNullableT2() => new(Uid, Comp1, Comp2, Comp3);
    public readonly Entity<T1, T2, T3?> AsNullableT3() => new(Uid, Comp1, Comp2, Comp3);

    public readonly Entity<T1?, T2?, T3> AsNullableT1T2() => new(Uid, Comp1, Comp2, Comp3);
    public readonly Entity<T1?, T2, T3?> AsNullableT1T3() => new(Uid, Comp1, Comp2, Comp3);
    public readonly Entity<T1, T2?, T3?> AsNullableT2T3() => new(Uid, Comp1, Comp2, Comp3);

    public readonly Entity<T1?, T2?, T3?> AsNullableAll() => new(Uid, Comp1, Comp2, Comp3);
}

public readonly record struct Entity<T1, T2, T3, T4>
    where T1 : Component?
    where T2 : Component?
    where T3 : Component?
    where T4 : Component?
{
    public readonly EntityUid Uid;
    public readonly T1 Comp1;
    public readonly T2 Comp2;
    public readonly T3 Comp3;
    public readonly T4 Comp4;

    public Entity(EntityUid uid, T1 comp1, T2 comp2, T3 comp3, T4 comp4)
    {
        Uid = uid;
        Comp1 = comp1;
        Comp2 = comp2;
        Comp3 = comp3;
        Comp4 = comp4;
    }

    public static implicit operator Entity<T1, T2, T3, T4>((EntityUid Uid, T1 Comp1, T2 Comp2, T3 Comp3, T4 Comp4) tuple)
        => new(tuple.Uid, tuple.Comp1, tuple.Comp2, tuple.Comp3, tuple.Comp4);

    public static implicit operator Entity<T1?, T2?, T3?, T4?>(EntityUid uid) 
        => new(uid, default, default, default, default);

    public readonly void Deconstruct(out EntityUid uid, out T1 comp1, out T2 comp2, out T3 comp3, out T4 comp4)
    {
        uid = Uid;
        comp1 = Comp1;
        comp2 = Comp2;
        comp3 = Comp3;
        comp4 = Comp4;
    }

    public override readonly int GetHashCode() => Uid.GetHashCode();

    public readonly Entity<T1?, T2, T3, T4> AsNullableT1() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1, T2?, T3, T4> AsNullableT2() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1, T2, T3?, T4> AsNullableT3() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1, T2, T3, T4?> AsNullableT4() => new(Uid, Comp1, Comp2, Comp3, Comp4);

    public readonly Entity<T1?, T2?, T3, T4> AsNullableT1T2() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1?, T2, T3?, T4> AsNullableT1T3() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1?, T2, T3, T4?> AsNullableT1T4() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1, T2?, T3?, T4> AsNullableT2T3() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1, T2?, T3, T4?> AsNullableT2T4() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1, T2, T3?, T4?> AsNullableT3T4() => new(Uid, Comp1, Comp2, Comp3, Comp4);

    public readonly Entity<T1?, T2?, T3?, T4> AsNullableT1T2T3() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1?, T2?, T3, T4?> AsNullableT1T2T4() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1?, T2, T3?, T4?> AsNullableT1T3T4() => new(Uid, Comp1, Comp2, Comp3, Comp4);
    public readonly Entity<T1, T2?, T3?, T4?> AsNullableT2T3T4() => new(Uid, Comp1, Comp2, Comp3, Comp4);

    public readonly Entity<T1?, T2?, T3?, T4?> AsNullableAll() => new(Uid, Comp1, Comp2, Comp3, Comp4);
}

// "surely no one needs more than 4" - Gwen 2026
