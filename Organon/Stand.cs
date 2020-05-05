using System.Collections.Generic;

namespace Osu.Cof.Ferm
{
    public class Stand
    {
        public string Name { get; set; }

        public SortedDictionary<FiaCode, Trees> TreesBySpecies { get; private set; }

        public Stand()
        {
            this.Name = null;
            this.TreesBySpecies = new SortedDictionary<FiaCode, Trees>();
        }
    }
}
