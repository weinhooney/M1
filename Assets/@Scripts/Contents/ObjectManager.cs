using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ObjectManager
{
    public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<Env> Envs { get; } = new HashSet<Env>();
    public HeroCamp Camp { get; set; }

    #region Roots
    public Transform GetRootTransform(string name)
    {
        GameObject root = GameObject.Find(name);
        if(null == root)
        {
            root = new GameObject { name = name };
        }

        return root.transform;
    }

    public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
    public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
    public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }
    #endregion

    public T Spawn<T>(Vector3 position, int templateID) where T : BaseObject
    {
        string prefabName = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(prefabName);
        go.name = prefabName;
        go.transform.position = position;

        BaseObject obj = go.GetComponent<BaseObject>();
        
        if(EObjectType.Creature == obj.ObjectType)
        {
            // Data Check
            if(0 != templateID && false == Managers.Data.CreatureDic.TryGetValue(templateID, out Data.CreatureData data))
            {
                Debug.LogError($"ObjectManager Spawn Creature Failed! TryGetValue TemplateID: {templateID}");
                return null;
            }

            Creature creature = go.GetComponent<Creature>();
            switch(creature.CreatureType)
            {
                case ECreatureType.Hero:
                    obj.transform.parent = HeroRoot;
                    Hero hero = creature as Hero;
                    Heroes.Add(hero);
                    break;

                case ECreatureType.Monster:
                    obj.transform.parent = MonsterRoot;
                    Monster monster = creature as Monster;
                    Monsters.Add(monster);
                    break;
            }

            creature.SetInfo(templateID);
        }
        else if(EObjectType.Projectile == obj.ObjectType)
        {

        }
        else if (EObjectType.Env == obj.ObjectType)
        {
            // Data Check
            if(0 != templateID && false == Managers.Data.EnvDic.TryGetValue(templateID, out Data.EnvData data))
            {
                Debug.LogError($"ObjectManager Spawn Env Failed TryGetValue TemplateID: {templateID}");
                return null;
            }

            obj.transform.parent = EnvRoot;

            Env env = go.GetComponent<Env>();
            Envs.Add(env);

            env.SetInfo(templateID);
        }
        else if(EObjectType.HeroCamp == obj.ObjectType)
        {
            Camp = go.GetComponent<HeroCamp>();
        }

        return obj as T;
    }

    public void Despawn<T>(T obj) where T : BaseObject
    {
        EObjectType objectType = obj.ObjectType;
        if(EObjectType.Creature == objectType)
        {
            Creature creature = obj.GetComponent<Creature>();
            switch (creature.CreatureType)
            {
                case ECreatureType.Hero:
                    Hero hero = creature as Hero;
                    Heroes.Remove(hero);
                    break;

                case ECreatureType.Monster:
                    Monster monster = creature as Monster;
                    Monsters.Remove(monster);
                    break;
            }
        }
        else if(EObjectType.Projectile == objectType)
        {

        }
        else if (EObjectType.Env == objectType)
        {
            Env env = obj as Env;
            Envs.Remove(env);
        }
        else if(EObjectType.HeroCamp == objectType)
        {
            Camp = null;
        }

        Managers.Resource.Destroy(obj.gameObject);
    }
}
