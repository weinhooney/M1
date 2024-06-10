using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public static class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if(null == component)
        {
            component = go.AddComponent<T>();
        }

        return component;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (null == transform) { return null; }

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (null == go) { return null; }

        if(false == recursive)
        {
            for(int i = 0; i < go.transform.childCount; ++i)
            {
                Transform transform = go.transform.GetChild(i);
                if(string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if(null != component)
                    {
                        return component;
                    }
                }
            }
        }
        else
        {
            foreach(T component in go.GetComponentsInChildren<T>())
            {
                if(string.IsNullOrEmpty(name) || component.name == name)
                {
                    return component;
                }
            }
        }

        return null;
    }

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static Color HexToColor(string color)
    {
        if (false == color.Contains("#"))
        {
            color = $"#{color}";
        }

        ColorUtility.TryParseHtmlString(color, out Color parsedColor);

        return parsedColor;
    }

    public static ECreatureType DetermineTargetType(ECreatureType ownerType, bool findAllies)
    {
        if(ECreatureType.Hero == ownerType)
        {
            return findAllies ? ECreatureType.Hero : ECreatureType.Monster;
        }
        else if(ECreatureType.Monster == ownerType)
        {
            return findAllies ? ECreatureType.Monster : ECreatureType.Hero;
        }

        return ECreatureType.None;
    }

    public static float GetEffectRadius(EEffectSize size)
    {
        switch(size)
        {
            case EEffectSize.CircleSmall:
                return EFFECT_SMALL_RADIUS;

            case EEffectSize.CircleNormal:
                return EFFECT_NORMAL_RADIUS;

            case EEffectSize.CircleBig:
                return EFFECT_BIG_RADIUS;

            case EEffectSize.ConeSmall:
                return EFFECT_SMALL_RADIUS * 2f;

            case EEffectSize.ConeNormal:
                return EFFECT_NORMAL_RADIUS * 2f;

            case EEffectSize.ConeBig:
                return EFFECT_BIG_RADIUS * 2f;

            default:
                return EFFECT_SMALL_RADIUS;
        }
    }
}
