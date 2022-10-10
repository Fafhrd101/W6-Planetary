using UnityEngine;
using System.Collections;

namespace FORGE3D
{
    public class F3DBurnoutExample : MonoBehaviour
    {
        public ParticleSystem[] Heat;

        public bool heatShow;
        public MeshRenderer[] visualRenderers;
        private int _burnoutId;

        private float burnout;
        private float heatBias = 0f;

        // Use this for initialization
        private void Start()
        {
            _burnoutId = Shader.PropertyToID("_Burnout");
        }

        // Update is called once per frame
        private void Update()
        {
            burnout = Mathf.Lerp(0, 1f, Mathf.Sin(Time.time));

            if (burnout > heatBias && heatShow)
            {
                heatShow = false;
            
                for (var i = 0; i < Heat.Length; i++)
                    Heat[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            else if (burnout <= heatBias && !heatShow)
            {
                heatShow = true;

                for (var i = 0; i < Heat.Length; i++)
                    Heat[i].Play(true);
            }


            for (var i = 0; i < visualRenderers.Length; i++)
                visualRenderers[i].material.SetFloat(_burnoutId, burnout);
        }
    }
}