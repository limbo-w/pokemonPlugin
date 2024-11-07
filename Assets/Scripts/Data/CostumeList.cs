using System.Collections.Generic;

[System.Serializable]
public class CostumeList
{
    public int total;
    public List<CostumeDetailInfo> rows;
    public int code;
    public string msg;
    public int pageNum;
}

[System.Serializable]
public class CostumeDetailInfo
{
    public int aHfId;
    public int aRId;
    public string aBfIds;
}

