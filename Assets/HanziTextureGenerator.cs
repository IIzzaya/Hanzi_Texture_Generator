using System;
using System.Collections.Generic;
using UnityEngine;

// REF:
// https://docs.unity3d.com/ScriptReference/CharacterInfo.html
// https://docs.unity3d.com/ScriptReference/Font.RequestCharactersInTexture.html
// https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
// http://blog.almostlogical.com/2010/08/20/adding-text-to-texture-at-runtime-in-unity3d-without-using-render-texture/
// https://www.youtube.com/watch?v=5meqgEg3VJM
// @IIzzaya
public class HanziTextureGenerator : MonoBehaviour {
    public Font font;
    public static Vector2 defaultMeshSize = new Vector2(512, 512);

    private Texture2D fontTexture {
        get {
            if (_fontTexture == null) _fontTexture = font.material.mainTexture as Texture2D;

            return _fontTexture;
        }
    }

    private Texture2D _fontTexture;
    public static HanziTextureGenerator instance;

    public List<OverrideRecipe> overrideRecipes = new List<OverrideRecipe>();

    private void Awake() {
        Debug.Assert(font);

        if (instance == null)
            instance = this;
        else if (instance != this) instance = this;
    }

    public static void CheckInstance() {
        if (instance == null) {
            instance = FindObjectOfType<HanziTextureGenerator>();
            if (instance == null) {
                Debug.LogError("There is no [Character Generator] in Scene");
                return;
            }
        }
        else if (instance.font == null) {
            Debug.LogError("There is no [Font] attached on [Character Generator]");
            return;
        }
    }


    public static Texture RequestCharacterTexture(char hanzi) {
        return RequestCharacterTexture(hanzi, defaultMeshSize);
    }

    public static Sprite LookupOverrideRecipes(char hanZi) {
        foreach (var item in instance.overrideRecipes)
            if (hanZi == item.character)
                return item.sprite;

        return null;
    }

    public static Texture RequestCharacterTexture(char hanzi, Vector2 meshSize) {
        CheckInstance();

        var tryToLookUp = LookupOverrideRecipes(hanzi);

        if (tryToLookUp != null) return tryToLookUp.texture;

        instance.font.RequestCharactersInTexture(hanzi.ToString());
        
        CharacterInfo characterInfo;
        instance.font.GetCharacterInfo(hanzi, out characterInfo);

        var uvStartOffset = characterInfo.uvBottomLeft;
        var uvEndOffset = characterInfo.uvTopRight;

        var uvStartPos = new Vector2(uvStartOffset.x, uvEndOffset.y);
        uvStartPos = new Vector2(uvStartPos.x * instance.fontTexture.width,
            uvStartPos.y * instance.fontTexture.height);

        var uvEndPos = new Vector2(uvEndOffset.x, uvStartOffset.y);
        uvEndPos = new Vector2(uvEndPos.x * instance.fontTexture.width, uvEndPos.y * instance.fontTexture.height);

        // Create a temporary RenderTexture of the same size as the texture
        RenderTexture tmp = RenderTexture.GetTemporary(
            instance.fontTexture.width,
            instance.fontTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        // Blit the pixels on texture to the RenderTexture
        Graphics.Blit(instance.fontTexture, tmp);

        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;

        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = tmp;

        // Create a new readable Texture2D to copy the pixels to it
        Texture2D myTexture2D = instance.CreatefillTexture2D(Color.clear, (int) meshSize.x, (int) meshSize.y);

        var uvSizePos = uvEndPos - uvStartPos;
        var sizeRect = new Rect((int) uvStartPos.x, (int) uvStartPos.y, (int) uvSizePos.x, (int) uvSizePos.y);

#if PLATFORM_STANDALONE_WIN
        sizeRect.y = instance.fontTexture.height - sizeRect.y - uvSizePos.y;
#endif

        if (meshSize.x - uvSizePos.x < 0 || meshSize.y - uvSizePos.y < 0) {
            Debug.LogWarning($"meshSize {meshSize} is too small");
            return null;
        }

        // Copy the pixels from the RenderTexture to the new Texture
        myTexture2D.ReadPixels(sizeRect,
            (int) (meshSize.x - uvSizePos.x) / 2,
            (int) (meshSize.y - uvSizePos.y) / 2);
        myTexture2D.Apply();

        // Flip Texture
        var output = instance.CreatefillTexture2D(Color.clear, (int) meshSize.x, (int) meshSize.y);
        var startOffsetX = (int) (meshSize.x - uvSizePos.x) / 2;
        var startOffsetY = (int) (meshSize.y - uvSizePos.y) / 2;

        switch (instance.GetFlipType(characterInfo.uvBottomRight, characterInfo.uvTopLeft)) {
            case EFlipType.DABC:
                // meshRenderer.transform.eulerAngles = new Vector3(0f, 0f, 90f);
                for (int i = startOffsetX; i < uvSizePos.x + startOffsetX; i++)
                for (int j = startOffsetY; j < uvSizePos.y + startOffsetY; j++)
                    output.SetPixel(-j, i, myTexture2D.GetPixel(i, j));

                break;
            case EFlipType.DCBA:
                // meshRenderer.transform.eulerAngles = new Vector3(0f, 180f, 180f);
                for (int i = startOffsetX; i < uvSizePos.x + startOffsetX; i++)
                for (int j = startOffsetY; j < uvSizePos.y + startOffsetY; j++)
                    output.SetPixel(-i, j, myTexture2D.GetPixel(-i, -j));

                break;
        }

        output.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = previous;

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(tmp);

        return output;
    }

    private EFlipType GetFlipType(Vector2 bottomRight, Vector2 topLeft) {
        if (topLeft.x >= bottomRight.x && topLeft.y >= bottomRight.y)
            return EFlipType.DABC;
        else
            return EFlipType.DCBA;
    }

    private Texture2D CreatefillTexture2D(Color color, int textureWidth, int textureHeight) {
        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        int numOfPixels = texture.width * texture.height;
        Color[] colors = new Color[numOfPixels];
        for (int x = 0; x < numOfPixels; x++) colors[x] = color;

        texture.SetPixels(colors);
        return texture;
    }

    // TL --- TR A --- B  ->  D --- A D --- C
    //  |      | |     |  ->  |     | |     |
    // BL --- BR D --- C  ->  C --- B A --- B
    public enum EFlipType {
        DABC,
        DCBA
    }

    [Serializable]
    public struct OverrideRecipe {
        public char character;
        public Sprite sprite;
    }
}