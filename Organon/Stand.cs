﻿using System.Collections.Generic;

namespace Osu.Cof.Ferm
{
    public class Stand
    {
        public SortedDictionary<FiaCode, Trees> TreesBySpecies { get; private set; }

        public Stand()
        {
            this.TreesBySpecies = new SortedDictionary<FiaCode, Trees>();
        }
    }
}
