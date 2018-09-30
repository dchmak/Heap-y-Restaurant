/*
* Created by Daniel Mak
*/

using UnityEngine;
using UnityEngine.Events;

public class EndOfAnnouncement : MonoBehaviour {

    public UnityEvent @event;

	public void End() {
        @event.Invoke();
        gameObject.SetActive(false);
    }
}