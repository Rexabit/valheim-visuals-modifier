using System;
using UnityEngine;

namespace VisualsModifier
{
    internal interface IRealtimeApplicator
    {
        void Apply(Transform transform, GameObject prefab);
    }

    public class RealtimeApplicator: IRealtimeApplicator
    {
        private Type _type;

        public RealtimeApplicator(Type type)
        {
            _type = type;
        }

        public void Apply(Transform transform, GameObject prefab)
        {
            object te = transform.gameObject.GetComponent(_type);

            if (te != null)
            {
                //((IRealtimeEffector) te).Reset();
            } else
            {
                te = transform.gameObject.AddComponent(_type);
            }

            IRealtimeEffector effector = (IRealtimeEffector)te;

            effector.SetVisuals(VisualController.GetVisualIndex(prefab.name));
        }
    }
}