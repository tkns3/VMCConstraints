using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VMCConstraints
{
    public class UnityRotationConstraintObject
    {
        Transform target;

        float weight;

        bool rotationAxisX;

        bool rotationAxisY;

        bool rotationAxisZ;

        List<UnityConstraintSourceObject> sources;

        Quaternion rotationAtRest;

        Quaternion rotationOffset;

        public UnityRotationConstraintObject(TransformFinder finder, UnityRotationConstraintSetting setting)
        {
            var target = finder.FindTransform(setting.targetName);
            var sources = new List<UnityConstraintSourceObject>();
            foreach (var settingSource in setting.sources)
            {
                sources.Add(new UnityConstraintSourceObject(finder, settingSource));
            }
            if (setting.enableVMC && target != null && sources.All(source => source.IsValid()))
            {
                this.target = target;
                weight = setting.weight;
                if (setting.rotationAtRest != null)
                {
                    rotationAtRest = Quaternion.Euler(setting.rotationAtRest.x, setting.rotationAtRest.y, setting.rotationAtRest.z);
                }
                else
                {
                    rotationAtRest = Quaternion.identity;
                }
                if (setting.rotationOffset != null)
                {
                    rotationOffset = Quaternion.Euler(setting.rotationOffset.x, setting.rotationOffset.y, setting.rotationOffset.z);
                }
                else
                {
                    rotationOffset = Quaternion.identity;
                }
                rotationAxisX = setting.rotationAxisX;
                rotationAxisY = setting.rotationAxisY;
                rotationAxisZ = setting.rotationAxixZ;
                this.sources = sources;
                rotationAtRest = target.rotation;
            }
            else
            {
                if (target == null)
                {
                    Logger.Log($"not found Transform name=\"{setting.targetName}\".", member: "UnityRotationConstraintObject");
                }
                this.target = null;
            }
        }

        public bool IsValid()
        {
            return target != null;
        }

        public void Update()
        {
            Quaternion targetRotation = Quaternion.identity;

            sources.ForEach(src =>
            {
                targetRotation *= Quaternion.Slerp(rotationAtRest, src.source.rotation, src.weight * weight);
            });

            Vector3 euler = rotationAtRest.eulerAngles;
            if (rotationAxisX) euler.x = targetRotation.eulerAngles.x;
            if (rotationAxisY) euler.y = targetRotation.eulerAngles.y;
            if (rotationAxisZ) euler.z = targetRotation.eulerAngles.z;
            targetRotation = Quaternion.Euler(euler);

            target.rotation = targetRotation;
        }

        public override string ToString()
        {
            return $"target={target.name}, weight={weight}, rest={rotationAtRest.eulerAngles}, offset={rotationOffset.eulerAngles}, axisX={rotationAxisX}, axisY={rotationAxisY}, axisZ={rotationAxisZ}, sources=[{string.Join(",", sources.Select(s => s.ToString()))}]";
        }
    }
}
