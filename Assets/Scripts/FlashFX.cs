// From ghere:
// https://www.youtube.com/watch?v=tyDYKTgcWws

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using DG.Tweening;


public class FlashFX : MonoBehaviour
{

    [SerializeField] private float _duration = 0.25f;
    [SerializeField] public float _flashIntensity = 0.75f;

    private int _flashFXAmount = Shader.PropertyToID("_FlashAmount");
    private MeshRenderer[] _meshRenderers;
    private Material[] _materials;


    public float _lerpAmount;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();

        _materials = new Material[_meshRenderers.Length];
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i] = _meshRenderers[i].material;
        }

    }

    // Update is called once per frame
    private void Update()
    {
        // moved to UIKeyboard script
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            TweenInOut();
        }
    }

    private void OnLerpUpdate()
    {
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetFloat(_flashFXAmount, GetLerpValue());
        }
    }

    private void OnLerpComplete()
    {
        DOTween.To(GetLerpValue, SetLerpValue, 0f, _duration).OnUpdate(OnLerpUpdate);
    }



    private float GetLerpValue()
    {
        return _lerpAmount;
    }

    private void SetLerpValue(float newValue)
    {
        _lerpAmount = newValue;
    }

    public void TweenInOut()
    {
        _lerpAmount = 0f;
        DOTween.To(GetLerpValue, SetLerpValue, _flashIntensity, _duration).OnUpdate(OnLerpUpdate).OnComplete(OnLerpComplete);
    }

}
