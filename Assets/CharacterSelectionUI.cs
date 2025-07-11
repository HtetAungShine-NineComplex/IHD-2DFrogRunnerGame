using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionUI : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    //private RuntimeAnimatorController m_NewController;

    public void ChangeAnimatorController(RuntimeAnimatorController m_NewController)
    {
        playerAnimator.runtimeAnimatorController = m_NewController;
    }
}