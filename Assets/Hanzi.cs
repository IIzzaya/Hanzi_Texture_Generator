using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Hanzi : MonoBehaviour {
    private RawImage _rawImage;
    public char hanzi;
    public Vector2 size;
    public bool apply = false;

    private void Awake() {
        _rawImage = GetComponent<RawImage>();
        _rawImage.texture = HanziTextureGenerator.RequestCharacterTexture(hanzi, size);
    }

    private void Update() {
        if (apply) {
            apply = false;
            _rawImage.texture = HanziTextureGenerator.RequestCharacterTexture(hanzi, size);
        }
    }

    private void OnDrawGizmos() {
        if (_rawImage == null) {
            Awake();
        }
        else {
            Update();
        }
    }
}