/*
* Created by Daniel Mak
*/

using System;
using UnityEngine;

namespace Daniel.Event {
    [CreateAssetMenu(fileName = "New Event", menuName = "Event")]
    public class CustomEvent : ScriptableObject {
        public string stringToBeAnnounced = "";
        public Vector2 timeBetweenNewcomerRange = Vector2.zero;
        public Vector2 patientRange = Vector2.zero;
        public Vector2Int spendingRange = Vector2Int.zero;
        public Vector2 stayTimeRange = Vector2.zero;
        public int costPerSpacePerSecond = 0;
    }
}