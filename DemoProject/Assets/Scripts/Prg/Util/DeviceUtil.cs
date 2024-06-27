using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prg.Util
{
    /// <summary>
    /// Convenience class to check is we are running "inside" a simulator (in Editor).
    /// </summary>
    public class DeviceUtil : MonoBehaviour
    {
        public static readonly List<string> DeviceNames = new();

        public static string SimulatorDeviceName { get; private set; }

        public static bool IsSimulatorDevice { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SubsystemRegistration()
        {
            CheckSimulator();
        }

        public static bool IsSimulator
        {
            get
            {
                if (DeviceNames.Count == 0)
                {
                    CheckSimulator();
                }
                return IsSimulatorDevice;
            }
        }

        public static void RequireSimulator()
        {
            if (IsSimulatorDevice)
            {
                return;
            }
            Debug.Log($"Devices: {string.Join(", ", DeviceNames)}");
            Debug.LogError($"Must run inside a simulator: {SimulatorDeviceName}");
        }

        public static void DoNotRequireSimulator()
        {
            if (!IsSimulatorDevice)
            {
                return;
            }
            Debug.Log($"Devices: {string.Join(", ", DeviceNames)}");
            Debug.LogError($"Can not run inside a simulator: {SimulatorDeviceName}");
        }

        private static void CheckSimulator()
        {
            DeviceNames.Clear();
            foreach (var device in InputSystem.devices)
            {
                DeviceNames.Add(device.name);
            }
            DeviceNames.Sort();
            SimulatorDeviceName = DeviceNames.Find(x => x.Contains("Simulated")) ??
                                  DeviceNames.Find(x => x.Contains("Simulator"));
            IsSimulatorDevice = SimulatorDeviceName != null;
            if (IsSimulatorDevice)
            {
                Debug.Log($"device {SimulatorDeviceName}");
            }
        }
    }
}
