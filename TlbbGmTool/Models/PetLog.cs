namespace liuguang.TlbbGmTool.Models;

public class PetLog
{
    /// <summary>
    /// ID bản ghi
    /// </summary>
    public int Id;
    /// <summary>
    /// Guid nhân vật
    /// </summary>
    public int CharGuid;
    /// <summary>
    /// Tên
    /// </summary>
    public string PetName = string.Empty;
    /// <summary>
    /// Cấp độ
    /// </summary>
    public int Level;
    /// <summary>
    /// Cấp độ yêu cầu
    /// </summary>
    public int NeedLevel;
    /// <summary>
    /// Tính cách
    /// </summary>
    public int AiType;
    /// <summary>
    /// Loại (Nội công, Ngoại công, Bình hành)
    /// </summary>
    public int PetType;
    /// <summary>
    /// Thọ mệnh
    /// </summary>
    public int Life;
    /// <summary>
    /// Genera
    /// </summary>
    public int Genera;
    /// <summary>
    /// Vui vẻ
    /// </summary>
    public int Enjoy;
    /// <summary>
    /// Ngộ tính
    /// </summary>
    public int Savvy;
    /// <summary>
    /// Căn cốt
    /// </summary>
    public int Gengu;
    /// <summary>
    /// Tỷ lệ trưởng thành
    /// </summary>
    public int GrowRate;
    /// <summary>
    /// Tiềm năng
    /// </summary>
    public int Repoint;
    /// <summary>
    /// Kinh nghiệm
    /// </summary>
    public int Exp;
    /// <summary>
    /// Cường lực
    /// </summary>
    public int Str;
    /// <summary>
    /// Nội lực
    /// </summary>
    public int Spr;
    /// <summary>
    /// Thể chất
    /// </summary>
    public int Con;
    /// <summary>
    /// Định lực
    /// </summary>
    public int Ipr;
    /// <summary>
    /// Thân pháp
    /// </summary>
    public int Dex;
    /// <summary>
    /// Tư chất cường lực
    /// </summary>
    public int StrPer;
    /// <summary>
    /// Tư chất nội lực
    /// </summary>
    public int SprPer;
    /// <summary>
    /// Tư chất thể chất
    /// </summary>
    public int ConPer;
    /// <summary>
    /// Tư chất định lực
    /// </summary>
    public int IprPer;
    /// <summary>
    /// Tư chất thân pháp
    /// </summary>
    public int DexPer;
    /// <summary>
    /// Kỹ năng
    /// </summary>
    public string Skill = string.Empty;
}
