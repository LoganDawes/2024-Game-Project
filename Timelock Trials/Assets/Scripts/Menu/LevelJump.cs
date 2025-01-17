using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelJump : MonoBehaviour
{
    public void JumptoLevel2()
    {
        GameManager.Instance.LoadStaircaseScene("Level 2", "1", new Vector2(2, 0));
        GameManager.Instance.ResumeGame();
    }

    public void JumptoLevel3()
    {
        GameManager.Instance.LoadStaircaseScene("Level 3", "3", new Vector2(2, 0));
        GameManager.Instance.ResumeGame();
    }

    public void JumptoLevel1()
    {
        GameManager.Instance.JumpToScene("Level 1", new Vector3(0, 1.6f, 0));
        GameManager.Instance.ResumeGame();
    }

    public void JumptoTest()
    {
        GameManager.Instance.JumpToScene("TestMovement", new Vector3(0, 1.6f, 0));
        GameManager.Instance.ResumeGame();
    }
}
