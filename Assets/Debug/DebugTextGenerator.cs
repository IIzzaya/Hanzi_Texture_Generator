using UnityEngine;

public class DebugTextGenerator : MonoBehaviour {
    public Font font;
    public char hanZi;
    public MeshRenderer fontTexture;

    private MeshFilter _meshFilter;
    private Texture2D txtTexture;

    private void Awake() {
        Debug.Assert(font);

        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.GetComponent<MeshRenderer>().material.mainTexture = font.material.mainTexture;
        fontTexture.material.mainTexture = font.material.mainTexture;
    }

    private void Update() {
        GenerateCharacter(hanZi);
    }

    public void GenerateCharacter(char hanZi) {
        font.RequestCharactersInTexture(hanZi.ToString());

        CharacterInfo characterInfo;
        font.GetCharacterInfo(hanZi, out characterInfo);

        var uvs = new Vector2[4];

        uvs[0] = characterInfo.uvBottomLeft;
        uvs[1] = characterInfo.uvTopRight;
        uvs[2] = characterInfo.uvBottomRight;
        uvs[3] = characterInfo.uvTopLeft;

        var tex = font.material.mainTexture as Texture2D;

        DebugPanel.Log($"index: {characterInfo.index}");
        DebugPanel.Log($"advance: {characterInfo.advance}");
        DebugPanel.Log($"bearing: {characterInfo.bearing}");
        DebugPanel.Log("glyphWidth: " + characterInfo.glyphWidth);
        DebugPanel.Log("glyphHeight: " + characterInfo.glyphHeight);
        DebugPanel.Log($"maxXY: {characterInfo.maxX}, {characterInfo.maxY}");
        DebugPanel.Log($"minXY: {characterInfo.minX}, {characterInfo.minY}");
        DebugPanel.Log($"uv.BL: {characterInfo.uvBottomLeft}");
        DebugPanel.Log($"uv.BR: {characterInfo.uvBottomRight}");
        DebugPanel.Log($"uv.TL: {characterInfo.uvTopLeft}");
        DebugPanel.Log($"uv.TR: {characterInfo.uvTopRight}");

        DebugPanel.Log(
            $"textureSize: {fontTexture.material.mainTexture.width}, {fontTexture.material.mainTexture.height}");

        DebugDraw.WireBox(fontTexture.transform.position, fontTexture.transform.lossyScale, 0, Color.red, 1f);

        var uvStartOffset = characterInfo.uvBottomLeft;
        var uvEndOffset = characterInfo.uvTopRight;

        var uvSize = uvEndOffset - uvStartOffset;
        var uvCenterOffset = uvStartOffset + (uvSize) / 2;
        uvCenterOffset.x *= fontTexture.transform.lossyScale.x;
        uvCenterOffset.y *= fontTexture.transform.lossyScale.y;
        uvSize.x *= fontTexture.transform.lossyScale.x;
        uvSize.y *= fontTexture.transform.lossyScale.y;

        uvStartOffset.x *= fontTexture.transform.lossyScale.x;
        uvStartOffset.y *= fontTexture.transform.lossyScale.y;

        uvEndOffset.x *= fontTexture.transform.lossyScale.x;
        uvEndOffset.y *= fontTexture.transform.lossyScale.y;

        var uvButtomRightOffset = characterInfo.uvBottomRight;

        uvButtomRightOffset.y = uvButtomRightOffset.y * fontTexture.transform.lossyScale.y;
        uvButtomRightOffset.x *= fontTexture.transform.lossyScale.x;

        var uvTopLeftOffset = characterInfo.uvTopLeft;

        uvTopLeftOffset.y = uvTopLeftOffset.y * fontTexture.transform.lossyScale.y;
        uvTopLeftOffset.x *= fontTexture.transform.lossyScale.x;

        DebugDraw.WireBox(
            (fontTexture.transform.position - fontTexture.transform.lossyScale / 2) + (Vector3) uvCenterOffset,
            uvSize, 0, Color.red, 1f);

        DebugDraw.WireBox(
            (fontTexture.transform.position - fontTexture.transform.lossyScale / 2) + (Vector3) uvStartOffset,
            0.05f, 0, Color.green, 1f);
        DebugDraw.WireBox(
            (fontTexture.transform.position - fontTexture.transform.lossyScale / 2) + (Vector3) uvButtomRightOffset,
            0.05f, 0, Color.green, 1f);

        Debug.DrawLine(
            (fontTexture.transform.position - fontTexture.transform.lossyScale / 2) + (Vector3) uvStartOffset,
            (fontTexture.transform.position - fontTexture.transform.lossyScale / 2) + (Vector3) uvButtomRightOffset,
            Color.green, 1f);

        DebugDraw.WireBox(
            (fontTexture.transform.position - fontTexture.transform.lossyScale / 2) + (Vector3) uvEndOffset,
            0.05f, 0, Color.magenta, 1f);
        DebugDraw.WireBox(
            (fontTexture.transform.position - fontTexture.transform.lossyScale / 2) + (Vector3) uvTopLeftOffset,
            0.05f, 0, Color.magenta, 1f);

        Debug.DrawLine(
            (fontTexture.transform.position - fontTexture.transform.lossyScale / 2) + (Vector3) uvEndOffset,
            (fontTexture.transform.position - fontTexture.transform.lossyScale / 2) + (Vector3) uvTopLeftOffset,
            Color.magenta, 1f);

        _meshFilter.mesh.uv = uvs;
    }

    private void OnDrawGizmos() {
        // if (font == null) return;
        //
        // if (_meshFilter == null) {
        //     _meshFilter = GetComponent<MeshFilter>();
        //     _meshFilter.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = font.material.mainTexture;
        // }
        //
        // if (fontTexture.sharedMaterial.mainTexture == null) fontTexture.material.mainTexture = font.material.mainTexture;
        //
        // GenerateCharacter(hanZi);
    }
}