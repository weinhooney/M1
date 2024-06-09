using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;
using static Define;

public class Hero : Creature
{
    bool _needArrange = true;
    public bool NeedArrange
    {
        get { return _needArrange; }
        set
        {
            _needArrange = value;

            if (value)
            {
                ChangeColliderSize(EColliderSize.Big);
            }
            else
            {
                TryResizeCollider();
            }
        }
    }


    EHeroMoveState _heroMoveState = EHeroMoveState.None;

    public EHeroMoveState HeroMoveState
    {
        get { return _heroMoveState; }
        set
        {
            _heroMoveState = value;
            switch(value)
            {
                case EHeroMoveState.CollectEnv:
                    NeedArrange = true;
                    break;

                case EHeroMoveState.TargetMonster:
                    NeedArrange = true;
                    break;

                case EHeroMoveState.ForceMove:
                    NeedArrange = true;
                    break;
            }
        }
    }

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        CreatureType = ECreatureType.Hero;

        Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        CreatureState = ECreatureState.Idle;

        // Skill
        skills = gameObject.GetOrAddComponent<SkillComponent>();
        skills.SetInfo(this, CreatureData.SkillIdList);
    }

    #region AI
    public Transform HeroCampDest
    {
        get
        {
            HeroCamp camp = Managers.Object.Camp;
            if (EHeroMoveState.ReturnToCamp == HeroMoveState)
            {
                return camp.Pivot;
            }

            return camp.Destination;
        }
    }

    protected override void UpdateIdle()
    {
        SetRigidBodyVelocity(Vector2.zero);

        // 0. 이동 상태라면 강제 변경
        if(EHeroMoveState.ForceMove == HeroMoveState)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        // 0. 너무 멀어졌다면 강제로 이동

        // 1. 몬스터
        Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
        if(null != creature)
        {
            Target = creature;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.TargetMonster;
            return;
        }

        // 2. 주변 Env 채굴
        Env env = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Envs) as Env;
        if (null != env)
        {
            Target = env;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.CollectEnv;
            return;
        }

        // 3. Camp 주변으로 모이기
        if(NeedArrange)
        {
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.ReturnToCamp;
            return;
        }
    }

    protected override void UpdateMove()
    {
        // 0. 누르고 있다면 강제 이동
        if(EHeroMoveState.ForceMove == HeroMoveState)
        {
            Vector3 dir = HeroCampDest.position - transform.position;
            SetRigidBodyVelocity(dir.normalized * MoveSpeed);
            return;
        }

        // 1. 주변 몬스터 서치
        if(EHeroMoveState.TargetMonster == HeroMoveState)
        {
            // 몬스터가 죽었으면 포기
            if(false == Target.IsValid())
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            SkillBase skill = skills.GetReadySkill();
            //ChaseOrAttackTarget(AttackDistance, HERO_SEARCH_DISTANCE);
            ChaseOrAttackTarget(HERO_SEARCH_DISTANCE, skill);
            return;
        }

        // 2. 주변 Env 채굴
        if (EHeroMoveState.CollectEnv == HeroMoveState)
        {
            // 몬스터가 있으면 포기
            Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
            if(null != creature)
            {
                Target = creature;
                HeroMoveState = EHeroMoveState.TargetMonster;
                CreatureState = ECreatureState.Move;
                return;
            }

            // Env 이미 채집했으면 포기
            if(false == Target.IsValid())
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            SkillBase skill = skills.GetReadySkill();
            ChaseOrAttackTarget(HERO_SEARCH_DISTANCE, skill);
            return;
        }

        // 3. Camp 주변으로 모이기
        if(EHeroMoveState.ReturnToCamp == HeroMoveState)
        {
            Vector3 dir = HeroCampDest.position - transform.position;
            float stopDistanceSqr = HERO_DEFAULT_STOP_RANGE * HERO_DEFAULT_STOP_RANGE;
            if(dir.sqrMagnitude <= stopDistanceSqr)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Idle;
                NeedArrange = false;
                return;
            }
            else
            {
                // 멀리 있을수록 빨라짐
                float ratio = Mathf.Min(1, dir.magnitude);
                float moveSpeed = MoveSpeed * (float)Math.Pow(ratio, 3);
                SetRigidBodyVelocity(dir.normalized * moveSpeed);
                return;
            }
        }

        // 4. 기타(누르다 뗐을 때)
        CreatureState = ECreatureState.Idle;
    }

    protected override void UpdateSkill()
    {
        SetRigidBodyVelocity(Vector2.zero);

        if(EHeroMoveState.ForceMove == HeroMoveState)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        if(false == Target.IsValid())
        {
            CreatureState = ECreatureState.Move;
            return;
        }
    }

    protected override void UpdateDead()
    {
        SetRigidBodyVelocity(Vector2.zero);
    }
    #endregion

    private void TryResizeCollider()
    {
        // 일단 충돌체를 아주 작게
        ChangeColliderSize(EColliderSize.Small);

        foreach(var hero in Managers.Object.Heroes)
        {
            if (EHeroMoveState.ReturnToCamp == hero.HeroMoveState) { return; }
        }

        // ReturnToCamp가 한 명도 없으면 Collider 조정
        foreach(var hero in Managers.Object.Heroes)
        {
            // 단 채집이나 전투중이면 스킵
            if(ECreatureState.Idle == hero.CreatureState)
            {
                hero.ChangeColliderSize(EColliderSize.Big);
            }
        }
    }

    private void HandleOnJoystickStateChanged(EJoystickState joystickState)
    {
        switch(joystickState)
        {
            case EJoystickState.PointerDown:
                HeroMoveState = EHeroMoveState.ForceMove;
                break;

            case EJoystickState.Drag:
                HeroMoveState = EHeroMoveState.ForceMove;
                break;

            case EJoystickState.PointerUp:
                HeroMoveState = EHeroMoveState.None;
                break;

            default:
                break;
        }
    }

    public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        base.OnAnimEventHandler(trackEntry, e);
    }
}
