using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class BaseObject : InitBase
{
    public EObjectType ObjectType { get; protected set; } = EObjectType.None;
    public CircleCollider2D Collider { get; private set; }
    public SkeletonAnimation SkeletonAnim { get; private set; }
    public Rigidbody2D RigidBody { get; private set; }

    public float ColliderRadius { get { return null != Collider ? Collider.radius : 0.0f; } }
    public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }

    public int DataTemplateID { get; set; }

    bool _lookLeft = true;
    public bool LookLeft
    {
        get { return _lookLeft; }
        set
        {
            _lookLeft = value;
            Flip(!value);
        }
    }

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        Collider = gameObject.GetOrAddComponent<CircleCollider2D>();
        SkeletonAnim = GetComponent<SkeletonAnimation>();
        RigidBody = GetComponent<Rigidbody2D>();

        return true;
    }

    public void LookAtTarget(BaseObject target)
    {
        Vector2 dir = target.transform.position - transform.position;
        if(dir.x < 0)
        {
            LookLeft = true;
        }
        else
        {
            LookLeft = false;
        }
    }

    #region Battle
    public virtual void OnDamaged(BaseObject attacker, SkillBase skill)
    {

    }

    public virtual void OnDead(BaseObject attacker, SkillBase skill)
    {

    }
    #endregion

    #region Spine
    protected virtual void SetSpineAnimation(string dataLabel, int sortingOrder)
    {
        if (null == SkeletonAnim) { return; }

        SkeletonAnim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(dataLabel);
        SkeletonAnim.Initialize(true);

        // Spine SkeletonAnimation은 SpriteRenderer를 사용하지 않고 MeshRenderer를 사용함
        // 그렇기 때문에 2D Sort Axis가 안 먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRederer를 같이 계산함
        SortingGroup sg = Util.GetOrAddComponent<SortingGroup>(gameObject);
        sg.sortingOrder = sortingOrder;
    }

    protected virtual void UpdateAnimation()
    {

    }

    public void SetRigidBodyVelocity(Vector2 velocity)
    {
        if (null == RigidBody) { return; }

        RigidBody.velocity = velocity;

        if(velocity.x < 0)
        {
            LookLeft = true;
        }
        else if(0 < velocity.x)
        {
            LookLeft = false;
        }
    }

    public void PlayAnimation(int trackIndex, string animName, bool loop)
    {
        if (null == SkeletonAnim) { return; }

        SkeletonAnim.AnimationState.SetAnimation(trackIndex, animName, loop);
    }

    public void AddAnimation(int trackIndex, string animName, bool loop, float delay)
    {
        if (null == SkeletonAnim) { return; }

        SkeletonAnim.AnimationState.AddAnimation(trackIndex, animName, loop, delay);
    }

    public void Flip(bool flag)
    {
        if (null == SkeletonAnim) { return; }

        SkeletonAnim.skeleton.ScaleX = flag ? -1 : 1;
    }

    public virtual void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        Debug.Log("OnAnimEventHandler");
    }
    #endregion
}
