using UnityEngine;

namespace VMCConstraints
{
    public class UnityConstraintSourceObject
    {
        public Transform source = null;

        public float weight;

        public Vector3 sourceRestPosition;

        public Quaternion sourceRestRotation;

        public UnityConstraintSourceObject(TransformFinder finder, UnityConstraintSourceSetting setting)
        {
            var source = finder.FindTransform(setting.sourceName);
            if (source != null)
            {
                this.source = source;
                weight = setting.weight;
                sourceRestPosition = source.position;
                sourceRestRotation = source.rotation;
            }
            else
            {
                Logger.Log($"not found Transform name=\"{setting.sourceName}\".", member: "UnityConstraintSourceObject");
                this.source = null;
                weight = 0f;
                sourceRestPosition = Vector3.zero;
                sourceRestRotation = Quaternion.identity;
            }
        }

        public bool IsValid()
        {
            return source != null;
        }

        public override string ToString()
        {
            return $"{{source={source.name}, weight={weight}}}";
        }
    }
}
