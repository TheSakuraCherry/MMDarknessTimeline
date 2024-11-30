#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace HovlStudio
{
    [InitializeOnLoad]
    public class RPChanger : EditorWindow
    {
        [InitializeOnLoadMethod]
        private static void LoadWindow()
        {
            string[] checkAsset = AssetDatabase.FindAssets("HSstartupCheck");
            foreach (var guid in checkAsset)
            {
                ShowWindow();
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        static private int pipeline;
        [MenuItem("Tools/RP changer for Hovl Studio assets")]

        public static void ShowWindow()
        {
            RPChanger window = (RPChanger)EditorWindow.GetWindow(typeof(RPChanger));
            window.minSize = new Vector2(250, 140);
            window.maxSize = new Vector2(250, 140);
        }

        public void OnGUI()
        {
            GUILayout.Label("Change VFX shaders to:");
            if (GUILayout.Button("URP/HDRP"))
            {
                FindShaders();
                ChangeToSG();
            }
            if (GUILayout.Button("Built-in RP"))
            {
                FindShaders();
                ChangeToBiRP();
            }
            GUILayout.Label("Don't forget to enable Depth and Opaque\ncheck-buttons in your URP asset seeting.", GUILayout.ExpandWidth(true));
        }

        static Shader Add_CG, Blend_CG, LightGlow, Lit_CenterGlow, Blend_TwoSides, Blend_Normals, Ice, Distortion, ParallaxIce, BlendDistort, VolumeLaser, Explosion, SwordSlash, ShockWave, SoftNoise;
        static Shader Blend_CG_SG, LightGlow_SG, Lit_CenterGlow_SG, Blend_TwoSides_SG, Blend_Normals_SG, Ice_SG, Distortion_SG, ParallaxIce_SG,
            BlendDistort_SG, VolumeLaser_SG, Explosion_SG, SwordSlash_SG, ShockWave_SG, SoftNoise_SG;
        static Material[] shaderMaterials;

        private static void FindShaders()
        {
            if (Shader.Find("Hovl/Particles/Add_CenterGlow") != null) Add_CG = Shader.Find("Hovl/Particles/Add_CenterGlow");
            if (Shader.Find("Hovl/Particles/Blend_CenterGlow") != null) Blend_CG = Shader.Find("Hovl/Particles/Blend_CenterGlow");
            if (Shader.Find("Hovl/Particles/LightGlow") != null) LightGlow = Shader.Find("Hovl/Particles/LightGlow");
            if (Shader.Find("Hovl/Particles/Lit_CenterGlow") != null) Lit_CenterGlow = Shader.Find("Hovl/Particles/Lit_CenterGlow");
            if (Shader.Find("Hovl/Particles/Blend_TwoSides") != null) Blend_TwoSides = Shader.Find("Hovl/Particles/Blend_TwoSides");
            if (Shader.Find("Hovl/Particles/Blend_Normals") != null) Blend_Normals = Shader.Find("Hovl/Particles/Blend_Normals");
            if (Shader.Find("Hovl/Particles/Ice") != null) Ice = Shader.Find("Hovl/Particles/Ice");
            if (Shader.Find("Hovl/Particles/Distortion") != null) Distortion = Shader.Find("Hovl/Particles/Distortion");
            if (Shader.Find("Hovl/Opaque/ParallaxIce") != null) ParallaxIce = Shader.Find("Hovl/Opaque/ParallaxIce");
            if (Shader.Find("Hovl/Particles/BlendDistort") != null) BlendDistort = Shader.Find("Hovl/Particles/BlendDistort");
            if (Shader.Find("Hovl/Particles/VolumeLaser") != null) VolumeLaser = Shader.Find("Hovl/Particles/VolumeLaser");
            if (Shader.Find("Hovl/Particles/Explosion") != null) Explosion = Shader.Find("Hovl/Particles/Explosion");
            if (Shader.Find("Hovl/Particles/SwordSlash") != null) SwordSlash = Shader.Find("Hovl/Particles/SwordSlash");
            if (Shader.Find("Hovl/Particles/ShockWave") != null) ShockWave = Shader.Find("Hovl/Particles/ShockWave");
            if (Shader.Find("Hovl/Particles/SoftNoise") != null) SoftNoise = Shader.Find("Hovl/Particles/SoftNoise");

            if (Shader.Find("Shader Graphs/HS_LightGlow") != null) LightGlow_SG = Shader.Find("Shader Graphs/HS_LightGlow");
            if (Shader.Find("Shader Graphs/HS_Lit_CenterGlow") != null) Lit_CenterGlow_SG = Shader.Find("Shader Graphs/HS_Lit_CenterGlow");
            if (Shader.Find("Shader Graphs/HS_Blend_TwoSides") != null) Blend_TwoSides_SG = Shader.Find("Shader Graphs/HS_Blend_TwoSides");
            if (Shader.Find("Shader Graphs/HS_Blend_Normals") != null) Blend_Normals_SG = Shader.Find("Shader Graphs/HS_Blend_Normals");
            if (Shader.Find("Shader Graphs/HS_Ice") != null) Ice_SG = Shader.Find("Shader Graphs/HS_Ice");
            if (Shader.Find("Shader Graphs/HS_Distortion") != null) Distortion_SG = Shader.Find("Shader Graphs/HS_Distortion");
            if (Shader.Find("Shader Graphs/HS_ParallaxIce") != null) ParallaxIce_SG = Shader.Find("Shader Graphs/HS_ParallaxIce");
            if (Shader.Find("Shader Graphs/HS_Blend_CG") != null) Blend_CG_SG = Shader.Find("Shader Graphs/HS_Blend_CG");
            if (Shader.Find("Shader Graphs/HS_BlendDistort") != null) BlendDistort_SG = Shader.Find("Shader Graphs/HS_BlendDistort");
            if (Shader.Find("Shader Graphs/HS_VolumeLaser") != null) VolumeLaser_SG = Shader.Find("Shader Graphs/HS_VolumeLaser");
            if (Shader.Find("Shader Graphs/HS_Explosion") != null) Explosion_SG = Shader.Find("Shader Graphs/HS_Explosion");
            if (Shader.Find("Shader Graphs/HS_SwordSlash") != null) SwordSlash_SG = Shader.Find("Shader Graphs/HS_SwordSlash");
            if (Shader.Find("Shader Graphs/HS_ShockWave") != null) ShockWave_SG = Shader.Find("Shader Graphs/HS_ShockWave");
            if (Shader.Find("Shader Graphs/HS_SoftNoise") != null) SoftNoise_SG = Shader.Find("Shader Graphs/HS_SoftNoise");

            string[] folderMat = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            shaderMaterials = new Material[folderMat.Length];

            for (int i = 0; i < folderMat.Length; i++)
            {
                var patch = AssetDatabase.GUIDToAssetPath(folderMat[i]);
                shaderMaterials[i] = (Material)AssetDatabase.LoadAssetAtPath(patch, typeof(Material));
            }
        }

        static private void ChangeToSG()
        {
            foreach (var material in shaderMaterials)
            {

                if (Shader.Find("Shader Graphs/HS_LightGlow") != null)
                {
                    if (material.shader == LightGlow)
                    {
                        material.shader = LightGlow_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_Lit_CenterGlow") != null)
                {
                    if (material.shader == Lit_CenterGlow)
                    {
                        material.shader = Lit_CenterGlow_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_Blend_TwoSides") != null)
                {
                    if (material.shader == Blend_TwoSides)
                    {
                        material.shader = Blend_TwoSides_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_Blend_Normals") != null)
                {
                    if (material.shader == Blend_Normals)
                    {
                        material.shader = Blend_Normals_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_Ice") != null)
                {
                    if (material.shader == Ice)
                    {
                        material.shader = Ice_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_ParallaxIce") != null)
                {
                    if (material.shader == ParallaxIce)
                    {
                        material.shader = ParallaxIce_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_Distortion") != null)
                {
                    if (material.shader == Distortion)
                    {
                        material.SetFloat("_ZWrite", 0);
                        material.shader = Distortion_SG;
                        material.SetFloat("_QueueControl", 1);
                        material.SetFloat("_BUILTIN_QueueControl", 1);
                        material.renderQueue = 2750;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_Blend_CG") != null)
                {
                    if (material.shader == Add_CG)
                    {
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        var cull = material.GetFloat("_CullMode");
                        material.shader = Blend_CG_SG;
                        material.SetFloat("_Cull", cull);
                        material.SetFloat("_Blend", 2);
                        material.SetFloat("_DstBlend", 1);
                        material.SetFloat("_SrcBlend", 5);
                        material.SetFloat("_BUILTIN_CullMode", cull);
                        material.SetFloat("_BUILTIN_Blend", 2);
                        material.SetFloat("_BUILTIN_DstBlend", 1);
                        material.SetFloat("_BUILTIN_SrcBlend", 5);
                        Debug.Log("Shaders changed successfully");
                    }
                    if (material.shader == Blend_CG)
                    {
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        material.shader = Blend_CG_SG;

                    }
                }
                else Debug.Log("First import shaders!");

                if (Shader.Find("Shader Graphs/HS_BlendDistort") != null)
                {
                    if (material.shader == BlendDistort)
                    {
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        material.shader = BlendDistort_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_VolumeLaser") != null)
                {
                    if (material.shader == VolumeLaser)
                    {
                        material.shader = VolumeLaser_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_Explosion") != null)
                {
                    if (material.shader == Explosion)
                    {
                        material.shader = Explosion_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_SwordSlash") != null)
                {
                    if (material.shader == SwordSlash)
                    {
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        material.shader = SwordSlash_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_ShockWave") != null)
                {
                    if (material.shader == ShockWave)
                    {
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        material.shader = ShockWave_SG;
                    }
                }

                if (Shader.Find("Shader Graphs/HS_SoftNoise") != null)
                {
                    if (material.shader == SoftNoise)
                    {
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        material.shader = SoftNoise_SG;
                    }
                }
            }
        }
        static private void ChangeToBiRP()
        {
            foreach (var material in shaderMaterials)
            {
                if (Shader.Find("Hovl/Particles/LightGlow") != null)
                {
                    if (material.shader == LightGlow_SG)
                    {
                        material.shader = LightGlow;
                    }
                }
                if (Shader.Find("Hovl/Particles/Lit_CenterGlow") != null)
                {
                    if (material.shader == Lit_CenterGlow_SG)
                    {
                        material.shader = Lit_CenterGlow;
                    }
                }
                if (Shader.Find("Hovl/Particles/Blend_TwoSides") != null)
                {
                    if (material.shader == Blend_TwoSides_SG)
                    {
                        material.shader = Blend_TwoSides;
                    }
                }
                if (Shader.Find("Hovl/Particles/Blend_Normals") != null)
                {
                    if (material.shader == Blend_Normals_SG)
                    {
                        material.shader = Blend_Normals;
                    }
                }
                if (Shader.Find("Hovl/Particles/Ice") != null)
                {
                    if (material.shader == Ice_SG)
                    {
                        material.shader = Ice;
                    }
                }
                if (Shader.Find("Hovl/Opaque/ParallaxIce") != null)
                {
                    if (material.shader == ParallaxIce_SG)
                    {
                        material.shader = ParallaxIce;
                    }
                }
                if (Shader.Find("Hovl/Particles/Distortion") != null)
                {
                    if (material.shader == Distortion_SG)
                    {
                        material.shader = Distortion;
                        material.renderQueue = 2750;
                    }
                }
                if (Shader.Find("Hovl/Particles/Add_CenterGlow") != null)
                {
                    if (material.shader == Blend_CG_SG)
                    {
                        if (material.HasProperty("_Blend"))
                        {
                            float blend = material.GetFloat("_Blend");
                            if (blend == 2)
                            {
                                material.shader = Add_CG;
                                Debug.Log("Shaders changed successfully");
                            }
                        }
                        if (material.HasProperty("_BUILTIN_Blend"))
                        {
                            float blend = material.GetFloat("_BUILTIN_Blend");
                            if (blend == 2)
                            {
                                material.shader = Add_CG;
                                Debug.Log("Shaders changed successfully");
                            }
                        }
                    }
                }
                if (Shader.Find("Hovl/Particles/Blend_CenterGlow") != null)
                {
                    if (material.shader == Blend_CG_SG)
                    {
                        if (material.HasProperty("_Blend"))
                        {
                            float blend = material.GetFloat("_Blend");
                            if (blend == 0)
                            {
                                material.shader = Blend_CG;
                                Debug.Log("Shaders changed successfully");
                            }
                        }
                        if (material.HasProperty("_BUILTIN_Blend"))
                        {
                            float blend = material.GetFloat("_BUILTIN_Blend");
                            if (blend == 0)
                            {
                                material.shader = Blend_CG;
                                Debug.Log("Shaders changed successfully");
                            }
                        }
                    }
                }
                if (Shader.Find("Hovl/Particles/BlendDistort") != null)
                {
                    if (material.shader == BlendDistort_SG)
                    {
                        material.shader = BlendDistort;
                    }
                }
                if (Shader.Find("Hovl/Particles/VolumeLaser") != null)
                {
                    if (material.shader == VolumeLaser_SG)
                    {
                        material.shader = VolumeLaser;
                    }
                }
                if (Shader.Find("Hovl/Particles/Explosion") != null)
                {
                    if (material.shader == Explosion_SG)
                    {
                        material.shader = Explosion;
                    }
                }
                if (Shader.Find("Hovl/Particles/SwordSlash") != null)
                {
                    if (material.shader == SwordSlash_SG)
                    {
                        material.shader = SwordSlash;
                    }
                }

                if (Shader.Find("Hovl/Particles/ShockWave") != null)
                {
                    if (material.shader == ShockWave_SG)
                    {
                        material.shader = ShockWave;
                    }
                }
                if (Shader.Find("Hovl/Particles/SoftNoise") != null)
                {
                    if (material.shader == SoftNoise_SG)
                    {
                        material.shader = SoftNoise;
                    }
                }
            }
        }

    }
}
#endif