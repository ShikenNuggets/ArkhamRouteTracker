namespace ArkhamDisplay;

public class OriginsSave(string filePath, int id) : SaveParser(filePath, id)
{
    public override bool HasKey(Entry entry, int requiredMatches)
    {
        if ("Data Handler".Equals(entry.type))
        {
            return HasKey("SS_Enigma_EnigmaHQ_Visited", requiredMatches) && !HasKey(entry.id, requiredMatches) && !HasKey(entry.alternateID, requiredMatches);
        }
        else if ("DarkKnightFinish".Equals(entry.metadata))
        {
            return HasKeyCustomRegex(@"\b" + entry.id + "............" + Convert.ToChar(Convert.ToByte("0x1", 16)), requiredMatches);
        }

        return base.HasKey(entry, requiredMatches);
    }

    protected override string GetFile() =>
        Path.Combine(m_filePath, "SpSave_v2_" + m_id.ToString() + ".sgd");
}