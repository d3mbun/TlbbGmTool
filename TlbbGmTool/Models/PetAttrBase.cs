namespace liuguang.TlbbGmTool.Models;

public class PetAttrBase
{
    public int Id { get; }
    public string Name { get; }
    public int Level { get; }
    public int MaxLife { get; }
    public int StrPer { get; }
    public int ConPer { get; }
    public int SprPer { get; }
    public int DexPer { get; }
    public int IprPer { get; }

    public PetAttrBase(int id, string name, int level, int maxLife,
        int strPer, int conPer, int sprPer, int dexPer, int iprPer)
    {
        Id = id;
        Name = name;
        Level = level;
        MaxLife = maxLife;
        StrPer = strPer;
        ConPer = conPer;
        SprPer = sprPer;
        DexPer = dexPer;
        IprPer = iprPer;
    }
}
