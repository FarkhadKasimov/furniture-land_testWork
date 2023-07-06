using FactoryFramework;
using UnityEngine;
using Utils;

public class WarehouseSocket : Socket
{
    [SerializeField] private FabriqueMachine fabriqueMachine;
    [SerializeField] private GameObject indicator;
    [SerializeField] private Direction flow;

    private Conveyor _conveyor;

    public override void Connect(Object obj)
    {
        if (obj is Conveyor conveyor)
        {
            _conveyor = conveyor;
        } 
        else if (obj is FabriqueMachine machine)
        {
            fabriqueMachine = machine;
        } 
        else
        {
            Debug.LogWarning($"Object {obj} is not of either type [Conveyor,FabriqueMachine]");
        }
    }

    public override bool IsOpen()
    {
        return _conveyor == null;
    }
    private void Update()
    {
        if (fabriqueMachine == null || _conveyor == null)
        {
            return;
        }
        
        if (flow == Direction.ConveyorToBuilding)
        {
            var b = (fabriqueMachine as IInput);
            if (b != null && b.CanTakeInput(_conveyor.OutputType()) && _conveyor.CanGiveOutput())
            {
                b.TakeInput(_conveyor.GiveOutput());
            }
        }
        else if (flow == Direction.BuildingToConveyor)
        {
            var b = (fabriqueMachine as IOutput);
            if (b != null && _conveyor.CanTakeInput(b.OutputType()) && b.CanGiveOutput())
            {
                _conveyor.TakeInput(b.GiveOutput());
            }
        }
    }
}
