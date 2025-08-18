using System;

public class RoundEvents
{
    public static event Action RoundStart;
    public static event Action<byte> RoundEnd;

    public static void OnRoundEnd(byte obj) => RoundEnd?.Invoke(obj);
    public static void OnRoundStart() => RoundStart?.Invoke();
}