using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static StringUtility;
using static MathUtility;

// 自定已的StringBuilder,用于封装C#自己的StringBuilder,提高其效率
public class MyStringBuilder : ClassObject
{
    protected StringBuilder sb = new(128); // 内置的StringBuilder实例,初始默认128个字节的缓冲区,足以应对大部分的字符串拼接

    public override void resetProperty()
    {
        base.resetProperty();
        sb.Clear();
    }

    public MyStringBuilder clear()
    {
        sb.Clear();
        return this;
    }

    public bool endWith(char value)
    {
        return sb.Length > 0 && sb[^1] == value;
    }

    // 因为是从后往前找的,startIndex表示从后面的哪个下标开始查找
    public int lastIndexOf(char value, int startIndex = -1)
    {
        int length = sb.Length;
        if (startIndex < 0)
        {
            startIndex = length - 1;
        }
        else
        {
            clampMax(ref startIndex, length - 1);
        }

        for (int i = startIndex; i >= 0; --i)
        {
            if (sb[i] == value)
                return i;
        }

        return -1;
    }

    public int indexOf(char value, int startIndex = 0)
    {
        int length = sb.Length;
        for (int i = startIndex; i < length; ++i)
        {
            if (sb[i] == value)
                return i;
        }

        return -1;
    }

    // 命名方式是按照c++的std::endl
    public MyStringBuilder endl()
    {
        sb.Append('\n');
        return this;
    }

    public MyStringBuilder append(char value)
    {
        sb.Append(value);
        return this;
    }

    public MyStringBuilder append(byte value)
    {
        sb.Append(IToS(value));
        return this;
    }

    public MyStringBuilder append(bool value)
    {
        sb.Append(boolToString(value));
        return this;
    }

    public MyStringBuilder append(short value)
    {
        sb.Append(IToS(value));
        return this;
    }

    public MyStringBuilder append(ushort value)
    {
        sb.Append(IToS(value));
        return this;
    }

    public MyStringBuilder append(int value)
    {
        sb.Append(IToS(value));
        return this;
    }

    public MyStringBuilder append(uint value)
    {
        sb.Append(IToS(value));
        return this;
    }

    public MyStringBuilder append(float value, int precision = 4)
    {
        sb.Append(FToS(value, precision));
        return this;
    }

    public MyStringBuilder append(double value)
    {
        sb.Append(value);
        return this;
    }

    public MyStringBuilder append(long value)
    {
        sb.Append(LToS(value));
        return this;
    }

    public MyStringBuilder append(ulong value)
    {
        sb.Append(ULToS(value));
        return this;
    }

    public MyStringBuilder append(Vector2 value, int precision = 4)
    {
        sb.Append(StringUtility.V2ToS(value, precision));
        return this;
    }

    public MyStringBuilder append(Vector3 value, int precision = 4)
    {
        sb.Append(StringUtility.V3ToS(value, precision));
        return this;
    }

    public MyStringBuilder append(Color32 value)
    {
        sb.Append(value.ToString());
        return this;
    }

    public MyStringBuilder colorString(string color, int value)
    {
        return append("<color=#", color, ">", IToS(value), "</color>");
    }

    public MyStringBuilder colorString(string color, int value0, string str0, int value1)
    {
        return append("<color=#", color, ">", IToS(value0), str0, IToS(value1), "</color>");
    }

    public MyStringBuilder colorString(string color, string str0)
    {
        return append("<color=#", color, ">", str0, "</color>");
    }

    public MyStringBuilder colorString(string color, string str0, string str1)
    {
        return append("<color=#", color, ">", str0, str1, "</color>");
    }

    public MyStringBuilder colorString(string color, string str0, string str1, string str2)
    {
        return append("<color=#", color, ">", str0, str1, str2, "</color>");
    }

    public MyStringBuilder colorString(string color, string str0, string str1, string str2, string str3)
    {
        return append("<color=#", color, ">", str0, str1, str2, str3, "</color>");
    }

    public MyStringBuilder colorString(string color, string str0, string str1, string str2, string str3, string str4)
    {
        return append("<color=#", color, ">", str0, str1, str2, str3, str4, "</color>");
    }

    public MyStringBuilder appendLine(string value0)
    {
        return append(value0, "\r\n");
    }

    public MyStringBuilder appendLine(string value0, string value1)
    {
        return append(value0, value1, "\r\n");
    }

    public MyStringBuilder appendLine(string value0, string value1, string value2)
    {
        return append(value0, value1, value2, "\r\n");
    }

