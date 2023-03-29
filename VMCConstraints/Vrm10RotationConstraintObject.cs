using System.Linq;
using UnityEngine;

namespace VMCConstraints
{
    public class Vrm10RotationConstraintObject
    {
        Transform target;

        Transform source;

        float weight;

        Quaternion sourceLocalRotationInverseAtRest;

        Quaternion targetLocalRotationAtRest;

        public Vrm10RotationConstraintObject(TransformFinder finder, VRM10RotationConstraintSetting setting)
        {
            var target = finder.FindTransform(setting.targetName);
            var source = finder.FindTransform(setting.sourceName);
            if (setting.enableVMC && target != null && source != null)
            {
                this.target = target;
                this.source = source;
                weight = setting.weight;
                sourceLocalRotationInverseAtRest = Quaternion.Inverse(source.localRotation);
                targetLocalRotationAtRest = target.localRotation;
            }
            else
            {
                if (target == null)
                {
                    Logger.Log($"not found Transform name=\"{setting.targetName}\".", member: "Vrm10RotationConstraintObject");
                }
                if (source == null)
                {
                    Logger.Log($"not found Transform name=\"{setting.sourceName}\".", member: "Vrm10RotationConstraintObject");
                }
                this.target = null;
                this.source = null;
                weight = 0;
                sourceLocalRotationInverseAtRest = Quaternion.identity;
                targetLocalRotationAtRest = Quaternion.identity;
            }
        }

        public bool IsValid()
        {
            return target != null;
        }

        public void Update()
        {
            var srcDeltaLocalQuat = sourceLocalRotationInverseAtRest * source.localRotation;
            target.localRotation = Quaternion.SlerpUnclamped(targetLocalRotationAtRest, targetLocalRotationAtRest * srcDeltaLocalQuat, weight);
        }

        public override string ToString()
        {
            return $"target={target.name}, source={source.name}, weight={weight}";
        }
    }
}
