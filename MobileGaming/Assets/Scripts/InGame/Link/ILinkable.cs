using System;
using UnityEngine;

public interface ILinkable
{
    public Vector3 Position { get; }
    public bool Inputable { get;}
    public bool Outputable { get;}
    
    public void SetStartLinkable(MachineLink link);
    public void SetEndLinkable(MachineLink link);
    public bool IsAvailable(MachineLink link);
    public event Action OnAvailable;
}
