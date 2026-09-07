using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]   // Required for JsonUtility + Unity Serialization
public class ObservableList<T> : IList<T>
{
    [SerializeField]
    private List<T> _list = new();   // Unity serializes this normally

    public event Action Changed;

    public T this[int index]
    {
        get => _list[index];
        set { _list[index] = value; Changed?.Invoke(); }
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public void Add(T item)
    {
        _list.Add(item);
        Changed?.Invoke();
    }

    public void Clear()
    {
        _list.Clear();
        Changed?.Invoke();
    }

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) =>
        _list.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        Changed?.Invoke();
    }

    public bool Remove(T item)
    {
        bool result = _list.Remove(item);
        if (result) Changed?.Invoke();
        return result;
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
        Changed?.Invoke();
    }

    public List<T> ToList() => new List<T>(_list);
}
