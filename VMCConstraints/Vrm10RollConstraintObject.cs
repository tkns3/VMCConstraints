using Newtonsoft.Json;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using VMC;
using VMCMod;

namespace VMCConstraints
{
    public class Vrm10RollConstraintObject
    {
        Transform target;

        Transform source;

        float weight;

        string rollAxis; // "X", "Y", "Z"

        Quaternion sourceLocalRotationAtRest;

        Quaternion targetLocalRotationAtRest;

        public Vrm10RollConstraintObject(TransformFinder finder, VRM10RollConstraintSetting setting)
        {
            var target = finder.FindTransform(setting.targetName);
            var source = finder.FindTransform(setting.sourceName);
            if (setting.enableVMC && target != null && source != null)
            {
                this.target = target;
                this.source = source;
                weight = setting.weight;
                rollAxis = setting.rollAxis;
                sourceLocalRotationAtRest = source.localRotation;
                targetLocalRotationAtRest = target.localRotation;
            }
            else
            {
                if (target == null)
                {
                    Logger.Log($"not found Transform name=\"{setting.targetName}\".", member: "Vrm10RollConstraintObject");
                }
                if (source == null)
                {
                    Logger.Log($"not found Transform name=\"{setting.sourceName}\".", member: "Vrm10RollConstraintObject");
                }
                this.target = null;
                this.source = null;
                weight = 0;
                rollAxis = "";
                sourceLocalRotationAtRest = Quaternion.identity;
                targetLocalRotationAtRest = Quaternion.identity;
            }
        }

        public bool IsValid()
        {
            return target != null;
        }

        public void Update()
        {
            Vector3 rollAxis = Vector3.right;
            switch (this.rollAxis)
            {
                case "X": rollAxis = Vector3.right; break;
                case "Y": rollAxis = Vector3.up; break;
                case "Z": rollAxis = Vector3.forward; break;
                default: break;
            }

            var deltaSrcQuat = Quaternion.Inverse(sourceLocalRotationAtRest) * source.localRotation;
            var deltaSrcQuatInParent = sourceLocalRotationAtRest * deltaSrcQuat * Quaternion.Inverse(sourceLocalRotationAtRest); // source to parent
            var deltaSrcQuatInDst = Quaternion.Inverse(targetLocalRotationAtRest) * deltaSrcQuatInParent * targetLocalRotationAtRest; // parent to destination
            var toVec = deltaSrcQuatInDst * rollAxis;
            var fromToQuat = Quaternion.FromToRotation(rollAxis, toVec);
            target.localRotation = Quaternion.SlerpUnclamped(
                targetLocalRotationAtRest,
                targetLocalRotationAtRest * Quaternion.Inverse(fromToQuat) * deltaSrcQuatInDst,
                weight
            );
        }

        public override string ToString()
        {
            return $"target={target.name}, source={source.name}, weight={weight}, rollAxis={rollAxis}";
        }
    }
}
