using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Env : BaseObject
{
    private Data.EnvData _data;

    private EEnvState _envState = EEnvState.Idle;
    public EEnvState EnvState
    {
        get { return _envState; }
        set
        {
            _envState = value;
            UpdateAnimation();
        }
    }

    #region Stat
    public float Hp { get; set; }
    public float MaxHp { get; set; }
    #endregion

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        ObjectType = EObjectType.Env;

        return true;
    }

    public void SetInfo(int templateID)
    {
        DataTemplateID = templateID;
        _data = Managers.Data.EnvDic[templateID];

        // Stat
        Hp = _data.MaxHp;
        MaxHp = _data.MaxHp;

        // Spine
        string ranSpine = _data.SkeletonDataIds[Random.Range(0, _data.SkeletonDataIds.Count)];
        SetSpineAnimation(ranSpine, SortingLayers.ENV);
    }

    protected override void UpdateAnimation()
    {
        switch(EnvState)
        {
            case Define.EEnvState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;

            case Define.EEnvState.OnDamaged:
                PlayAnimation(0, AnimName.DAMAGED, false);
                break;

            case Define.EEnvState.Dead:
                PlayAnimation(0, AnimName.DEAD, false);
                break;

            default:
                break;
        }
    }

    #region Battle
    public override void OnDamaged(BaseObject attacker, SkillBase skill)
    {
        if (EEnvState.Dead == EnvState) { return; }

        base.OnDamaged(attacker, skill);

        float finalDamage = 1;
        EnvState = EEnvState.OnDamaged;

        Managers.Object.ShowDamageFont(CenterPosition, finalDamage, transform);

        Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp);
        if(Hp <= 0)
        {
            OnDead(attacker, skill);
        }
    }

    public override void OnDead(BaseObject attacker, SkillBase skill)
    {
        base.OnDead(attacker, skill);

        EnvState = EEnvState.Dead;

        // TODO : Drop Item

        Managers.Object.Despawn(this);
    }
    #endregion
}
