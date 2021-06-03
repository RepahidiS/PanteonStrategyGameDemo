using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public UIProductions uiProductions;
    public UIInformation uiInformation;
    public UIBase uiExitBuildMode;
    public UIMessage uiMessage;
    public UISoldierCounter uiSoldierCounter;
    private Coroutine _coroutine;

    void Start()
    {
        instance = this;

        uiInformation.Hide();
        uiExitBuildMode.Hide();
        uiMessage.Hide();

        uiProductions.CreateElements();
        uiInformation.SetOnClickEvents();
        UpdateSoldierCounter();
    }

    public void UpdateSoldierCounter()
    {
        uiSoldierCounter.txtCounter.text = GameManager.instance.producibleManager.GetSoldierCounter();
    }

    public void ScrollEvent(float scrollDelta)
    {
        if (scrollDelta > 0.0f)
        {
            if (uiProductions.isPointerOnThisUI)
                uiProductions.ScrollDown(scrollDelta);
        }
        else
        {
            if (uiProductions.isPointerOnThisUI)
                uiProductions.ScrollUp(scrollDelta);
        }
    }

    public bool IsPointerOnUIs()
    {
        return uiProductions.isPointerOnThisUI || uiInformation.isPointerOnThisUI;
    }

    public void ShowMessage(string message)
    {
        uiMessage.txtMessage.text = message;
        uiMessage.Show();
        if(_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(HideMessage());
    }

    IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(uiMessage.hideMessageSeconds);
        uiMessage.Hide();
    }
}

[System.Serializable]
public class UIBase
{
    public GameObject goParent;

    public void Show()
    {
        goParent.SetActive(true);
    }

    public void Hide()
    {
        goParent.SetActive(false);
    }
}

[System.Serializable]
public class UIMessage : UIBase
{
    public TextMeshProUGUI txtMessage;
    public float hideMessageSeconds;
}

[System.Serializable]
public class UISoldierCounter : UIBase
{
    public TextMeshProUGUI txtCounter;
}