﻿using System.Collections.Generic;

namespace LeanCode.ViewRenderer
{
    public class ViewRendererOptions
    {
        internal IList<string> ViewLocations { get; }

        public ViewRendererOptions()
        {
            ViewLocations = new List<string>();
        }

        public void AddViewLocation(string viewLocation)
        {
            ViewLocations.Add(viewLocation);
        }
    }
}