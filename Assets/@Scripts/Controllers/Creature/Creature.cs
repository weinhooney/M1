using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Creature : BaseObject
{
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

        CreatureData = Managers.Data.CreatureDic[templateID];
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
                PlayAnimation(0, AnimName.ATTACK_A, true);
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
    public override void OnDamaged(BaseObject attacker)
    {
        base.OnDamaged(attacker);

        if (false == attacker.IsValid()) { return; }

        Creature creature = attacker as Creature;
        if (null == creature) { return; }

        float finalDamage = creature.Atk;
        Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp);

        if(Hp <= 0)
        {
            OnDead(attacker);
            CreatureState = ECreatureState.Dead;
        }
    }

    public override void OnDead(BaseObject attacker)
    {
        base.OnDead(attacker);
    }
    #endregion

    #region Wait
    protected Coroutine _coWait;

    protected void StartWait(float seconds)
    {
        CancelWait();
        _coWait = StartCoroutine(CoWait(seconds));
    }

    protected void CancelWait()
    {
        if(null != _coWait)
        {
            StopCoroutine(_coWait);
        }

        _coWait = null;
    }

    IEnumerator CoWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _coWait = null;
    }
    #endregion
}
