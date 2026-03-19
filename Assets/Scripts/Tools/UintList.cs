using System;
using System.Collections.Generic;

namespace Tools
{
/// <summary>
/// Wrapper class to be able to serialize a list of uints inside another list.
/// </summary>
[Serializable]
public class UintList
{
    public List<uint> items = new();
}
}