namespace FactoryFramework
{
    public interface IInput
    {
        public void TakeInput(Item item, CollectableObject resource = null);
        public bool CanTakeInput(Item item, CollectableObject resource = null);
    }
}