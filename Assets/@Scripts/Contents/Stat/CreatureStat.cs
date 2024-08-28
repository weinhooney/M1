using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CreatureStat
{
    public float BaseValue { get; private set; } // 초기값

    private bool _isDirty = true; // 스탯을 재계산해야할지 여부

    [SerializeField] private float _value;

    public virtual float Value
    {
        get
        {
            if (_isDirty)
            {
                _value = CalculateFinalValue();
                _isDirty = false;
            }

            return _value;
        }
    }

    public List<StatModifier> StatModifiers = new List<StatModifier>();

    public CreatureStat()
    {
        
    }

    public CreatureStat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    public virtual void AddModifier(StatModifier modifier)
    {
        _isDirty = true;
        StatModifiers.Add(modifier);
    }

    public virtual bool RemoveModifier(StatModifier modifier)
    {
        if (StatModifiers.Remove(modifier))
        {
            _isDirty = true;
            return true;
        }

        return false;
    }

    public virtual bool ClearModifiersFromSource(object source)
    {
        int numRemovals = StatModifiers.RemoveAll(mod => mod.Source == source);

        if (0 < numRemovals)
        {
            _isDirty = true;
            return true;
        }

        return false;
    }

    private int CompareOrder(StatModifier a, StatModifier b)
    {
        if (a.Order == b.Order)
        {
            return 0;
        }

        return (a.Order < b.Order) ? -1 : 1;
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;
        
        StatModifiers.Sort(CompareOrder);

        for (int i = 0; i < StatModifiers.Count; ++i)
        {
            StatModifier modifier = StatModifiers[i];

            switch (modifier.Type)
            {
                case Define.EStatModType.Add:
                    finalValue += modifier.Value;
                    break;
                
                case Define.EStatModType.PercentAdd:
                    sumPercentAdd += modifier.Value;
                    if (i == StatModifiers.Count - 1 || StatModifiers[i + 1].Type != Define.EStatModType.PercentAdd)
                    {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                    break;
                
                case Define.EStatModType.PercentMult:
                    finalValue *= 1 + modifier.Value;
                    break;
            }
        }

        return (float)Math.Round(finalValue, 4);
    }
}
