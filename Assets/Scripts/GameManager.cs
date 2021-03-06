using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Player ActivePlayer => ActiveRoom.Player;
    public Room NextRoom => ActiveRoom.NextRoom;
    public Room PreviousRoom => ActiveRoom.PreviousRoom;
    public AudioSource MetaSource;

    [SerializeField] private Room _activeRoom;

    public Room ActiveRoom
    {
        get => _activeRoom;

        set
        {
            if (_activeRoom != value)
            {
                _activeRoom?.Focus(false);
                value.Focus(true);

                _activeRoom = value;
            }
        }
    }

    public List<Display> Displays;

    public bool AnyDisplays => Displays.Count > 0;

    private int firstVisibleDisplay;

    private void Awake()
    {
        Instance = this;
        ActiveRoom.Init(null);
        ActiveRoom.Focus(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4) && Displays.Count > 0) {
            var firstDisplay = Displays[firstVisibleDisplay];

            if (firstDisplay.CurrentState == Display.State.ControllingNextRoom) {
                StartCoroutine(MakeDisappear());

                IEnumerator MakeDisappear()
                {
                    yield return firstDisplay.MakeDisappear();
                    //firstDisplay.PlayerCamera.enabled = false;
                    firstVisibleDisplay++;
                }
            }
        }

        if (Input.GetMouseButton(1))
        {
            ActivePlayer.DropBurden();
        }
    }

    public void ZoomOutDisplay()
    {
        var lastDisplay = Displays[Displays.Count - 1];

        if (lastDisplay.CurrentState == Display.State.UI || lastDisplay.CurrentState == Display.State.ControllingNextRoom) {
            StartCoroutine(ZoomOutAndRemove());

            IEnumerator ZoomOutAndRemove()
            {
                yield return lastDisplay.ZoomOut();
                Displays.RemoveAt(Displays.Count - 1);
            }
        }
    }

    public void Descend()
    {
        ActiveRoom = ActiveRoom.NextRoom;

        if (ActiveRoom.PreviousRoom != null)
            ActiveRoom.PreviousRoom.Player.AudioListener.enabled = false;

        ActiveRoom.Player.AudioListener.enabled = true;
        ActiveRoom.MixerSnapshot.TransitionTo(2f);
    }

    public void Ascend()
    {
        if (ActiveRoom.PreviousRoom != null) {
            ActiveRoom = ActiveRoom.PreviousRoom;
            ActiveRoom.MixerSnapshot.TransitionTo(2f);
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1)
    {
        if(clip == null)
            return;

        MetaSource.clip = clip;
        MetaSource.volume = volume;
        MetaSource.Play();
    }
}
