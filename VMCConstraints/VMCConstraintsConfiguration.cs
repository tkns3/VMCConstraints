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
    public class VMCConstraintsConfiguration
    {
        public Dictionary<string, AvatarSettingPath> List { get; set; }
    }

    public class AvatarSettingPath
    {
        public string FilePath { get; set; }
    }

    public class AvatarSetting
    {
        public bool enableVMC { get; set; } = false;

        public List<VRM10RollConstraintSetting> vrm10RollConstraintList { get; set; } = new List<VRM10RollConstraintSetting>();

        public List<VRM10RotationConstraintSetting> vrm10RotationConstraintList { get; set; } = new List<VRM10RotationConstraintSetting>();

        public List<UnityPositionConstraintSetting> unityPositionConstraintList { get; set; } = new List<UnityPositionConstraintSetting>();

        public List<UnityRotationConstraintSetting> unityRotationConstraintList { get; set; } = new List<UnityRotationConstraintSetting>();
    }

    public class VRM10RollConstraintSetting
    {
        public bool enableVMC { get; set; } = false;

        public bool enableBS { get; set; } = false;

        public string targetName { get; set; }

        public string sourceName { get; set; }

        public float weight { get; set; }

        public string rollAxis { get; set; } // "X", "Y", "Z"
    }

    public class VRM10RotationConstraintSetting
    {
        public bool enableVMC { get; set; } = false;

        public bool enableBS { get; set; } = false;

        public string targetName { get; set; }

        public string sourceName { get; set; }

        public float weight { get; set; }
    }

    public class UnityPositionConstraintSetting
    {
        public bool enableVMC { get; set; } = false;

        public bool enableBS { get; set; } = false;

        public string targetName { get; set; }

        public float weight { get; set; }

        public XYZ translationAtRest { get; set; } = null;

        public XYZ translationOffset { get; set; } = null;

        public bool translationAxisX { get; set; } = false;

        public bool translationAxisY { get; set; } = false;

        public bool translationAxisZ { get; set; } = false;

        public List<UnityConstraintSourceSetting> sources { get; set; } = new List<UnityConstraintSourceSetting>();
    }

    public class UnityRotationConstraintSetting
    {
        public bool enableVMC { get; set; } = false;

        public bool enableBS { get; set; } = false;

        public string targetName { get; set; }

        public float weight { get; set; }

        public XYZ rotationAtRest { get; set; } = null;

        public XYZ rotationOffset { get; set; } = null;

        public bool rotationAxisX { get; set; } = false;

        public bool rotationAxisY { get; set; } = false;

        public bool rotationAxixZ { get; set; } = false;

        public List<UnityConstraintSourceSetting> sources { get; set; } = new List<UnityConstraintSourceSetting>();
    }

    public class XYZ
    {
        public float x { get; set; }

        public float y { get; set; }

        public float z { get; set; }
    }


    public class UnityConstraintSourceSetting
    {
        public string sourceName { get; set; }

        public float weight { get; set; }
    }
}
