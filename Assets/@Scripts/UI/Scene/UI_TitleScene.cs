using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TitleScene : UI_Scene
{
    enum GameObjects
    {
        StartImage,
    }

    enum Texts
    {
        DisplayText
    }

    public override bool Init()
    {
        if (false == base.Init()) { return false; }

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

        GetObject((int)GameObjects.StartImage).BindEvent((evt) => 
        {
            Debug.Log("Change Scene");
            Managers.Scene.LoadScene(Define.EScene.GameScene);
        });

        GetObject((int)GameObjects.StartImage).SetActive(false);
        GetText((int)Texts.DisplayText).text = $"";

        StartLoadAssets();

        return true;
    }

    private void StartLoadAssets()
    {
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {
                //Managers.Data.Init();

                GetObject((int)GameObjects.StartImage).SetActive(true);
                GetText((int)Texts.DisplayText).text = "Touch To Start";
            }
        });
    }
}
