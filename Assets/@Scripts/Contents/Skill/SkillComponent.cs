using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SkillComponent : InitBase
{
    public List<SkillBase> SkillList { get; } = new List<SkillBase>();
    public List<SkillBase> ActiveSkills { get; set; } = new List<SkillBase>();

    public SkillBase DefaultSkill { get; private set; }
    public SkillBase EnvSkill { get; private set; }
    public SkillBase ASkill { get; private set; }
    public SkillBase BSkill { get; private set; }

    public SkillBase CurrentSkill
    {
        get
        {
            if(0 == ActiveSkills.Count)
            {
                if(null == DefaultSkill)
                {
                    int temp = 0;
                }

                return DefaultSkill;
            }

            int randomIndex = UnityEngine.Random.Range(0, ActiveSkills.Count);
            if(null == ActiveSkills[randomIndex])
            {
                int temp = 0;
            }
            return ActiveSkills[randomIndex];
        }
    }

    Creature _owner;

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        return true;
    }

    public void SetInfo(Creature owner, Data.CreatureData creatureData)
    {
        _owner = owner;

        if(creatureData.DataId == 202006)
        {
            int temp = 0;
        }

        AddSkill(creatureData.DefaultSkillId, ESkillSlot.Default);
        AddSkill(creatureData.EnvSkillId, ESkillSlot.Env);
        AddSkill(creatureData.SkillAId, ESkillSlot.A);
        AddSkill(creatureData.SKillBId, ESkillSlot.B);
    }

    public void AddSkill(int skillTemplateID, ESkillSlot skillSlot)
    {
        if (0 == skillTemplateID) { return; }

        if(false == Managers.Data.SkillDic.TryGetValue(skillTemplateID, out var data))
        {
            Debug.LogWarning($"AddSkill Failed {skillTemplateID}");
            return;
        }

        SkillBase skill = gameObject.AddComponent(Type.GetType(data.ClassName)) as SkillBase;
        if (null == skill) { return; }

        skill.SetInfo(_owner, skillTemplateID);

        SkillList.Add(skill);

        switch (skillSlot)
        {
            case ESkillSlot.Default:
                DefaultSkill = skill;
                break;

            case ESkillSlot.Env:
                EnvSkill = skill;
                break;

            case ESkillSlot.A:
                ASkill = skill;
                ActiveSkills.Add(skill);
                break;

            case ESkillSlot.B:
                BSkill = skill;
                ActiveSkills.Add(skill);
                break;
        }
    }

    public SkillBase GetReadySkill()
    {
        // TEMP
        return SkillList[0];
    }
}
