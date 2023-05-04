using System;
using UnityEngine;

public interface ILinkable
{
    public Transform tr { get; }
    public bool Inputable { get;}
    public bool Outputable { get;}
    
    public void AddLinkAction(MachineLink link,Action action);
    public void RemoveLinkAction(MachineLink link);
    
    public void Output(out Product product);
    public event Action<Product> OnOutput;

    public void Input(Product product);
    public event Action<Product> OnInput;
}
