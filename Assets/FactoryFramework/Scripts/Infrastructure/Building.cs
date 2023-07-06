using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public class Building : LogisticComponent
    {
        // public LocalStorage input;
        // public LocalStorage output;

        private void Update()
        {
            this.ProcessLoop();
        }

        protected Recipe[] GetAllRecipes()
        {
            return Resources.LoadAll<Recipe>("");
        }
    }
}