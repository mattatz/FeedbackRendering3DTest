using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(FeedbackRendering3DTest))]
public class FeedbackRendering3DTestEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Reset"))
        {
            var test = target as FeedbackRendering3DTest;
            test.Reset();
        }
    }

}
