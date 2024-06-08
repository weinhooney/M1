using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : InitBase
{
    private BaseObject _target;
    public BaseObject Target
    {
        get { return _target; }
        set { _target = value; }
    }

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        Camera.main.orthographicSize = 15.0f;

        return true;
    }

    private void LateUpdate()
    {
        if (null == Target) { return; }

        Vector3 targetPosition = new Vector3(Target.CenterPosition.x, Target.CenterPosition.y, -10f);
        transform.position = targetPosition;
    }
}
