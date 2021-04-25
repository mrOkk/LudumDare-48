using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnableView : MonoBehaviour
{
    public Turnable[] RequiresOn;
    public bool TurnablesAreOn => RequiresOn.All(x => x.State == Turnable.ETurnableState.On);

    private void Start()
    {
        foreach (var turnable in RequiresOn)
            turnable.StateChangedAction += StateChangedAction;

        StateChangedAction(Turnable.ETurnableState.Transition);
    }

    private void StateChangedAction(Turnable.ETurnableState obj)
    {
        gameObject.SetActive(TurnablesAreOn);
    }
}
