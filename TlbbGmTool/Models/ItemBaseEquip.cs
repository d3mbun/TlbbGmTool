namespace liuguang.TlbbGmTool.Models;
public class ItemBaseEquip : ItemBase
{
    public readonly int EquipPoint;
    public readonly int BagCapacity;
    public readonly int MaterialCapacity;
    public readonly ushort EquipVisual;
    /// <summary>
    /// Độ bền tối đa
    /// </summary>
    public readonly byte MaxDurPoint;
    /// <summary>
    /// Cấu hình giá trị thuộc tính int[64]/null
    /// </summary>
    public readonly int[]? EquipAttrValues;

    public ItemBaseEquip(int id, int tClass, int tType, byte rulerId,
        string name, string shortTypeString, string description, byte level,
        int equipPoint, int bagCapacity, int materialCapacity, ushort equipVisual, byte maxDurPoint, int[]? equipAttrValues) : base(id, tClass, tType, rulerId, name, shortTypeString, description, level, 1)
    {
        EquipPoint = equipPoint;
        BagCapacity = bagCapacity;
        MaterialCapacity = materialCapacity;
        EquipVisual = equipVisual;
        MaxDurPoint = maxDurPoint;
        EquipAttrValues = equipAttrValues;
    }
}
