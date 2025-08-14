using UnityEngine;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;


namespace VNEngine
{
    public class FadeOutMusicNode : Node
    {

        public override void Run_Node()
        {
            Debug.Log($"Fading out {FMODAudioManager.Instance.currentMusic}");
            FMODAudioManager.Instance.FadeOutMusic(1f);
            Debug.Log($"Fading out {FMODAudioManager.Instance.currentAmbient}");
            FMODAudioManager.Instance.FadeOutAmbient(1f);
            Finish_Node();
        }
    }
}
