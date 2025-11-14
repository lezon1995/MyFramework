using System.Text;

// 表示一个文件的信息
public class GameFileInfo
{
    public string name; // StreamingAssets下的相对路径
    public long size; // 文件大小
    public string md5; // 文件MD5

    public static GameFileInfo createInfo(string infoString)
    {
        string[] list = infoString.Split('\t');
        if (list is not { Length: 3 })
            return null;

        var info = new GameFileInfo
        {
            name = list[0],
            size = int.Parse(list[1]),
            md5 = list[2]
        };
        return info;
    }

    public void toString(StringBuilder builder)
    {
        builder.Append(name);
        builder.Append('\t');
        builder.Append(size);
        builder.Append('\t');
        builder.Append(md5);
    }
}