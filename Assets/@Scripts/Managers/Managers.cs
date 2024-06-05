using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers s_instance;
    private static Managers Instance { get { Init(); return s_instance; } }

    private static void Init()
    {
        if(null == s_instance)
        {
            GameObject go = GameObject.Find("@Managers");
            if(null == go)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);

            // √ ±‚»≠
            s_instance = go.GetComponent<Managers>();
        }
    }
}
