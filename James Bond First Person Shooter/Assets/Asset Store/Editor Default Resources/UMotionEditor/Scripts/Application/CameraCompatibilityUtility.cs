#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2018_1_OR_NEWER
using UnityEngine.Experimental.Rendering;
#endif

namespace UMotionEditor
{
    public static class CameraCompatibilityUtility
    {
        //********************************************************************************
        // Public Properties
        //********************************************************************************

        public static event Action<Camera[]> SrpBeginFrameRendering
        {
            add
            {
                #if UNITY_2018_1_OR_NEWER
                RenderPipeline.beginFrameRendering += value;
                #endif
            }
            remove
            {
                #if UNITY_2018_1_OR_NEWER
                RenderPipeline.beginFrameRendering -= value;
                #endif
            }
        }

        public static event Action<Camera> SrpBeginCameraRendering
        {
            add
            {
                #if UNITY_2018_1_OR_NEWER
                RenderPipeline.beginCameraRendering += value;
                #endif
            }
            remove
            {
                #if UNITY_2018_1_OR_NEWER
                RenderPipeline.beginCameraRendering -= value;
                #endif
            }
        }

        //********************************************************************************
        // Private Properties
        //********************************************************************************

        //********************************************************************************
        // Public Methods
        //********************************************************************************

        public static void SetAllowHdr(Camera camera, bool hdrAllowed)
        {
            #if UNITY_5_6_OR_NEWER
            camera.allowHDR = hdrAllowed;
            #else
            camera.hdr = hdrAllowed;
            #endif
        }

        public static bool GetAllowHdr(Camera camera)
        {
            #if UNITY_5_6_OR_NEWER
            return camera.allowHDR;
            #else
            return camera.hdr;
            #endif
        }

        //********************************************************************************
        // Private Methods
        //********************************************************************************
    }
}
#endif
