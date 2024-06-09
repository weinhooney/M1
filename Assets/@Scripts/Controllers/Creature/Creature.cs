using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Creature : BaseObject
{
    public BaseObject Target { get; protected set; }
    public SkillComponent skills { get; protected set; }

    public Data.CreatureData CreatureData { get; private set; }
    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;

    #region Stats
    public float Hp { get; set; }
    public float MaxHp { get; set; }
    public float MaxHpBonusRate { get; set; }
    public float HealBonusRate { get; set; }
    public float HpRegen { get; set; }
    public float Atk { get; set; }
    public float AttackRate { get; set; }
    public float Def { get; set; }
    public float DefRate { get; set; }
    public float CriRate { get; set; }
    public float CriDamage { get; set; }
    public float DamageReduction { get; set; }
    public float MoveSpeedRate { get; set; }
    public float MoveSpeed { get; set; }
    #endregion

    protected ECreatureState _creatureState = ECreatureState.None;
    public virtual ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if(_creatureState != value)
            {
                _creatureState = value;
                UpdateAnimation();
            }
        }
    }

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        ObjectType = EObjectType.Creature;
        
        return true;
    }

    public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;

        if(ECreatureType.Hero == CreatureType)
        {
            CreatureData = Managers.Data.HeroDic[templateID];
        }
        else
        {
            CreatureData = Managers.Data.MonsterDic[templateID];
        }

        gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";

        // Collider
        Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
        Collider.radius = CreatureData.ColliderRadius;

        // RigidBody
        RigidBody.mass = CreatureData.Mass;

        // Spine
        SkeletonAnim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(CreatureData.SkeletonDataID);
        SkeletonAnim.Initialize(true);

        // Register AnimEvent
        if(null != SkeletonAnim.AnimationState)
        {
            SkeletonAnim.AnimationState.Event += OnAnimEventHandler;
        }

        // Spine SkeletonAnimation은 SpriteRenderer를 사용하지 않고 MeshRenderer를 사용함
        // 그렇기 때문에 2D Sort Axis가 안 먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRederer를 같이 계산함
        SortingGroup sg = Util.GetOrAddComponent<SortingGroup>(gameObject);
        sg.sortingOrder = SortingLayers.CREATURE;

        // Skills
        //CreatureData.SkillIdList

        // Stat
        MaxHp = CreatureData.MaxHp;
        Hp = CreatureData.MaxHp;
        Atk = CreatureData.Atk;
        MoveSpeed = CreatureData.MoveSpeed;

        // State
        CreatureState = ECreatureState.Idle;
    }

    protected override void UpdateAnimation()
    {
        switch(CreatureState)
        {
            case ECreatureState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;

            case ECreatureState.Skill:
                //PlayAnimation(0, AnimName.ATTACK_A, true);
                break;

            case ECreatureState.Move:
                PlayAnimation(0, AnimName.MOVE, true);
                break;

            case ECreatureState.Dead:
                PlayAnimation(0, AnimName.DEAD, true);
                RigidBody.simulated = false;
                break;

            default:
                break;
        }
    }

    public void ChangeColliderSize(EColliderSize size = EColliderSize.Normal)
    {
        switch (size)
        {
            case EColliderSize.Small:
                Collider.radius = CreatureData.ColliderRadius * 0.8f;
                break;

            case EColliderSize.Normal:
                Collider.radius = CreatureData.ColliderRadius;
                break;

            case EColliderSize.Big:
                Collider.radius = CreatureData.ColliderRadius * 1.2f;
                break;
        }
    }

    #region AI
    public float UpdateAITick { get; protected set; } = 0.0f;
    protected IEnumerator CoUpdateAI()
    {
        while(true)
        {
            switch (CreatureState)
            {
                case ECreatureState.Idle:
                    UpdateIdle();
                    break;

                case ECreatureState.Move:
                    UpdateMove();
                    break;

                case ECreatureState.Skill:
                    UpdateSkill();
                    break;

                case ECreatureState.Dead:
                    UpdateDead();
                    break;
            }

            if(0 < UpdateAITick)
            {
                yield return new WaitForSeconds(UpdateAITick);
            }
            else
            {
                yield return null;
            }
        }
    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }
    protected virtual void UpdateSkill() { }
    protected virtual void UpdateDead() { }
    #endregion

    #region Battle
    public override void OnDamaged(BaseObject attacker, SkillBase skill)
    {
        base.OnDamaged(attacker, skill);

        if (false == attacker.IsValid()) { return; }

        Creature creature = attacker as Creature;
        if (null == creature) { return; }

        float finalDamage = creature.Atk;
        Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp);

        if(Hp <= 0)
        {
            OnDead(attacker, skill);
            CreatureState = ECreatureState.Dead;
        }
    }

    public override void OnDead(BaseObject attacker, SkillBase skill)
    {
        base.OnDead(attacker, skill);
    }

    protected BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs, Func<BaseObject, bool> func = null)
    {
        BaseObject target = null;
        float bestDistanceSqr = float.MaxValue;
        float searchDistanceSqr = range * range;

        foreach (BaseObject obj in objs)
        {
            Vector3 dir = obj.transform.position - transform.position;
            float distToTargetSqr = dir.sqrMagnitude;

            // 서치 범위보다 멀리 있으면 스킵
            if (searchDistanceSqr < distToTargetSqr) { continue; }

            // 이미 더 좋은 후보를 찾았으면 스킵
            if (bestDistanceSqr < distToTargetSqr) { continue; }

            // 추가 조건
            if (null != func && false == func.Invoke(obj)) { continue; }

            target = obj;
            bestDistanceSqr = distToTargetSqr;
        }

        return target;
    }

    //protected void ChaseOrAttackTarget(float attackRange, float chaseRange)
    //{
    //    Vector3 dir = Target.transform.position - transform.position;
    //    float distToTargetSqr = dir.sqrMagnitude;
    //    float attackDistanceSqr = attackRange * attackRange;

    //    if (distToTargetSqr <= attackDistanceSqr)
    //    {
    //        // 공격 범위 이내로 들어왔다면 공격
    //        CreatureState = ECreatureState.Skill;
    //        return;
    //    }
    //    else
    //    {
    //        // 공격 범위 밖이라면 추적
    //        SetRigidBodyVelocity(dir.normalized * MoveSpeed);

    //        // 너무 멀어지면 포기
    //        float searchDistanceSqr = chaseRange * chaseRange;
    //        if (searchDistanceSqr < distToTargetSqr)
    //        {
    //            Target = null;
    //            CreatureState = ECreatureState.Move;
    //        }

    //        return;
    //    }
    //}
    protected void ChaseOrAttackTarget(float chaseRange, SkillBase skill)
    {
        Vector3 dir = Target.transform.position - transform.position;
        float distToTargetSqr = dir.sqrMagnitude;

        // TEMP
        float attackRange = HERO_DEFAULT_MELEE_ATTACK_RANGE;
        if(0 != skill.SkillData.ProjectileId)
        {
            attackRange = HERO_DEFAULT_RANGED_ATTACK_RANGE;
        }

        float finalAttackRange = attackRange + Target.ColliderRadius + ColliderRadius;
        float attackDistanceSqr = finalAttackRange * finalAttackRange;

        if(distToTargetSqr <= attackDistanceSqr)
        {
            // 공격 범위 이내로 들어왔다면 공격
            CreatureState = ECreatureState.Skill;
            skill.DoSkill();
            return;
        }
        else
        {
            // 공격 범위 밖이라면 추적
            SetRigidBodyVelocity(dir.normalized * MoveSpeed);

            // 너무 멀어지면 포기
            float searchDistanceSqr = chaseRange * chaseRange;
            if(searchDistanceSqr < distToTargetSqr)
            {
                Target = null;
                CreatureState = ECreatureState.Move;
            }

            return;
        }
    }


    #endregion

    #region Misc
    protected bool IsValid(BaseObject bo)
    {
        return bo.IsValid();
    }
    #endregion
}
