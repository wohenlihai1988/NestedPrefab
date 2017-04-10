using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class NestedPrefab : MonoBehaviour {
    [HideInInspector]
    [SerializeField]
    public List<string> _idSet = new List<string>();
    [HideInInspector]
    public string _guid;

    public void AddRefrerencePrefab(string uid_)
    {
        if (!_idSet.Contains(uid_))
        {
            _idSet.Add(uid_);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach(var id in _idSet)
        {
            sb.Append(id);
            sb.Append("-");
        }
        return sb.ToString();
    }
}
