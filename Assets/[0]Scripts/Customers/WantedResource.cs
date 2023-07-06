[System.Serializable]
public class WantedResource
{
    public int count;
    public int currentCount;
    public ObjectSettings type;
    public bool orderCompleted;

    public WantedResource(ObjectSettings _type, int _count)
    {
        count = _count;
        type = _type;
        orderCompleted = false;
        currentCount = 0;
    }
}
