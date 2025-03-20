using System;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class LIC : MonoBehaviour
{
    public float minU = -47.95f; // -10.37;
    public float maxU = 43.49f; // 10.37;
    public float minV = -44.56f; // -4.79;
    public float maxV = 41.64f; // 4.79;

    public float minMag = -0.0f; // -4.79;
    public float maxMag = 60.0f; // -4.79;
    public bool isabelIndexing = false;
    public bool RK4 = false;
    private float maxMagnitude;

    [Range(0.00f, 500.0f)]
    public float arcLength = 50.0f;

    [Range(0.01f, 6.0f)]
    public float stepSize = 0.5f;

    [Range(0.0001f, 500.0f)]
    public float sigma = 0.4f;
    public float contrastAlpha = 1.0f;
    [Range(-1.0f, 1.0f)]
    public float contrastBeta = 0.0f;
    public Texture2D uTex;
    public Texture2D vTex;
    public Texture2D noiseTex;
    public Texture2D maskTex;
    public Texture2D colorMap;
    public ComputeShader computeShader;
    public RenderTexture resultTex;
    public string savePath = "/Users/edvart/Documents/Result.png";

    private void Start()
    {

    }
    private void Update()
    {
        computeShader.SetTexture(0, "uTex", uTex);
        computeShader.SetTexture(0, "vTex", vTex);
        computeShader.SetTexture(0, "noiseTex", noiseTex);
        computeShader.SetTexture(0, "maskTex", maskTex);
        computeShader.SetTexture(0, "resultTex", resultTex);
        computeShader.SetTexture(0, "colorMapTex", colorMap);

        computeShader.SetFloat("minU", minU);
        computeShader.SetFloat("minV", minV);
        computeShader.SetFloat("maxU", maxU);
        computeShader.SetFloat("maxV", maxV);
        computeShader.SetFloat("minMag", minMag);
        computeShader.SetFloat("maxMag", maxMag);
        computeShader.SetBool("isabelIndexing", isabelIndexing);
        computeShader.SetBool("RK4", RK4);

        computeShader.SetFloat("resX", resultTex.width);
        computeShader.SetFloat("resY", resultTex.height);

        float maxAbsU = Mathf.Abs(minU) > Mathf.Abs(maxU) ? Mathf.Abs(minU) : Math.Abs(maxU);
        float maxAbsV = Mathf.Abs(minV) > Mathf.Abs(maxV) ? Mathf.Abs(minV) : Math.Abs(maxV);
        maxMagnitude = Mathf.Sqrt(maxAbsU * maxAbsU + maxAbsV * maxAbsV);

        computeShader.SetFloat("maxMagnitude", maxMagnitude);
        computeShader.SetFloat("arc_length", arcLength);
        computeShader.SetFloat("step_size", stepSize);
        computeShader.SetFloat("sigma", sigma);
        computeShader.SetFloat("contrastAlpha", contrastAlpha);
        computeShader.SetFloat("contrastBeta", contrastBeta);

        computeShader.Dispatch(0, Mathf.CeilToInt(resultTex.width / 8.0f), Mathf.CeilToInt(resultTex.height / 8.0f), 1);
    }
    [ContextMenu("Save Result")]
    private void SaveResult()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = resultTex;

        Texture2D tex = new Texture2D(resultTex.width, resultTex.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, resultTex.width, resultTex.height), 0, 0);
        tex.Apply();

        // Apply gamma correction
        Color[] pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(
                Mathf.Pow(pixels[i].r, 1 / 2.2f),
                Mathf.Pow(pixels[i].g, 1 / 2.2f),
                Mathf.Pow(pixels[i].b, 1 / 2.2f),
                pixels[i].a
            );
        }
        tex.SetPixels(pixels);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);

        RenderTexture.active = currentRT;
        Destroy(tex);
    }

}