    public MyStringBuilder appendLine(string value0, string value1, string value2, string value3)
    {
        return append(value0, value1, value2, value3, "\r\n");
    }

    public MyStringBuilder appendLine(string value0, string value1, string value2, string value3, string value4)
    {
        return append(value0, value1, value2, value3, value4, "\r\n");
    }

    public MyStringBuilder appendLine(string value0, string value1, string value2, string value3, string value4, string value5)
    {
        return append(value0, value1, value2, value3, value4, value5, "\r\n");
    }

    public MyStringBuilder append(string value)
    {
        sb.Append(value);
        return this;
    }

    public MyStringBuilder appendRepeat(string value, int repeatCount)
    {
        for (int i = 0; i < repeatCount; ++i)
            sb.Append(value);

        return this;
    }

    public MyStringBuilder append(string str0, string str1)
    {
        if (str0 != null && str1 != null)
            checkCapacity(str0.Length + str1.Length);

        sb.Append(str0).Append(str1);
        return this;
    }

    public MyStringBuilder append(string str0, int value)
    {
        return append(str0, IToS(value));
    }

    public MyStringBuilder append(string str0, int value, string str1)
    {
        return append(str0, IToS(value), str1);
    }

    public MyStringBuilder append(string str0, float value, int precision = 4)
    {
        return append(str0, FToS(value, precision));
    }

    public MyStringBuilder append(string str0, float value, int precision, string str1)
    {
        return append(str0, FToS(value, precision), str1);
    }

    public MyStringBuilder append(string str0, float value, string str1)
    {
        return append(str0, FToS(value), str1);
    }

    public MyStringBuilder append(string str0, bool value)
    {
        return append(str0, boolToString(value));
    }

    public MyStringBuilder append(string str0, long value)
    {
        return append(str0, LToS(value));
    }

    public MyStringBuilder append(string str0, long value, string str1)
    {
        return append(str0, LToS(value), str1);
    }

    public MyStringBuilder append(string str0, ulong value)
    {
        return append(str0, ULToS(value));
    }

    public MyStringBuilder append(string str0, ulong value, string str1)
    {
        return append(str0, ULToS(value), str1);
    }

    public MyStringBuilder append(string str0, Vector2 value, int precision = 4)
    {
        return append(str0, StringUtility.V2ToS(value, precision));
    }

    public MyStringBuilder append(string str0, Vector3 value, int precision = 4)
    {
        return append(str0, StringUtility.V3ToS(value, precision));
    }

    public MyStringBuilder append(string str0, Color32 value)
    {
        return append(str0, value.ToString());
    }

    public MyStringBuilder append(string str0, Type value)
    {
        if (value != null)
            return append(str0, value.ToString());

        return append(str0);
    }

