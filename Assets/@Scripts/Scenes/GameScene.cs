using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        SceneType = Define.EScene.GameScene;

        GameObject map = Managers.Resource.Instantiate("BaseMap");
        map.transform.position = Vector3.zero;
        map.name = "@BaseMap";

        HeroCamp camp = Managers.Object.Spawn<HeroCamp>(new Vector3Int(-10, -5, 0), 0);

        for(int i = 0; i < 5; ++i)
        {
            Managers.Object.Spawn<Hero>(new Vector3Int(-10 + Random.Range(-5, 5), -5 + Random.Range(-5, 5), 0), HERO_KNIGHT_ID);
        }

        CameraController camera = Camera.main.gameObject.GetOrAddComponent<CameraController>();
        camera.Target = camp;

        Managers.UI.ShowBaseUI<UI_Joystick>();

        {
            Managers.Object.Spawn<Monster>(new Vector3Int(0, 1, 0), MONSTER_BEAR_ID);
            //Managers.Object.Spawn<Monster>(new Vector3Int(1, 1, 0), Define.MONSTER_SLIME_ID);
        }

        {
            Env env = Managers.Object.Spawn<Env>(new Vector3(0, 2, 0), ENV_TREE1_ID);
            env.EnvState = EEnvState.Idle;
        }



        return true;
    }

    public override void Clear()
    {

    }
}
