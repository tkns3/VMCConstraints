using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VMCConstraints
{
    public class UnityPositionConstraintObject
    {
        Transform target;

        float weight;

        Vector3 translationAtRest;

        Vector3 translationOffset;

        bool translationAxisX;

        bool translationAxisY;

        bool translationAxisZ;

        List<UnityConstraintSourceObject> sources;

        public UnityPositionConstraintObject(TransformFinder finder, UnityPositionConstraintSetting setting)
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
                if (setting.translationAtRest != null)
                {
                    translationAtRest.x = setting.translationAtRest.x;
                    translationAtRest.y = setting.translationAtRest.y;
                    translationAtRest.z = setting.translationAtRest.z;
                }
                else
                {
                    translationAtRest = target.position;
                }
                if (setting.translationOffset != null)
                {
                    translationOffset.x = setting.translationOffset.x;
                    translationOffset.y = setting.translationOffset.y;
                    translationOffset.z = setting.translationOffset.z;
                }
                else
                {
                    Vector3 sourcePosition = Vector3.zero;
                    float weightSum = 0f;
                    foreach (var src in sources)
                    {
                        sourcePosition += src.sourceRestPosition * src.weight;
                        weightSum += src.weight;
                    }
                    sourcePosition = sourcePosition / weightSum;
                    translationOffset = target.position - sourcePosition;
                }
                translationAxisX = setting.translationAxisX;
                translationAxisY = setting.translationAxisY;
                translationAxisZ = setting.translationAxisZ;
                this.sources = sources;
            }
            else
            {
                if (target == null)
                {
                    Logger.Log($"not found Transform name=\"{setting.targetName}\".", member: "UnityPositionConstraintObject");
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
            Vector3 position = translationAtRest + translationOffset;
            Vector3 sourcePosition = Vector3.zero;
            float weightSum = 0f;
            sources.ForEach(src =>
            {
                sourcePosition += src.source.position * src.weight;
                weightSum += src.weight;
            });
            if (weightSum > 0f)
            {
                sourcePosition /= weightSum;
                if (translationAxisX) position.x = sourcePosition.x + translationOffset.x;
                if (translationAxisY) position.y = sourcePosition.y + translationOffset.y;
                if (translationAxisZ) position.z = sourcePosition.z + translationOffset.z;
                position = Vector3.Lerp(target.position, position, weight);
            }

            target.position = position;
        }

        public override string ToString()
        {
            return $"target={target.name}, weight={weight}, rest={translationAtRest}, offset={translationOffset}, axisX={translationAxisX}, axisY={translationAxisY}, axisZ={translationAxisZ}, sources=[{string.Join(",", sources.Select(s => s.ToString()))}]";
        }
    }
}
