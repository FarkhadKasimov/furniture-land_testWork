namespace Utils
{
    public enum ObjectType
    {
        Log
    }

    public enum CustomerState
    {
        GoToTarget,
        Wrapping,
        WaitingForBuy,
        TakeProducts,
        GoToExit
    }

    public enum ManagerType
    {
        DoubleCustomers,
        DoubleMoney
    }

    public enum WorkerType
    {
        FabriquePlacer,
        Cassier,
        None
    }

    public enum WorkerState
    {
        TakeResources,
        FillResources
    }

    public enum InstrumentType
    {
        Axe
    }

    public enum GamePlayZoneType
    {
        Resources,
        Facture,
        Shop
    }
    public enum Direction
    {
        ConveyorToBuilding,
        BuildingToConveyor
    }
    
}