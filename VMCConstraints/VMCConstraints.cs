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
    [VMCPlugin(
    Name: "VMCConstraints",
    Version: "0.1.0",
    Author: "sato",
    Description: "Constraintを追加する",
    AuthorURL: "https://twitter.com/sato_310_jp",
    PluginURL: "https://github.com/tkns3/VMCConstraints")]
    public class VMCConstraints : MonoBehaviour
    {
        private VRIK _vrik;
        private IKSolver _ikSolver;
        private GameObject _currentModel;

        private static readonly string debugFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @".\VMCConstraintsDebug.json");
        private static readonly string configurationFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @".\VMCConstraints.json");
        private List<Vrm10RollConstraintObject> _vrm10RollConstraintObjects;
        private List<Vrm10RotationConstraintObject> _vrm10RotationConstraintObjects;
        private List<UnityPositionConstraintObject> _unityPositionConstraintObjects;
        private List<UnityRotationConstraintObject> _unityRotationConstraintObjects;

        private void Awake()
        {
            VMCEvents.OnModelUnloading += OnModelUnloading;
            VMCEvents.OnCurrentModelChanged += OnCurrentModelChanged;
            VMCEvents.OnModelLoaded += OnModelLoaded;
        }

        void Start()
        {
            var debugInfo = DeserializeObject<DebugInfo>(debugFile);
            if (debugInfo != null)
            {
                Logger.AddOSC(gameObject.AddComponent<uOSC.uOscClient>(), debugInfo.address, debugInfo.port);
            }

            Logger.Log("");

            _vrm10RollConstraintObjects = new List<Vrm10RollConstraintObject>();
            _vrm10RotationConstraintObjects = new List<Vrm10RotationConstraintObject>();
            _unityPositionConstraintObjects = new List<UnityPositionConstraintObject>();
            _unityRotationConstraintObjects = new List<UnityRotationConstraintObject>();
        }

        private void Update()
        {
            if (_vrik == null && _currentModel != null)
            {
                if (_ikSolver != null) _ikSolver.OnPostUpdate -= OnPostUpdate;
                _vrik = _currentModel.GetComponent<VRIK>();
                if (_vrik == null) return;
                _ikSolver = _vrik.GetIKSolver();
                _ikSolver.OnPostUpdate += OnPostUpdate;
            }
        }

        private void OnPostUpdate()
        {
            _vrm10RollConstraintObjects.ForEach(cObj => cObj.Update());
            _vrm10RotationConstraintObjects.ForEach(cObj => cObj.Update());
            _unityPositionConstraintObjects.ForEach(cObj => cObj.Update());
            _unityRotationConstraintObjects.ForEach(cObj => cObj.Update());
        }

        [OnSetting]
        public void OnSetting()
        {
        }

        private void OnModelUnloading(GameObject model)
        {
            if (model == null) return;
            var meta = model.GetComponent<VRM.VRMMeta>();
            Logger.Log($"{model.name}, {meta.Meta.Title}, {meta.Meta.Version}");
            UnloadModel();
        }

        private void UnloadModel()
        {
            if (_currentModel != null)
            {
                _vrm10RollConstraintObjects.Clear();
                _vrm10RotationConstraintObjects.Clear();
                _unityPositionConstraintObjects.Clear();
                _unityRotationConstraintObjects.Clear();
                _vrik = null;
                if (_ikSolver != null)
                {
                    _ikSolver.OnPostUpdate -= OnPostUpdate;
                    _ikSolver = null;
                }
                _currentModel = null;
            }
        }

        private void OnCurrentModelChanged(GameObject model)
        {
            if (model == null) return;
            var meta = model.GetComponent<VRM.VRMMeta>();
            Logger.Log($"{model.name}, {meta.Meta.Title}, {meta.Meta.Version}");
        }

        private void OnModelLoaded(GameObject model)
        {
            if (model == null) return;

            var meta = model.GetComponent<VRM.VRMMeta>();
            Logger.Log($"{model.name}, {meta.Meta.Title}, {meta.Meta.Version}");

            var vrmMetaKey = $"{meta.Meta.Title}_{meta.Meta.Version}";

            UnloadModel();

            if (!File.Exists(configurationFile))
            {
                var json = JsonConvert.SerializeObject(new VMCConstraintsConfiguration());
                File.WriteAllText(configurationFile, json);
            }
            var configuration = DeserializeObject<VMCConstraintsConfiguration>(configurationFile);
            if (configuration == null)
            {
                Logger.Log($"\"{configurationFile}\" deserialize failed.");
                return;
            }

            if (!configuration.List.ContainsKey(vrmMetaKey))
            {
                Logger.Log($"not found key=\"{vrmMetaKey}\".");
                return;
            }
            Logger.Log($"found key=\"{vrmMetaKey}\".");

            if (!File.Exists(configuration.List[vrmMetaKey].FilePath))
            {
                Logger.Log($"not found file=\"{configuration.List[vrmMetaKey].FilePath}\".");
                return;
            }

            var avatarSetting = DeserializeObject<AvatarSetting>(configuration.List[vrmMetaKey].FilePath);
            if (avatarSetting == null)
            {
                Logger.Log($"\"{configuration.List[vrmMetaKey].FilePath}\" deserialize failed.");
                return;
            }

            if (!avatarSetting.enableVMC)
            {
                Logger.Log($"enableVMC is false.");
                return;
            }

            var finder = new TransformFinder(model);

            avatarSetting.vrm10RollConstraintList.ForEach(setting =>
            {
                var cObj = new Vrm10RollConstraintObject(finder, setting);
                if (cObj.IsValid())
                {
                    _vrm10RollConstraintObjects.Add(cObj);
                    Logger.Log($"Vrm10RollConstraintObject={{{cObj}}}.");
                }
            });

            avatarSetting.vrm10RotationConstraintList.ForEach(setting =>
            {
                var cObj = new Vrm10RotationConstraintObject(finder, setting);
                if (cObj.IsValid())
                {
                    _vrm10RotationConstraintObjects.Add(cObj);
                    Logger.Log($"Vrm10RotationConstraintObject={{{cObj}}}.");
                }
            });

            avatarSetting.unityPositionConstraintList.ForEach(setting =>
            {
                var cObj = new UnityPositionConstraintObject(finder, setting);
                if (cObj.IsValid())
                {
                    _unityPositionConstraintObjects.Add(cObj);
                    Logger.Log($"UnityPositionConstraintObject={{{cObj}}}.");
                }
            });

            avatarSetting.unityRotationConstraintList.ForEach(setting =>
            {
                var cObj = new UnityRotationConstraintObject(finder, setting);
                if (cObj.IsValid())
                {
                    _unityRotationConstraintObjects.Add(cObj);
                    Logger.Log($"UnityRotationConstraintObject={{{cObj}}}.");
                }
            });

            _currentModel = model;
        }

        private T DeserializeObject<T>(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                Logger.Log($"not found file=\"{jsonFilePath}\"");
                return default;
            }

            try
            {
                string jsonText = File.ReadAllText(jsonFilePath);
                var obj = JsonConvert.DeserializeObject<T>(jsonText);
                return obj;
            }
            catch (System.Exception e)
            {
                Logger.Log($"{e.Message}");
                return default;
            }
        }

        class DebugInfo
        {
            public string address { get; set; } = "127.0.0.1";

            public int port { get; set; } = 3333;
        }
    }

    public static class Logger
    {
        public static uOSC.uOscClient _cli = null;

        public static void AddOSC(uOSC.uOscClient cli, string address = "127.0.0.1", int port = 3333)
        {
            _cli = cli;
            if (_cli != null)
            {
                var type = typeof(uOSC.uOscClient);
                var addressfield = type.GetField("address", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
                addressfield.SetValue(_cli, address);
                var portfield = type.GetField("port", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
                portfield.SetValue(_cli, port);
                _cli.enabled = true;
            }
        }

        public static void Log(string message,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            Debug.Log($"VMCConstraints[{line}, {member}] {message}");
            _cli?.Send("/VMCConstraints", line, member, message);
        }
    }

    public class TransformFinder
    {
        Transform[] _children;

        public TransformFinder(GameObject model)
        {
            _children = model.GetComponentsInChildren<Transform>();
        }

        public Transform FindTransform(string name)
        {
            return _children.FirstOrDefault(x => x.name.Equals(name));
        }
    }
}
