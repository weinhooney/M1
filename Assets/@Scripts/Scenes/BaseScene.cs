using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public abstract class BaseScene : InitBase
{
    public EScene SceneType { get; protected set; } = Define.EScene.Unknown;

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        UnityEngine.Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if(null == obj)
        {
            GameObject go = new GameObject { name = "@EventSystem" };
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        return true;
    }

    public abstract void Clear();
}
