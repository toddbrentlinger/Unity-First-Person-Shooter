using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UMotionEditor
{
    public static class ParticlesUtility
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

        public static void SetLifetime(ParticleSystem particleSystem, float lifetime)
        {
            #if UNITY_5_5_OR_NEWER
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.startLifetime = lifetime;
            #else
            particleSystem.startLifetime = lifetime;
            #endif
        }

        public static void SetMaxParticles(ParticleSystem particleSystem, int maxParticles)
        {
            #if UNITY_5_5_OR_NEWER
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.maxParticles = maxParticles;
            #else
            particleSystem.maxParticles = maxParticles;
            #endif
        }

        public static void SetSimulationSpace(ParticleSystem particleSystem, ParticleSystemSimulationSpace simulationSpace)
        {
            #if UNITY_5_5_OR_NEWER
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.simulationSpace = simulationSpace;
            #else
            particleSystem.simulationSpace = simulationSpace;
            #endif
        }

        //********************************************************************************
        // Private Methods
        //********************************************************************************
    }
}
