using UnityEngine;

namespace VisualsModifier
{
    public abstract class MaterialEffect<T>
    {
        protected string Name;
        protected T Value;

        public MaterialEffect(string name, T value) {
            Name = name;
            Value = value;
        }
    }
}
