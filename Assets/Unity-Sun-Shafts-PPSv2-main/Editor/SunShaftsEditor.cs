using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor (typeof(SunShafts))]
class SunShaftsEditor : Editor 
{	
	SerializedObject serObj;	
	
	SerializedProperty radialBlurIterations;
	SerializedProperty sunColor;
	SerializedProperty sunThreshold;
	SerializedProperty sunShaftBlurRadius;
	SerializedProperty sunShaftIntensity;
	SerializedProperty useDepthTexture;
    SerializedProperty resolution;
    SerializedProperty screenBlendMode;
    SerializedProperty maxRadius;
    SerializedProperty sunShaftsShader;
    SerializedProperty simpleClearShader;

    void OnEnable () 
    {
		serObj = new SerializedObject (target);
		
		screenBlendMode = serObj.FindProperty("screenBlendMode");
		
		sunColor = serObj.FindProperty("sunColor");
		sunThreshold = serObj.FindProperty("sunThreshold");
		
		sunShaftBlurRadius = serObj.FindProperty("sunShaftBlurRadius");
		radialBlurIterations = serObj.FindProperty("radialBlurIterations");
		
		sunShaftIntensity = serObj.FindProperty("sunShaftIntensity");
        
        resolution =  serObj.FindProperty("resolution");
        
        maxRadius = serObj.FindProperty("maxRadius"); 
		
		useDepthTexture = serObj.FindProperty("useDepthTexture");

        sunShaftsShader = serObj.FindProperty("sunShaftsShader");

        simpleClearShader = serObj.FindProperty("simpleClearShader");
    }


    public override void OnInspectorGUI ()
    {        
    	serObj.Update ();
    	
    	EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField (useDepthTexture, new GUIContent ("Rely on Z Buffer?"));
		if((target as SunShafts).GetComponent<Camera>())
			GUILayout.Label("Current camera mode: "+ (target as SunShafts).GetComponent<Camera>().depthTextureMode, EditorStyles.miniBoldLabel);
		
    	EditorGUILayout.EndHorizontal();
		
    	EditorGUILayout.PropertyField (resolution,  new GUIContent("Resolution"));
     	EditorGUILayout.PropertyField (screenBlendMode, new GUIContent("Blend mode"));
    	
		EditorGUILayout.Separator ();
    	
		EditorGUILayout.PropertyField (sunThreshold,  new GUIContent ("Threshold color"));
    	EditorGUILayout.PropertyField (sunColor,  new GUIContent ("Shafts color"));
        maxRadius.floatValue = 1.0f - EditorGUILayout.Slider ("Distance falloff", 1.0f - maxRadius.floatValue, 0.1f, 1.0f);
    	
    	EditorGUILayout.Separator ();
    	
    	sunShaftBlurRadius.floatValue = EditorGUILayout.Slider ("Blur size", sunShaftBlurRadius.floatValue, 1.0f, 10.0f);
    	radialBlurIterations.intValue = EditorGUILayout.IntSlider ("Blur iterations", radialBlurIterations.intValue, 1, 3);
    	
    	EditorGUILayout.Separator ();
    	
    	EditorGUILayout.PropertyField (sunShaftIntensity,  new GUIContent("Intensity"));

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(sunShaftsShader, new GUIContent("Sun Shafts Shader"));

        EditorGUILayout.PropertyField(simpleClearShader, new GUIContent("Simple Clear Shader"));

        serObj.ApplyModifiedProperties();
    }
}
