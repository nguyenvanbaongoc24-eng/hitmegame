namespace HitMe.Server.Authority;

/// <summary>Trial roster and server-owned weapon catalog. Enabled trial characters are free to use.</summary>
public sealed class RoomLoadoutPolicy
{
    public const string DefaultCharacterId = "nam_lang";
    public const string DefaultWeaponId = "dep_to_ong";
    private static readonly HashSet<string> KnownCharacters = new(StringComparer.Ordinal)
    { "nam_lang", "nu_nang_dong", "quay_boy", "co_ba", "anh_teen", "cho_vang", "meo_mun" };
    private static readonly string[] DefaultEnabled =
    { "nam_lang", "nu_nang_dong", "co_ba", "anh_teen" };
    private readonly HashSet<string> enabled;
    private readonly ItemCatalog catalog;

    public RoomLoadoutPolicy(ItemCatalog catalog, IEnumerable<string>? enabledCharacters = null)
    {
        this.catalog = catalog;
        enabled = new HashSet<string>(enabledCharacters ?? DefaultEnabled, StringComparer.Ordinal);
        if (!enabled.Contains(DefaultCharacterId) || enabled.Any(id => !KnownCharacters.Contains(id)))
            throw new ArgumentException("Enabled roster must contain nam_lang and known character IDs.");
    }

    public static RoomLoadoutPolicy FromEnvironment(ItemCatalog catalog, string? value) =>
        new(catalog, string.IsNullOrWhiteSpace(value) ? null : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

    public bool TryResolve(string? characterId, string? weaponId, ProfileSnapshot? profile,
        out string selectedCharacter, out string selectedWeapon, out string error)
    {
        selectedCharacter = characterId ?? DefaultCharacterId;
        selectedWeapon = weaponId ?? DefaultWeaponId;
        if (!KnownCharacters.Contains(selectedCharacter)) { error = "character_not_found"; return false; }
        if (!enabled.Contains(selectedCharacter)) { error = "character_locked"; return false; }
        if (!catalog.TryGet(selectedWeapon, out var weapon) || weapon.Category != "weapon")
        { error = "weapon_not_found"; return false; }
        if (!weapon.IsStarter && profile?.PurchasedItemIds?.Contains(selectedWeapon, StringComparer.Ordinal) != true)
        { error = "weapon_not_owned"; return false; }
        error = string.Empty;
        return true;
    }
}
