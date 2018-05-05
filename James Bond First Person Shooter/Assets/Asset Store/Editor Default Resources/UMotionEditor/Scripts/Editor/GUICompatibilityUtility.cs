using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace UMotionEditor
{
    public static class GUICompatibilityUtility
    {
        //********************************************************************************
        // Public Properties
        //********************************************************************************

        //********************************************************************************
        // Private Properties
        //********************************************************************************

        //----------------------
        // Inspector
        //----------------------

        //----------------------
        // Internal
        //----------------------

        //********************************************************************************
        // Public Methods
        //********************************************************************************

        [MenuItem("Window/UMotion Editor/Contact Support", true, 1232)]
        public static bool RestartUMotionMenuItemValidate()
        {
            CheckCurrentAssembly();
            return true;
        }

        [MenuItem("Window/UMotion Editor/Contact Support", false, 1232)]
        public static void RestartUMotionMenuItem()
        {
            Help.BrowseURL("https://support.soxware.com");
        }

        public static Color ColorField(GUIContent label, Color value, bool showEyedropper, bool showAlpha, bool hdr, params GUILayoutOption[] options)
        {
            #if UNITY_2018_1_OR_NEWER
            return EditorGUILayout.ColorField(label, value, showEyedropper, showAlpha, hdr, options);
            #else
            return EditorGUILayout.ColorField(label, value, showEyedropper, showAlpha, hdr, null, options);
            #endif
        }

        //********************************************************************************
        // Private Methods
        //********************************************************************************

        private static bool CheckCurrentAssembly()
        {
            string applicationAssemblyName = VersionCompatibilityUtility.GetCurrentAssemblyName();
            string editorAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            
            bool assemblyOk = (applicationAssemblyName == "Assembly-CSharp") && (editorAssemblyName == "Assembly-CSharp-Editor");

            if (!assemblyOk)
            {
                string message = string.Format("The UMotion script files are not compiled to the correct assembly:\r\n\r\n\"{0}\"\r\n(should be \"Assembly-CSharp\")\r\n\r\n\"{1}\"\r\n(should be \"Assembly-CSharp-Editor\")\r\n\r\nMake sure that UMotion isn't installed as a sub-folder of a folder named \"Plugins\". Also make sure not to use assembly definition files for UMotion script files.", applicationAssemblyName, editorAssemblyName);
                EditorUtility.DisplayDialog("UMotion - Invalid Assembly", message, "OK");
            }

            return assemblyOk;
        }
    }
}