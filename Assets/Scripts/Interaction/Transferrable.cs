using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Transferrable : InteractableObject
{
    public Vector3 CarryOffset;
    public Vector3 CarryRotation;
    public bool CanDrop;

    private Rigidbody _rigidbody;
    private Rigidbody Rigidbody
    {
        get
        {
            if (_rigidbody == null)
                _rigidbody = gameObject.AddComponent<Rigidbody>();

            return _rigidbody;
        }
    }

    public override bool CanInteract(Player player)
    {
        return base.CanInteract(player) && player.Burden == null;
    }

    public override void Interact(Player player)
    {
        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;

        if(CanDrop)
            Rigidbody.isKinematic = true;

        player.Burden = this;
    }

    public override void AcceptRequiredItem(Transferrable requiredItem)
    {
        throw new System.NotImplementedException();
    }

    public void Carried(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position + rotation * CarryOffset, rotation * Quaternion.Euler(CarryRotation));
    }

    public void Drop(Player player)
    {
        if (CanDrop)
        {
            player.Burden = null;
            Rigidbody.isKinematic = false;

            foreach (var collider in GetComponentsInChildren<Collider>())
                collider.enabled = true;
        }
    }
}