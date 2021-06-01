using System.Threading;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PseudorandomizingTask
    {
        private readonly ThreadLocal<Pseudorandom> pseudorandom;
        
        protected PseudorandomizingTask()
        {
            this.pseudorandom = new(() => new Pseudorandom());
        }

        protected Pseudorandom Pseudorandom 
        { 
            get { return this.pseudorandom.Value!; }
        }
    }
}
