using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FactoryFramework
{
    public class ConveyorSocket : Socket
    {
        private Building building;
        public Conveyor conveyor;
        [SerializeField] private GameObject indicator;

        private void Awake()
        {
            building = GetComponentInParent<Building>();
        }

        public enum Direction
        {
            ConveyorToBuilding,
            BuildingToConveyor
        }
        public Direction flow;

        public override void Connect(Object obj)
        {
            if (obj is Conveyor)
            {
                conveyor = obj as Conveyor;
            } else if (obj is Building)
            {
                building = obj as Building;
            } else
            {
                Debug.LogWarning($"Object {obj} is not of either type [Conveyor,Building]");
            }
            
        }

        public override bool IsOpen()
        {
            return conveyor == null;
        }

        private void Update()
        {
            if (building == null || conveyor == null)
            {
                // FIXME only show indicator when hovering
                indicator.SetActive(true);
                return;
            }
            indicator.SetActive(false);

            // special handling, oops!
            if (building is Merger || building is Splitter)
            {
                // handle the logic in splitter or merger code so you can handle a "zipper" type input/output
                // or even subclass these to make smart splitters and mergers based on specific input types
                return;
            }
            

            if (flow == Direction.ConveyorToBuilding)
            {
                Debug.Assert(building is IInput, "Building does not implement IInput interface.");

                var b = (building as IInput);
                if (b.CanTakeInput(conveyor.OutputType()) && conveyor.CanGiveOutput())
                {
                    b.TakeInput(conveyor.GiveOutput());
                }
            }
            else if (flow == Direction.BuildingToConveyor)
            {
                Debug.Assert(building is IOutput, "Building does not implement IOutput interface.");

                var b = (building as IOutput);
                if (conveyor.CanTakeInput(b.OutputType()) && b.CanGiveOutput())
                {
                    conveyor.TakeInput(b.GiveOutput());
                }
            }
        }
    }
}