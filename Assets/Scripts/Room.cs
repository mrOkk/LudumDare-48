using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Room : MonoBehaviour
{
    public LevelManager LevelManager;
    public ComputerView ComputerView;
    public Display Display;
    public Player Player;
    public Camera Camera => Player.Camera;
    public List<Component> TurnOn;
    public Room NextRoom;
    public AudioMixerSnapshot MixerSnapshot;

    public bool GameplayActivated;


    public float TimeSpentInRoom;
    private int _upcomingEventIndex = 0;
    public RoomEvent[] RoomEvents;
    public Turnable[] PowerRequirement;

    private bool _isDirty = true;
    private bool _turnables = false;
    public bool PowerIsOn
    {
        get
        {
            if (_isDirty)
                _turnables = PowerRequirement.Length == 0 || PowerRequirement.All(x => x.State == Turnable.ETurnableState.On);

            return _turnables;
        }
    }

    [HideInInspector]
    public Room PreviousRoom;

    public void Init(Room previousRoom)
    {
        PreviousRoom = previousRoom;
        Player = GetComponentInChildren<Player>();
        Display = GetComponentInChildren<Display>();
        ComputerView = GetComponentInChildren<ComputerView>();
        LevelManager = GetComponentInChildren<LevelManager>();

        foreach (var roomie in new[] {(IRoomie) Player, Display, ComputerView, LevelManager})
            if (roomie != null)
                roomie.ParentRoom = this;

        foreach (var turnable in PowerRequirement)
            turnable.StateChangedAction += StateChangedAction;

        NextRoom?.Init(this);
    }

    private void StateChangedAction(Turnable.ETurnableState obj)
    {
        _isDirty = true;
        
        NextRoom.LevelManager.SetComputerTurnOnStatus(PowerIsOn);

        if (!PowerIsOn)
        {
            if (Display == GameManager.Instance.Displays.Last())
                GameManager.Instance.ZoomOutDisplay();
        }
    }

    public void Focus(bool on)
    {
        if (!on)
            Player.enabled = false;
        else
            StartCoroutine(WaitForZoomOut());

        var scene = SceneManager.GetSceneByName(gameObject.name);
        SceneManager.SetActiveScene(scene);

        if (on)
            LevelManager.Activate();
        else
            LevelManager.Deactivate();

        IEnumerator WaitForZoomOut()
        {
            yield return new WaitWhile(() => Display.CurrentState != Display.State.Idle);
            Player.enabled = true;
        }
    }

    public void Update()
    {
        if (this == GameManager.Instance.ActiveRoom && LevelManager.CurrentState == ELevelState.Gameplay)
            TimeSpentInRoom += Time.deltaTime;

        for (int i = _upcomingEventIndex; i < RoomEvents.Length; i++)
        {
            if (TimeSpentInRoom > RoomEvents[i].TimeSinceRoomActive)
            {
                RoomEvents[i].Invoke();
                _upcomingEventIndex++;
            }
            else
            {
                break;
            }
        }

    }

    [System.Serializable]
    public class RoomEvent
    {
        public float TimeSinceRoomActive;
        public Turnable Turnable;
        public Turnable.ETurnableState TargetState;
        public AudioSource Source;

        public void Invoke()
        {
            if (Turnable)
                Turnable.State = TargetState;
            if (Source)
                Source.Play();
        }
    }

    public async Task Reload()
    {
        
    }
}