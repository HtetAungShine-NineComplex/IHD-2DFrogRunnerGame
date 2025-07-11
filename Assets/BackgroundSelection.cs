using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSelection : MonoBehaviour
{
    [SerializeField] SpriteRenderer BGSpriteRenderer;
    //private RuntimeAnimatorController m_NewController;

    public void ChangeBackground(Sprite m_NewSprite)
    {
        BGSpriteRenderer.sprite = m_NewSprite;
    }
}
