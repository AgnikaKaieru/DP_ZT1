using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Core
{
    public class SceneCore_LvlA : SceneCore
    {
        [Tooltip("Size of area agents can move on.")]
        [SerializeField] private Vector2 playableAreaSize = new Vector2 (10, 10);


        public override void Initialize_SceneCore()
        {
            base.Initialize_SceneCore();

            //Prepare playable area
            playableAreaSize = math.clamp(playableAreaSize, new float2(1, 1), new float2(100, 100));

            //Spawn and initialize agents
        }

        public override void StartScene()
        {
            base.StartScene();

            GameCore.Instance.Resume();
        }
    }
}
