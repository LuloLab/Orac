using System.Collections.Generic;
using UnityEngine;

public class Wrapper<T>
{
    public List<T> list;
    public static List<T> Parse(string json)
    {
        return JsonUtility.FromJson<Wrapper<T>>($"{{\"list\":{json}}}").list;
    }
}