    public MyStringBuilder append(string str0, string str1, string str2)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + 1);
        sb.Append(str0).Append(str1).Append(str2);
        return this;
    }

    public MyStringBuilder append(string str0, string str1, string str2, string str3)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + 1);
        sb.Append(str0).Append(str1).Append(str2).Append(str3);
        return this;
    }

    public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + 1);
        sb.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4);
        return this;
    }

    public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + 1);
        sb.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5);
        return this;
    }

    public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + 1);
        sb.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6);
        return this;
    }

    public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + 1);
        sb.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7);
        return this;
    }

    public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + 1);
        sb.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8);
        return this;
    }

    public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + str9.Length + 1);
        sb.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8).Append(str9);
        return this;
    }

    public MyStringBuilder append(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length + str6.Length + str7.Length + str8.Length + str9.Length + str10.Length + 1);
        sb.Append(str0).Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7).Append(str8).Append(str9).Append(str10);
        return this;
    }

    public MyStringBuilder append(string value, int startIndex, int count)
    {
        sb.Append(value, startIndex, count);
        return this;
    }

    public MyStringBuilder insert(int index, string value)
    {
        sb.Insert(index, value);
        return this;
    }

    public MyStringBuilder insert(int index, char value)
    {
        sb.Insert(index, value);
        return this;
    }

    public MyStringBuilder insertFront(string str0, string str1)
    {
        checkCapacity(str0.Length + str1.Length + 1);
        sb.Insert(0, str1);
        sb.Insert(0, str0);
        return this;
    }

    public MyStringBuilder insertFront(string str0, string str1, string str2)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + 1);
        sb.Insert(0, str2);
        sb.Insert(0, str1);
        sb.Insert(0, str0);
        return this;
    }

    public MyStringBuilder insertFront(string str0, string str1, string str2, string str3)
    {
        checkCapacity(str0.Length + str1.Length + str2.Length + str3.Length + 1);
        sb.Insert(0, str3);
        sb.Insert(0, str2);
        sb.Insert(0, str1);
        sb.Insert(0, str0);
        return this;
    }

    public MyStringBuilder remove(int startIndex, int length = -1)
    {
        if (length < 0)
            length = sb.Length - startIndex;

        sb.Remove(startIndex, length);
        return this;
    }

    public MyStringBuilder replace(char oldString, char newString)
    {
        sb.Replace(oldString, newString);
        return this;
    }

    public MyStringBuilder replace(string oldString, string newString)
    {
        sb.Replace(oldString, newString);
        return this;
    }

    public void replace(int begin, int end, string reStr)
    {
        remove(begin, end - begin);
        if (!reStr.isEmpty())
            insert(begin, reStr);
    }

    public void replaceAll(string key, string newWords)
    {
        int startPos = 0;
        while (true)
        {
            int pos = findFirstSubstr(key, startPos);
            if (pos < 0)
                break;

            replace(pos, pos + key.Length, newWords);
            startPos = pos + newWords.length();
        }
    }

    public void replaceAll(char key, char newWords)
    {
        int len = sb.Length;
        for (int i = 0; i < len; ++i)
        {
            if (sb[i] == key)
                sb[i] = newWords;
        }
    }

    // returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
    public int findFirstSubstr(char pattern, int startPos = 0, bool sensitive = true)
    {
        if (!sensitive)
            pattern = toLower(pattern);

        int posFind = -1;
        int len = sb.Length;
        for (int i = startPos; i < len; ++i)
        {
            if ((sensitive && sb[i] == pattern) || (!sensitive && toLower(sb[i]) == pattern))
            {
                posFind = i;
                break;
            }
        }

        return posFind;
    }

    // returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
    public int findFirstSubstr(string pattern, int startPos = 0, bool returnEndIndex = false, bool sensitive = true)
    {
        if (sb.Length < pattern.Length)
            return -1;

        int posFind = -1;
        int subLen = pattern.Length;
        int len = sb.Length;
        for (int i = startPos; i < len; ++i)
        {
            if (len - i < subLen)
                continue;

            int j = 0;
            // 大小写敏感
            if (sensitive)
            {
                for (; j < subLen; ++j)
                {
                    if (i + j >= 0 && i + j < len && sb[i + j] != pattern[j])
                        break;
                }
            }
            // 大小写不敏感,则需要都转换为小写
            else
            {
                for (; j < subLen; ++j)
                {
                    if (i + j >= 0 && i + j < len && toLower(sb[i + j]) != toLower(pattern[j]))
                        break;
                }
            }

            if (j == subLen)
            {
                posFind = i;
                break;
            }
        }

        if (returnEndIndex && posFind >= 0)
        {
            posFind += subLen;
        }

        return posFind;
    }

    public void removeLast(char key)
    {
        int length = sb.Length;
        for (int i = 0; i < length; ++i)
        {
            if (sb[length - 1 - i] == key)
            {
                remove(length - 1 - i, 1);
                break;
            }
        }
    }

    public void removeLastComma()
    {
        removeLast(',');
    }

    // json
    public void jsonStartArray(string name = null, int preTableCount = 0, bool returnLine = false)
    {
        // 如果不是最外层的数组,则需要加上数组的名字
        if (!name.isEmpty())
        {
            appendRepeat("\t", preTableCount);
            append("\"", name, "\"", ":");
            if (returnLine)
                append("\r\n");
        }

        appendRepeat("\t", preTableCount);
        append("[");
        if (returnLine)
            append("\r\n");
    }

    public void jsonEndArray(int preTableCount = 0, bool returnLine = false)
    {
        if (endWith(','))
            remove(sb.Length - 1);

        appendRepeat("\t", preTableCount);
        append("],");
        if (returnLine)
            append("\r\n");
    }

    public void jsonStartStruct(string name = null, int preTableCount = 0, bool returnLine = false)
    {
        // 如果不是最外层的数组,则需要加上数组的名字
        if (!name.isEmpty())
        {
            appendRepeat("\t", preTableCount);
            append("\"", name, "\"", ":");
            if (returnLine)
                append("\r\n");
        }

        // 如果不是最外层且非数组元素的结构体,则需要加上结构体的名字
        appendRepeat("\t", preTableCount);
        append("{");
        if (returnLine)
            append("\r\n");
    }

    public void jsonEndStruct(bool keepComma = true, int preTableCount = 0, bool returnLine = false)
    {
        if (endWith(','))
            remove(sb.Length - 1);

        appendRepeat("\t", preTableCount);
        append("}");
        if (keepComma)
            append(",");

        if (returnLine)
            append("\r\n");
    }

    public void jsonAddPair(string name, string value, int preTableCount = 0, bool returnLine = false)
    {
        appendRepeat("\t", preTableCount);
        // 如果是数组中的元素则不需要名字
        if (!name.isEmpty())
            append("\"", name, "\": ");

        append("\"", value, "\",");
        if (returnLine)
            append("\r\n");
    }

    public void jsonAddObject(string name, string value, int preTableCount = 0, bool returnLine = false)
    {
        appendRepeat("\t", preTableCount);
        append("\"", name, "\": ", value, ",");
        if (returnLine)
            append("\r\n");
    }

    public void rightToLeft()
    {
        replace('\\', '/');
    }

    public void leftToRight()
    {
        replace('/', '\\');
    }

    public void V2IToS(Vector2Int value, int limitLength = 0)
    {
        append(IToS(value.x, limitLength), ",", IToS(value.y, limitLength));
    }

    public void V2ToS(Vector2 value, int precision = 4)
    {
        append(FToS(value.x, precision), ",", FToS(value.y, precision));
    }

    public void V3ToS(Vector3 value, int precision = 4)
    {
        append(FToS(value.x, precision), ",", FToS(value.y, precision), ",", FToS(value.z, precision));
    }

    public void byteToHEXString(byte value, bool upperOrLower = true)
    {
        char[] hexChar = upperOrLower ? hexUpperChar : hexLowerChar;
        // 等效于int high = value / 16;
        // 等效于int low = value % 16;
        int high = value >> 4;
        int low = value & 15;
        if (high < 10)
            append((char)('0' + high));
        else
            append(hexChar[high - 10]);

        if (low < 10)
            append((char)('0' + low));
        else
            append(hexChar[low - 10]);
    }

    public MyStringBuilder setColor(string color)
    {
        if (sb.Length == 0)
            return this;

        insertFront("<color=#", color, ">");
        append("</color>");
        return this;
    }

    public void addSprite(string spriteName, float width = 1.0f)
    {
        append("<quad width=").append(width).append(" sprite=").append(spriteName).append("/>");
    }

    public void line(string line, bool returnLine = true)
    {
        if (returnLine)
            append(line, "\r\n");
        else
            append(line);
    }

    public void appendValueString(string str)
    {
        append("\"", str, "\",");
    }

    public void appendValueVector2(Vector2 value)
    {
        V2ToS(value);
        append(',');
    }

    public void appendValueVector2Int(Vector2Int value)
    {
        V2IToS(value);
        append(',');
    }

    public void appendValueVector3(Vector3 value)
    {
        V3ToS(value);
        append(',');
    }

    public void appendValueInt(int value)
    {
        append(IToS(value), ",");
    }

    public void appendValueUInt(uint value)
    {
        append(IToS(value), ",");
    }

    public void appendValueFloat(float value)
    {
        append(FToS(value), ",");
    }

    public void appendValueFloats(List<float> floatArray)
    {
        appendValueString(FsToS(floatArray));
    }

    public void appendValueInts(List<int> intArray)
    {
        appendValueString(IsToS(intArray));
    }

    public void appendConditionString(string col, string str, string operate)
    {
        append(col, "=\"", str, "\"", operate);
    }

    public void appendConditionInt(string col, int value, string operate)
    {
        append(col, " = ", IToS(value), operate);
    }

    public void appendUpdateString(string col, string str)
    {
        append(col, " = \"", str, "\",");
    }

    public void appendUpdateInt(string col, int value)
    {
        append(col, " = ", IToS(value), ",");
    }

    public void appendUpdateInts(string col, List<int> intArray)
    {
        appendUpdateString(col, IsToS(intArray));
    }

    public void appendUpdateFloats(string col, List<float> floatArray)
    {
        appendUpdateString(col, FsToS(floatArray));
    }

    public override string ToString()
    {
        return sb.ToString();
    }

    public string toString(int startIndex, int length)
    {
        return sb.ToString(startIndex, length);
    }

    public char this[int index] // 根据下标获得字符
    {
        get => sb[index];
        set => sb[index] = value;
    }

    public int Length // 当前字符串长度
    {
        get => sb.Length;
        set => sb.Length = value;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void checkCapacity(int increaseSize)
    {
        sb.EnsureCapacity(sb.Length + increaseSize);
    }
};