using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Event = Spine.Event;

public abstract class SkillBase : InitBase
{
    public Creature Owner { get; protected set; }
    public float RemainCoolTime { get; set; }

    public Data.SkillData SkillData { get; private set; }

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        return true;
    }

    public virtual void SetInfo(Creature owner, int skillTemplateID)
    {
        Owner = owner;
        SkillData = Managers.Data.SkillDic[skillTemplateID];

        // Register AnimEvent
        if(null != Owner.SkeletonAnim && null != Owner.SkeletonAnim.AnimationState)
        {
            Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
            Owner.SkeletonAnim.AnimationState.Event += OnOwnerAnimEventHandler;
        }
    }

    private void OnDisable()
    {
        if (null == Managers.Game) { return; }
        if (false == Owner.IsValid()) { return; }
        if (null == Owner.SkeletonAnim) { return; }
        if (null == Owner.SkeletonAnim.AnimationState) { return; }

        Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
    }

    public virtual void DoSkill()
    {
        // 준비된 스킬에서 해제
        if(null != Owner.skills)
        {
            Owner.skills.ActiveSkills.Remove(this);
        }

        float timeScale = 1.0f;

        if(this == Owner.skills.DefaultSkill)
        {
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = timeScale;
        }
        else
        {
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = 1;
        }

        StartCoroutine(CoCountDownCooldown());
    }

    private IEnumerator CoCountDownCooldown()
    {
        RemainCoolTime = SkillData.CoolTime;
        yield return new WaitForSeconds(SkillData.CoolTime);

        RemainCoolTime = 0;

        // 준비된 스킬에 추가
        if(null != Owner.skills)
        {
            Owner.skills.ActiveSkills.Add(this);
        }
    }

    public virtual void CancelSkill()
    {

    }

    protected virtual void GenerateProjectile(Creature owner, Vector3 spawnPos)
    {
        Projectile projectile = Managers.Object.Spawn<Projectile>(spawnPos, SkillData.ProjectileId);

        LayerMask excludeMask = 0;
        excludeMask.AddLayer(Define.ELayer.Default);
        excludeMask.AddLayer(Define.ELayer.Projectile);
        excludeMask.AddLayer(Define.ELayer.Env);
        excludeMask.AddLayer(Define.ELayer.Obstacle);

        switch(owner.CreatureType)
        {
            case Define.ECreatureType.Hero:
                excludeMask.AddLayer(Define.ELayer.Hero);
                break;

            case Define.ECreatureType.Monster:
                excludeMask.AddLayer(Define.ELayer.Monster);
                break;
        }

        projectile.SetSpawnInfo(Owner, this, excludeMask);
    }

    private void OnOwnerAnimEventHandler(TrackEntry trackEntry, Event e)
    {
        // 다른 스킬의 애니메이션 이벤트도 받기 때문에 자기꺼만 써야 함
        if(trackEntry.Animation.Name == SkillData.AnimName)
        {
            OnAttackEvent();
        }
    }

    protected abstract void OnAttackEvent();
}
