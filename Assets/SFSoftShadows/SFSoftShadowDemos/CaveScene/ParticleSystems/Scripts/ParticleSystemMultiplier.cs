using System;
using UnityEngine;

namespace UnityStandardAssets.Effects
{
    public class ParticleSystemMultiplier : MonoBehaviour
    {
        // a simple script to scale the size, speed and lifetime of a particle system

        public float multiplier = 1;


        private void Start()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem system in systems)
            {
                var m = system.main;
                var a1 = m.startSize;
                a1.curveMultiplier = multiplier;
                a1 = m.startSpeed;
                a1.curveMultiplier = multiplier;
                a1 = m.startLifetime;
                a1.curveMultiplier = Mathf.Lerp(multiplier, 1, 0.5f);
                system.Clear();
                system.Play();
            }
        }
    }
}
