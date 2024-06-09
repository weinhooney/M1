using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class NormalAttack : SkillBase
{
    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        return true;
    }

    public override void SetInfo(Creature owner, int skillTemplateID)
    {
        base.SetInfo(owner, skillTemplateID);
    }

    public override void DoSkill()
    {
        base.DoSkill();

        Owner.CreatureState = Define.ECreatureState.Skill;
        Owner.PlayAnimation(0, SkillData.AnimName, false);

        Owner.LookAtTarget(Owner.Target);
    }

    protected override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        if(e.ToString().Contains(SkillData.AnimName))
        {
            OnAttackEvent();
        }
    }

    protected virtual void OnAttackEvent()
    {
        if (false == Owner.Target.IsValid()) { return; }

        if(0 == SkillData.ProjectileId)
        {
            // Melee
            Owner.Target.OnDamaged(Owner, this);
        }
        else
        {
            // Ranged
            GenerateProjectile(Owner, Owner.CenterPosition);
        }
    }

    protected override void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        if (false == Owner.Target.IsValid()) { return; }

        if(Define.ECreatureState.Skill == Owner.CreatureState)
        {
            Owner.CreatureState = Define.ECreatureState.Move;
        }
    }
}
