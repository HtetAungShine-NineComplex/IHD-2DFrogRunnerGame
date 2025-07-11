using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeftPopupUI : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    [SerializeField] private GameObject m_UI;
    [SerializeField] private TMP_InputField nameInputField;
    //[SerializeField] private string defaultName = "Player";
    //private void Start()
    //{
    //    PlayerPrefs.DeleteAll();
    //}
    public void OnConfirmName()
    {
        string enteredName = nameInputField.text.Trim();

        //if (string.IsNullOrEmpty(enteredName))
        //{
        //    enteredName = defaultName;
        //}

        PlayerPrefs.SetString("PlayerName", enteredName);
        PlayerPrefs.Save();

        saveCharacterAvator();

        m_UI.SetActive(false);
    }

    private void saveCharacterAvator()
    {
        if(playerAnimator.runtimeAnimatorController.name == "Player_AC_Mask")
        {
            PlayerPrefs.SetInt("PlayerProfileIndex", 0);
            PlayerPrefs.Save();
        }
        else if (playerAnimator.runtimeAnimatorController.name == "Player_AC") //Frog Ninja
        {
            PlayerPrefs.SetInt("PlayerProfileIndex", 1);
            PlayerPrefs.Save();
        }
        else if (playerAnimator.runtimeAnimatorController.name == "Player_AC_Pink") //Frog Ninja
        {
            PlayerPrefs.SetInt("PlayerProfileIndex", 2);
            PlayerPrefs.Save();
        }
        else
        {
            PlayerPrefs.SetInt("PlayerProfileIndex", 3);
            PlayerPrefs.Save();
        }
    }
}
