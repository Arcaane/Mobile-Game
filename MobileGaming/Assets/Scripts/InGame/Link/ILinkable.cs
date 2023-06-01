using System;
using UnityEngine;

public interface ILinkable
{
    public Vector3 Position { get; }
    public float Width { get; }
    public bool Inputable { get;}
    public bool Outputable { get;}
    public void ShowHighlight(bool value);
    public void SetStartLinkable(Link link);
    public void SetEndLinkable(Link link);
    public bool IsAvailable();
    public event Action OnAvailable;
}
