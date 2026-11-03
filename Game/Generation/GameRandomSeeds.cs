using System.Globalization;

namespace GameProject.Game.Generation;

internal static class GameRandomSeeds
{
    public const int DerivationVersion = 1;

    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    public static ulong ForBattle(
        ulong campaignSeed,
        string operationId,
        int battleNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(battleNumber);

        var scope =
            $"battle-v{DerivationVersion}/" +
            $"{operationId}/" +
            battleNumber.ToString(CultureInfo.InvariantCulture);

        return Derive(campaignSeed, scope);
    }

    public static ulong ForBattleGeneration(
        ulong campaignSeed,
        string operationId,
        int battleNumber)
    {
        return Derive(
            ForBattle(campaignSeed, operationId, battleNumber),
            "generation");
    }

    public static ulong ForBattleDeck(
        ulong campaignSeed,
        string operationId,
        int battleNumber)
    {
        return Derive(
            ForBattle(campaignSeed, operationId, battleNumber),
            "deck");
    }

    public static ulong ForBattleEvents(
        ulong campaignSeed,
        string operationId,
        int battleNumber)
    {
        return Derive(
            ForBattle(campaignSeed, operationId, battleNumber),
            "events");
    }

    private static ulong Derive(
        ulong parentSeed,
        string scope)
    {
        var hash = OffsetBasis;

        AddUInt64(ref hash, parentSeed);

        foreach (var character in scope)
        {
            AddByte(ref hash, (byte)character);
            AddByte(ref hash, (byte)(character >> 8));
        }

        AddByte(ref hash, 0xFF);

        return hash == 0
            ? 0x9E3779B97F4A7C15UL
            : hash;
    }

    private static void AddUInt64(
        ref ulong hash,
        ulong value)
    {
        for (var i = 0; i < 8; i++)
        {
            AddByte(ref hash, (byte)value);
            value >>= 8;
        }
    }

    private static void AddByte(
        ref ulong hash,
        byte value)
    {
        unchecked
        {
            hash ^= value;
            hash *= Prime;
        }
    }
}