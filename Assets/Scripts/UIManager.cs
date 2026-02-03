using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject customizationPanel;
    [SerializeField] private GameObject waitingPanel;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_Dropdown colorDropdown;
    [SerializeField] private TMP_Dropdown teamDropdown;

    public void OnClickStart()
    {
        NetworkSessionManager.Instance.SetPlayerName(nameField.text);
        NetworkSessionManager.Instance.SetColor(colorDropdown.value);
        NetworkSessionManager.Instance.SetTeam(teamDropdown.value);

        customizationPanel.SetActive(false);
        waitingPanel.SetActive(true);

        NetworkSessionManager.Instance.UI_StartGame();
    }
}