using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Projectile : BaseObject
{
    public Creature Owner { get; private set; }
    public SkillBase Skill { get; private set; }
    public Data.ProjectileData ProjectileData { get; private set; }
    public ProjectileMotionBase ProjectileMotion { get; private set; }

    private SpriteRenderer _spriteRenderer;

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        ObjectType = EObjectType.Projectile;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sortingOrder = SortingLayers.PROJECTILE;

        return true;
    }

    public void SetInfo(int dataTempplateID)
    {
        ProjectileData = Managers.Data.ProjectileDic[dataTempplateID];
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>(ProjectileData.ProjectileSpriteName);

        if(null == _spriteRenderer.sprite)
        {
            Debug.LogWarning($"Projectile Sprite Missing {ProjectileData.ProjectileSpriteName}");
            return;
        }
    }

    public void SetSpawnInfo(Creature owner, SkillBase skill, LayerMask layer)
    {
        Owner = owner;
        Skill = skill;

        Collider.excludeLayers = layer;

        if(null != ProjectileMotion)
        {
            Destroy(ProjectileMotion);
        }

        string componentName = ProjectileData.ComponentName;
        ProjectileMotion = gameObject.AddComponent(Type.GetType(componentName)) as ProjectileMotionBase;

        StraightMotion straightMotion = ProjectileMotion as StraightMotion;
        if(null != straightMotion)
        {
            straightMotion.SetInfo(ProjectileData.DataId, owner.CenterPosition, owner.Target.CenterPosition, () => { Managers.Object.Despawn(this); });
        }

        ParabolaMotion parabolaMotion = ProjectileMotion as ParabolaMotion;
        if(null != parabolaMotion)
        {
            parabolaMotion.SetInfo(ProjectileData.DataId, owner.CenterPosition, owner.Target.CenterPosition, () => { Managers.Object.Despawn(this); });
        }

        StartCoroutine(CoReserveDestroy(5.0f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseObject target = other.GetComponent<BaseObject>();
        if (false == target.IsValid()) { return; }

        target.OnDamaged(Owner, Skill);
        Managers.Object.Despawn(this);
    }

    private IEnumerator CoReserveDestroy(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Managers.Object.Despawn(this);
    }
}
