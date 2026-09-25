using System.Diagnostics.CodeAnalysis;

namespace Gearsetter.GameData;

[SuppressMessage("Performance", "CA1028", Justification = "uint in Lumina")]
[SuppressMessage("Design", "CA1027", Justification = "Not Flags")]
public enum EEquipSlotCategory : uint
{
    None = 0,
    OneHandedMainHand = 1,
    Shield = 2,
    Head = 3,
    Body = 4,
    Hands = 5,
    Legs = 7,
    Feet = 8,
    Ears = 9,
    Neck = 10,
    Wrists = 11,
    Rings = 12,
    TwoHandedMainHand = 13,

    // 14 isn't used
    // anything beyond is very weird gear, taking up multiple inventory slots; irrelevant in anything beyond ARR
}
