using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ObjectManager
{
    public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<Projectile> Projectiles { get; } = new HashSet<Projectile>();
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
    public Transform ProjectileRoot { get { return GetRootTransform("@Projectiles"); } }
    public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }
    #endregion

    public void ShowDamageFont(Vector2 position, float damage, Transform parent, bool isCritical = false)
    {
        GameObject go = Managers.Resource.Instantiate("DamageFont", pooling: true);
        DamageFont damageText = go.GetComponent<DamageFont>();
        damageText.SetInfo(position, damage, parent, isCritical);
    }

    public T Spawn<T>(Vector3 position, int templateID) where T : BaseObject
    {
        string prefabName = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(prefabName);
        go.name = prefabName;
        go.transform.position = position;

        BaseObject obj = go.GetComponent<BaseObject>();
        
        if(EObjectType.Creature == obj.ObjectType)
        {
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
            obj.transform.parent = ProjectileRoot;

            Projectile projectile = go.GetComponent<Projectile>();
            Projectiles.Add(projectile);

            projectile.SetInfo(templateID);
        }
        else if (EObjectType.Env == obj.ObjectType)
        {
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
            Projectile projectile = obj as Projectile;
            Projectiles.Remove(projectile);
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

    #region Skill 판정
    public List<Creature> FindConeRangeTargets(Creature owner, Vector3 dir, float range, int angleRange, bool isAllies = false)
    {
        List<Creature> targets = new List<Creature>();
        List<Creature> ret = new List<Creature>();

        ECreatureType targetType = Util.DetermineTargetType(owner.CreatureType, isAllies);

        if(ECreatureType.Monster == targetType)
        {
            var objs = Managers.Map.GatherObjects<Monster>(owner.transform.position, range, range);
            targets.AddRange(objs);
        }
        else if(ECreatureType.Hero == targetType)
        {
            var objs = Managers.Map.GatherObjects<Hero>(owner.transform.position, range, range);
            targets.AddRange(objs);
        }

        foreach(var target in targets)
        {
            // 1. 거리 안에 있는지 확인
            var targetPos = target.transform.position;
            float distance = Vector3.Distance(targetPos, owner.transform.position);
            if (range < distance) { continue; }

            // 2. 각도 확인
            if(360 != angleRange)
            {
                BaseObject ownerTarget = (owner as Creature).Target;

                // 2. 부채꼴 모양 각도 계산
                float dot = Vector3.Dot((targetPos - owner.transform.position).normalized, dir.normalized);
                float degree = Mathf.Rad2Deg * Mathf.Acos(dot);

                if (angleRange / 2f < degree) { continue; }
            }

            ret.Add(target);
        }

        return ret;
    }
    #endregion
}
