namespace Mars.Seem.Data
{
    public class TreeReader
    {
        protected float DefaultCrownRatio { get; private init; }
        
        protected TreeReader()
        {
            this.DefaultCrownRatio = 0.5F;
        }
    }
}
