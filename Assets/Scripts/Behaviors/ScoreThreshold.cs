using System;
using UnityEngine;

namespace Behaviors
{
    public class ScoreThreshold : MonoBehaviour
    {
        public int Points { get; }
        
        //set bounds, subscribe to event from parent WallSegment
        private void OnTriggerExit(Collider other)
        {
            //send points score event
        }
    }
}