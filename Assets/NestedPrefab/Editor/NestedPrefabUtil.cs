using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace NestedPrefabUtil
{
    [InitializeOnLoad]
    public class Startup
    {
        static Startup()
        {
            Debug.Log("OnSceneLoaded");
            PrefabUtility.prefabInstanceUpdated += OnPrefabModified;
            //EditorApplication.update += Update;
        }

        static bool _ignore;
        static void OnPrefabModified(GameObject go_)
        {
            if(_ignore)
            {
                return;
            }
            var nested = go_.GetComponent<NestedPrefab>();
            // process referenced prefabs
            if(null != nested)
            {
                foreach(var id in nested._idSet)
                {
                    var rPath = AssetDatabase.GUIDToAssetPath(id);
                    var rgo = AssetDatabase.LoadAssetAtPath(rPath, typeof(GameObject)) as GameObject;
                    Debug.Log("id is " + id + " path is " + rPath);
                    var instance = GameObject.Instantiate(rgo);
                    List<GameObject> _go2ReplaceList = new List<GameObject>();
                    LoopChilds(instance, (child) =>
                    {
                        Debug.Log("child name : " + child.name + " go name : " + go_.name);
                        Debug.Log(child.GetComponent<NestedPrefab>() == null);
                        Debug.Log(child.name == go_.name);
                        if (null != child.GetComponent<NestedPrefab>() && child.name == go_.name)
                        {
                            Debug.Log("AddReplace");
                            _go2ReplaceList.Add(child);
                        }
                    });
                    for(int i = 0; i < _go2ReplaceList.Count; i++)
                    {
                        var newGo = GameObject.Instantiate(go_);
                        var oldGo = _go2ReplaceList[i];
                        newGo.name = oldGo.name;
                        newGo.transform.parent = oldGo.transform.parent;
                        newGo.transform.position = oldGo.transform.position;
                        newGo.transform.localScale = oldGo.transform.localScale;
                        newGo.transform.rotation = oldGo.transform.rotation;
                        GameObject.DestroyImmediate(oldGo);
                    }
                    _ignore = true;
                    PrefabUtility.ReplacePrefab(instance, rgo);
                    _ignore = false;
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                    GameObject.DestroyImmediate(instance);
                }
                var prefab = PrefabUtility.GetPrefabParent(go_);
                var path4Id = AssetDatabase.GetAssetPath(prefab);
                nested._guid = AssetDatabase.AssetPathToGUID(path4Id);
                Debug.Log(nested.ToString());
            }

            //process nested prefabs in children
            var root = PrefabUtility.GetPrefabParent(go_);
            var path = AssetDatabase.GetAssetPath(root);
            Dictionary<GameObject, GameObject> replaceDic = new Dictionary<GameObject, GameObject>();
            LoopChilds(go_, (child) =>
            {
                if (go_ == child)
                {
                    return;
                }
                var childNested = child.GetComponent<NestedPrefab>();
                if (null != childNested)
                {
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    Debug.Log("path is " + path + " guid is " + guid);
                    childNested.AddRefrerencePrefab(guid);
                    var srcAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(childNested._guid), typeof(GameObject));
                    replaceDic[childNested.gameObject] = srcAsset as GameObject;
                }
            });
            foreach(var pair in replaceDic)
            {
                _ignore = true;
                PrefabUtility.ReplacePrefab(pair.Key, pair.Value);
                _ignore = false;
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets(); ;
        }

        static void LoopChilds(GameObject root_, Action<GameObject> process_)
        {
            if (null != process_)
            {
                process_(root_);
            }
            foreach (Transform child in root_.transform)
            {
                LoopChilds(child.gameObject, process_);
            }
        }

        [MenuItem("Test/DebugNestPrefab")]
        static void DebugNestPrefab()
        {
            var go = Selection.activeGameObject;
            if(null == go)
            {
                return;
            }
            var nested = go.GetComponent<NestedPrefab>();
            if(null != nested)
            {
                Debug.Log(nested._guid);
                if (nested._idSet.Count < 1)
                {
                    Debug.Log("Empty ");
                }
                else
                {
                    foreach (var id in nested._idSet)
                    {
                        Debug.Log(id);
                    }
                }
            }
        }
        //static void Update()
        //{
        //}
    }
}
