namespace liuguang.TlbbGmTool.Models;
public class PetSkillBase
{
    /// <summary>
    /// ID kỹ năng
    /// </summary>
    public readonly int Id;
    /// <summary>
    /// Loại kỹ năng
    /// </summary>
    public readonly int SkillType;
    /// <summary>
    /// Tên
    /// </summary>
    public readonly string Name;
    /// <summary>
    /// Mô tả
    /// </summary>
    public readonly string Description;

    public PetSkillBase(int id, int skillType, string name, string description)
    {
        Id = id;
        SkillType = skillType;
        Name = name;
        Description = description;
    }
}
