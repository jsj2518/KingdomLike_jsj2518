using UnityEngine;

public class NPCDebug
{
    private static GUIStyle guiStyle;

    private static Texture2D CreateBackgroundTexture(Color color)
    {
        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    public static GUIStyle GuiStyle
    {
        get
        {
            if (guiStyle == null)
            {
                guiStyle = new GUIStyle();
                guiStyle.normal.textColor = Color.white; // 텍스트 색상
                guiStyle.alignment = TextAnchor.MiddleCenter; // 가운데 정렬
                guiStyle.normal.background = CreateBackgroundTexture(new Color(0, 0, 0, 0.5f)); // 반투명 검정 배경
            }
            return guiStyle;
        }
    }
}