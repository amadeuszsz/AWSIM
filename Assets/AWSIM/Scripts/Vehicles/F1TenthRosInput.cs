using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROS2;

namespace AWSIM
{
    /// <summary>
    /// This class subscribes to the vehicleCommand  msg output from Autoware to ROS, 
    /// and after converting the msg, it inputs it to the Vehicle class of E2ESimualtor.
    /// </summary>
    [RequireComponent(typeof(Vehicle))]
    public class F1TenthRosInput : MonoBehaviour
    {
        [SerializeField] string ackermannControlCommandTopic = "/control/command/control_cmd";
        [SerializeField] string gearCommandTopic = "/control/command/gear_cmd";
        [SerializeField] string vehicleEmergencyStampedTopic = "/control/command/emergency_cmd";

        [SerializeField] QoSSettings qosSettings = new QoSSettings();
        [SerializeField] Vehicle vehicle;

        // subscribers.
        ISubscription<autoware_auto_control_msgs.msg.AckermannControlCommand> ackermanControlCommandSubscriber;
        ISubscription<autoware_auto_vehicle_msgs.msg.GearCommand> gearCommandSubscriber;
        ISubscription<tier4_vehicle_msgs.msg.VehicleEmergencyStamped> vehicleEmergencyStampedSubscriber;

        // Latest Emergency value.
        // If emergency is true, emergencyDeceleration is applied to the vehicle's deceleration.
        // TODO: In case of reverse gear?
        bool isEmergency = false;
        float emergencyDeceleration = -3.0f; // m/s^2

        void Reset()
        {
            if (vehicle == null)
                vehicle = GetComponent<Vehicle>();

            // initialize default QoS params.
            qosSettings.ReliabilityPolicy = ReliabilityPolicy.QOS_POLICY_RELIABILITY_RELIABLE;
            qosSettings.DurabilityPolicy = DurabilityPolicy.QOS_POLICY_DURABILITY_TRANSIENT_LOCAL;
            qosSettings.HistoryPolicy = HistoryPolicy.QOS_POLICY_HISTORY_KEEP_LAST;
            qosSettings.Depth = 1;
        }

        void Start()
        {
            var qos = qosSettings.GetQoSProfile();

            ackermanControlCommandSubscriber
                = SimulatorROS2Node.CreateSubscription<autoware_auto_control_msgs.msg.AckermannControlCommand>(
                    ackermannControlCommandTopic, msg =>
                    {
                        // highest priority is EMERGENCY.
                        // If Emergency is true, ControlCommand is not used for vehicle acceleration input.
                        if (!isEmergency)
                            vehicle.AccelerationInput = msg.Longitudinal.Acceleration;

                        vehicle.SteerAngleInput = -(float)msg.Lateral.Steering_tire_angle * Mathf.Rad2Deg;
                    }, qos);

            gearCommandSubscriber
                = SimulatorROS2Node.CreateSubscription<autoware_auto_vehicle_msgs.msg.GearCommand>(
                    gearCommandTopic, msg =>
                    {
                        vehicle.AutomaticShiftInput = VehicleROS2Utility.RosToUnityShift(msg);
                    }, qos);

            vehicleEmergencyStampedSubscriber
                = SimulatorROS2Node.CreateSubscription<tier4_vehicle_msgs.msg.VehicleEmergencyStamped>(
                    vehicleEmergencyStampedTopic, msg =>
                    {
                        // highest priority is EMERGENCY.
                        // If emergency is true, emergencyDeceleration is applied to the vehicle's deceleration.
                        isEmergency = msg.Emergency;
                        if (isEmergency)
                            vehicle.AccelerationInput = emergencyDeceleration;
                    });
        }

        void OnDestroy()
        {
            SimulatorROS2Node.RemoveSubscription<autoware_auto_control_msgs.msg.AckermannControlCommand>(ackermanControlCommandSubscriber);
            SimulatorROS2Node.RemoveSubscription<autoware_auto_vehicle_msgs.msg.GearCommand>(gearCommandSubscriber);
            SimulatorROS2Node.RemoveSubscription<tier4_vehicle_msgs.msg.VehicleEmergencyStamped>(vehicleEmergencyStampedSubscriber);
        }
    }
}