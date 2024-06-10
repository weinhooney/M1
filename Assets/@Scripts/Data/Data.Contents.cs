using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    #region CreatureData
    [System.Serializable]
    public class CreatureData
    {
        public int DataId;
        public string DescriptionTextID;
        public string PrefabLabel;
        public float ColliderOffsetX;
        public float ColliderOffsetY;
        public float ColliderRadius;
        public float Mass;
        public float MaxHp;
        public float MaxHpBonus;
        public float Atk;
        public float AtkRange;
        public float AtkBonus;
        public float Def;
        public float MoveSpeed;
        public float TotalExp;
        public float HpRate;
        public float AtkRate;
        public float DefRate;
        public float MoveSpeedRate;
        public string IconImage;
        public string SkeletonDataID;
        public string AnimatorName;
        public List<int> SkillIdList = new List<int>();
    }
    #endregion

    #region MonsterData
    [System.Serializable]
    public class MonsterData : CreatureData
    {
        public int DropItemId;
    }

    [System.Serializable]
    public class MonsterDataLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();

        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach(MonsterData monster in monsters)
            {
                dict.Add(monster.DataId, monster);
            }

            return dict;
        }
    }
    #endregion

    #region HeroData
    [System.Serializable]
    public class HeroData : CreatureData
    {

    }

    [System.Serializable]
    public class HeroDataLoader : ILoader<int, HeroData>
    {
        public List<HeroData> heroes = new List<HeroData>();

        public Dictionary<int, HeroData> MakeDict()
        {
            Dictionary<int, HeroData> dict = new Dictionary<int, HeroData>();
            foreach(HeroData hero in heroes)
            {
                dict.Add(hero.DataId, hero);
            }

            return dict;
        }
    }
    #endregion

    #region SkillData
    [System.Serializable]
    public class SkillData
    {
        public int DataId;
        public string Name;
        public string ClassName;
        public string ComponentName;
        public string Description;
        public int ProjectileId;
        public string PrefabLabel;
        public string IconLabel;
        public string AnimName;
        public float CoolTime;
        public float DamageMultiplier;
        public float Duration;
        public float NumProjectiles;
        public string CastingSound;
        public float AngleBetweenProj;
        public float SkillRange;
        public float RotateSpeed;
        public float ScaleMultiplier;
        public float AngleRange;
    }

    [System.Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skills = new List<SkillData>();

        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach(SkillData skill in skills)
            {
                dict.Add(skill.DataId, skill);
            }

            return dict;
        }
    }
    #endregion

    #region ProjectileData
    [System.Serializable]
    public class ProjectileData
    {
        public int DataId;
        public string Name;
        public string ComponentName;
        public string ProjectileSpriteName;
        public string PrefabLabel;
        public float duration;
        public float NumBounce;
        public float NumPenerations;
        public float HitSound;
        public float ProjRange;
        public float ProjSpeed;
    }

    [System.Serializable]
    public class ProjectileDataLoader : ILoader<int, ProjectileData>
    {
        public List<ProjectileData> projectiles = new List<ProjectileData>();

        public Dictionary<int, ProjectileData> MakeDict()
        {
            Dictionary<int, ProjectileData> dict = new Dictionary<int, ProjectileData>();
            foreach(ProjectileData projectile in projectiles)
            {
                dict.Add(projectile.DataId, projectile);
            }

            return dict;
        }
    }

    #endregion

    #region Env
    [System.Serializable]
    public class EnvData
    {
        public int DataId;
        public string DescriptionTextID;
        public string PrefabLabel;
        public float MaxHp;
        public int ResourceAmount;
        public float RegenTime;
        public List<string> SkeletonDataIds = new List<string>();
        public int DropItemId;
    }

    [System.Serializable]
    public class EnvDataLoader : ILoader<int, EnvData>
    {
        public List<EnvData> envs = new List<EnvData>();
        public Dictionary<int, EnvData> MakeDict()
        {
            Dictionary<int, EnvData> dict = new Dictionary<int, EnvData>();
            foreach(EnvData env in envs)
            {
                dict.Add(env.DataId, env);
            }

            return dict;
        }
    }
    #endregion
}
