using System;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using static StringUtility;
using static FrameBaseUtility;

public struct BoolScope : IDisposable
{
    bool flag;
    public BoolScope()
    {
        flag = true;
    }

    public void Dispose()
    {
        flag = false;
    }
}