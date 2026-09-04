using UnityEngine;

[CreateAssetMenu(fileName = "TMPNumberFontSettings", menuName = "TMPNumberFontSettings", order = 0)]
public class TMPNumberFontSettings : ScriptableObject
{
    public float globalScale = 3F;

    public GlyphData[] glyphsData = new GlyphData[12]
    {
        new(0, 0, 54, 22),
        new(1, -2, 53.7F, 16),
        new(2, -1, 54.5F, 23),
        new(3, 0, 54.5F, 23),
        new(4, -1, 54, 23),
        new(5, 0, 54, 23),
        new(6, 0, 54.5F, 23),
        new(7, 0, 54.5F, 22),
        new(8, 0, 54.5F, 23),
        new(9, 0, 54.5F, 22),
        new(10, 0, 53F, 26),
        new(11, 0, 55.5F, 26),
    };
}