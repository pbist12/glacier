using UnityEngine;

public class PauseMenuPanel : UIPanelBase
{
    public void OnClickResume()
    {
        PauseFlowController.Instance.ResumeGame();
    }

    public void OnClickSettings()
    {
        PauseFlowController.Instance.OpenSettings();
    }

    public void OnClickQuitToTitle()
    {
        PauseFlowController.Instance.QuitToTitle(); // �ʿ� �� ����
    }
}
