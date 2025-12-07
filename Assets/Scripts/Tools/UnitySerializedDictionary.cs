using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
/// <summary>
/// <p>Unity does not support Dictionary serialization out of the box, but by
/// exploiting Unity's serialization protocol it can be done (See On Unitys Serialization Procotol:
/// https://odininspector.com/tutorials/serialize-anything/on-unitys-serialization-procotol#odin-serializer).
/// By making a new class that inherits both Dictionary
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
    [SerializeField, HideInInspector]
    private List<TKey> keyData = new List<TKey>();

    [SerializeField, HideInInspector]
    private List<TValue> valueData = new List<TValue>();

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        this.Clear();
        for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
        {
            this[this.keyData[i]] = this.valueData[i];
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        this.keyData.Clear();
        this.valueData.Clear();

        foreach (var item in this)
        {
            this.keyData.Add(item.Key);
            this.valueData.Add(item.Value);
        }
    }
}
}