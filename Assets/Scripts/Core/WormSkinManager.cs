using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// WormSkinManager — Worm ke skins manage karta hai
/// Colors, patterns, aur designs
/// </summary>
public class WormSkinManager : MonoBehaviour
{
    public static WormSkinManager Instance;

    [Header("Available Skins")]
    public WormSkin[] skins;

    // Currently selected skin index
    private int selectedSkinIndex = 0;

    [System.Serializable]
    public class WormSkin
    {
        public string skinName;
        public Color headColor;
        public Color bodyColor;
        public Color glowColor;
        public Sprite headSprite;    // Optional custom sprite
        public bool isUnlocked = true;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Default skins setup
        if (skins == null || skins.Length == 0)
        {
            skins = new WormSkin[]
            {
                new WormSkin { skinName = "Classic Green",   headColor = new Color(0.2f, 0.9f, 0.2f),   bodyColor = new Color(0.1f, 0.7f, 0.1f),   glowColor = new Color(0f, 1f, 0f, 0.5f) },
                new WormSkin { skinName = "Fire Red",        headColor = new Color(1f, 0.2f, 0.1f),     bodyColor = new Color(0.8f, 0.1f, 0f),     glowColor = new Color(1f, 0.3f, 0f, 0.5f) },
                new WormSkin { skinName = "Ocean Blue",      headColor = new Color(0.1f, 0.5f, 1f),     bodyColor = new Color(0f, 0.3f, 0.9f),     glowColor = new Color(0f, 0.5f, 1f, 0.5f) },
                new WormSkin { skinName = "Royal Purple",    headColor = new Color(0.7f, 0.1f, 1f),     bodyColor = new Color(0.5f, 0f, 0.8f),     glowColor = new Color(0.8f, 0f, 1f, 0.5f) },
                new WormSkin { skinName = "Golden",          headColor = new Color(1f, 0.85f, 0f),      bodyColor = new Color(0.9f, 0.7f, 0f),     glowColor = new Color(1f, 0.9f, 0f, 0.5f) },
                new WormSkin { skinName = "Neon Pink",       headColor = new Color(1f, 0.1f, 0.7f),     bodyColor = new Color(0.9f, 0f, 0.6f),     glowColor = new Color(1f, 0f, 0.8f, 0.5f) },
                new WormSkin { skinName = "Ice White",       headColor = new Color(0.9f, 0.95f, 1f),    bodyColor = new Color(0.7f, 0.85f, 1f),    glowColor = new Color(0.8f, 0.9f, 1f, 0.5f) },
                new WormSkin { skinName = "Dark Shadow",     headColor = new Color(0.15f, 0.15f, 0.2f), bodyColor = new Color(0.1f, 0.1f, 0.15f),  glowColor = new Color(0.4f, 0f, 0.8f, 0.5f) },
                new WormSkin { skinName = "Toxic Green",     headColor = new Color(0.5f, 1f, 0f),       bodyColor = new Color(0.3f, 0.8f, 0f),     glowColor = new Color(0.6f, 1f, 0f, 0.5f) },
                new WormSkin { skinName = "Rainbow",         headColor = new Color(1f, 0.3f, 0.3f),     bodyColor = new Color(0.3f, 0.3f, 1f),     glowColor = new Color(1f, 1f, 0f, 0.5f) },
            };
        }

        // PlayerPrefs se selected skin load karo
        selectedSkinIndex = PlayerPrefs.GetInt("SelectedSkin", 0);
    }

    /// <summary>
    /// Worm pe selected skin apply karo
    /// </summary>
    public void ApplySkin(WormBody wormBody, int skinIndex = -1)
    {
        if (skinIndex == -1) skinIndex = selectedSkinIndex;

        skinIndex = Mathf.Clamp(skinIndex, 0, skins.Length - 1);
        WormSkin skin = skins[skinIndex];

        if (wormBody != null)
            wormBody.SetColor(skin.bodyColor);

        // Head color
        SpriteRenderer headSR = wormBody?.GetComponent<SpriteRenderer>();
        if (headSR != null)
            headSR.color = skin.headColor;
    }

    /// <summary>
    /// Skin select karo aur save karo
    /// </summary>
    public void SelectSkin(int index)
    {
        if (index >= 0 && index < skins.Length && skins[index].isUnlocked)
        {
            selectedSkinIndex = index;
            PlayerPrefs.SetInt("SelectedSkin", selectedSkinIndex);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Currently selected skin
    /// </summary>
    public WormSkin GetSelectedSkin() => skins[selectedSkinIndex];

    /// <summary>
    /// Skin unlock karo
    /// </summary>
    public void UnlockSkin(int index)
    {
        if (index >= 0 && index < skins.Length)
            skins[index].isUnlocked = true;
    }
}
