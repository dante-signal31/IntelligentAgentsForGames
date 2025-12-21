using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
/// <summary>
/// <p>Unity does not support Dictionary serialization out of the box, but by
/// exploiting Unity's serialization protocol, it can be done.</p>
/// <p>By making a new class that inherits both Dictionary
/// and Unity's ISerializationCallbackReceiver interface, we can convert the
/// Dictionary data to a format that Unity can serialize.</p>
/// <p>This class is abstract because Unity does not serialize generic types, it is
/// necessary to make a concrete Dictionary type by inheriting from the
/// UnitySerializedDictionary. See <see cref="CustomUnityDictionaries"/> for some
/// examples.</p>
/// <remarks>Implementation got from:
/// https://odininspector.com/tutorials/serialize-anything/serializing-dictionaries#odin-serializer</remarks>
/// </summary>
/// <typeparam name="TKey">Type for keys</typeparam>
/// <typeparam name="TValue">Type for values</typeparam>
public abstract class UnitySerializedDictionary<TKey, TValue> : 
    Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    // Don't remove [SerializeField] attribute or Unity will not serialize the dictionary.
    // I've tried it and it didn't work.
    [SerializeField, HideInInspector]
    private List<TKey> keyData = new();

    [SerializeField, HideInInspector]
    private List<TValue> valueData = new();

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        Clear();
        for (int i = 0; i < keyData.Count && i < valueData.Count; i++)
        {
            this[keyData[i]] = valueData[i];
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        keyData.Clear();
        valueData.Clear();

        foreach (var item in this)
        {
            keyData.Add(item.Key);
            valueData.Add(item.Value);
        }
    }
}
}