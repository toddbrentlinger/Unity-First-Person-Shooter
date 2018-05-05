using UnityEngine;
using System.Collections;

namespace UMotionEditor
{
    public static class TextEditorUtility
    {
        //********************************************************************************
        // Public Properties
        //********************************************************************************

        //********************************************************************************
        // Private Properties
        //********************************************************************************

        //********************************************************************************
        // Public Methods
        //********************************************************************************

        public static void UpdateScrollOffsetIfNeeded(TextEditor textEditor)
        {
            #if UNITY_5_5_OR_NEWER
            textEditor.UpdateScrollOffsetIfNeeded(Event.current);
            #else
            textEditor.UpdateScrollOffsetIfNeeded();
            #endif
        }

        //********************************************************************************
        // Private Methods
        //********************************************************************************
    }
}