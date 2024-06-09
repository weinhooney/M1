using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager
{
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, Object>();
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    #region Load Resource
    public T Load<T>(string key) where T : Object
    {
        if(_resources.TryGetValue(key, out Object resource))
        {
            return resource as T;
        }

        if(typeof(T) == typeof(Sprite) && false == key.Contains(".sprite"))
        {
            if(_resources.TryGetValue($"{key}.sprite", out resource))
            {
                return resource as T;
            }
        }

        return null;
    }

    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
    {
        GameObject prefab = Load<GameObject>(key);
        if(null == prefab)
        {
            Debug.LogError($"Failed to load prefab : {key}");
            return null;
        }

        if (pooling)
        {
            return Managers.Pool.Pop(prefab);
        }

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;

        return go;
    }

    public void Destroy(GameObject go)
    {
        if (null == go) { return; }
        if (Managers.Pool.Push(go)) { return; }

        Object.Destroy(go);
    }

    #endregion

    #region Addressables
    private void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
    {
        // Cache
        if(_resources.TryGetValue(key, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        string loadKey = key;
        if(key.Contains(".sprite"))
        {
            loadKey = $"{key}[{key.Replace(".sprite", "")}]";
        }

        var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
        asyncOperation.Completed += (op) => 
        {
            if (false == _resources.ContainsKey(key)) // 실제 에셋번들을 Load할 때 이상하게 Hero, AchievementIcon, Env가 중복 Load됨
            {
                _resources.Add(key, op.Result);
                _handles.Add(key, asyncOperation);
            }

            callback?.Invoke(op.Result);
        };
    }

    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : UnityEngine.Object
    {
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;
            int totalCount = op.Result.Count;

            foreach(var result in op.Result)
            {
                if(result.PrimaryKey.Contains(".sprite")) // sprite일 경우 일반적인 방법으로 로드하면 Texture2D로 인식하기 때문에 따로 처리
                {
                    LoadAsync<Sprite>(result.PrimaryKey, (obj) => 
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
                else
                {
                    LoadAsync<T>(result.PrimaryKey, (obj) => 
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
            }
        };
    }

    public void Clear()
    {
        _resources.Clear();

        foreach(var handle in _handles)
        {
            Addressables.Release(handle);
        }

        _handles.Clear();
    }
    #endregion
}
