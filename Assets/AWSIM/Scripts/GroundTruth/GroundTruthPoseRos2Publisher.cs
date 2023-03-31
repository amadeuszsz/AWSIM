using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROS2;

namespace AWSIM
{
    /// <summary>
    /// Convert the data output from GroundTruthPose to ROS2 msg and Publish.
    /// </summary>
    [RequireComponent(typeof(GroundTruthPose))]
    public class GroundTruthPoseRos2Publisher : MonoBehaviour
    {
        /// <summary>
        /// Topic name of PoseStamped msg.
        /// </summary>
        public string topic = "/ground_truth/pose";

        /// <summary>
        /// PoseStamped frame id.
        /// </summary>
        public string frameId = "map";

        /// <summary>
        /// QoS settings.
        /// </summary>
        public QoSSettings qosSettings = new QoSSettings()
        {
            ReliabilityPolicy = ReliabilityPolicy.QOS_POLICY_RELIABILITY_RELIABLE,
            DurabilityPolicy = DurabilityPolicy.QOS_POLICY_DURABILITY_VOLATILE,
            HistoryPolicy = HistoryPolicy.QOS_POLICY_HISTORY_KEEP_LAST,
            Depth = 10,
        };

        IPublisher<geometry_msgs.msg.PoseStamped> poseStampedPublisher;
        geometry_msgs.msg.PoseStamped poseStampedMsg;
        GroundTruthPose groundTruthPose;

        // Start is called before the first frame update
        void Start()
        {
            // Get GroundTruthPose component.
            groundTruthPose = GetComponent<GroundTruthPose>();

            // Set callback.
            groundTruthPose.OnOutputData += Publish;

            // create PoseStamped ros msg.
            poseStampedMsg = new geometry_msgs.msg.PoseStamped()
            {
                Pose = new geometry_msgs.msg.Pose()
                {
                    Position = new geometry_msgs.msg.Point(),
                    Orientation = new geometry_msgs.msg.Quaternion()
                    {
                        W = 1,
                        X = 0,
                        Y = 0,
                        Z = 0,
                    },
                },
                Header = new std_msgs.msg.Header()
                {
                    Frame_id = frameId,
                }
            };

            // Create publisher.
            var qos = qosSettings.GetQoSProfile();
            poseStampedPublisher = SimulatorROS2Node.CreatePublisher<geometry_msgs.msg.PoseStamped>(topic, qos);
        }

        void Publish(GroundTruthPose.OutputData outputData)
        {
            // Convert from Unity to ROS coordinate system and set the value to msg.
            var rosPosition = ROS2Utility.UnityToRosPosition(outputData.Position);
            var rosOrientation = ROS2Utility.UnityToRosRotation(outputData.Orientation);

            // Update msg.
            poseStampedMsg.Pose.Position.X = rosPosition.x;
            poseStampedMsg.Pose.Position.Y = rosPosition.y;
            poseStampedMsg.Pose.Position.Z = rosPosition.z;
            poseStampedMsg.Pose.Orientation.X = rosOrientation.x;
            poseStampedMsg.Pose.Orientation.Y = rosOrientation.y;
            poseStampedMsg.Pose.Orientation.Z = rosOrientation.z;
            poseStampedMsg.Pose.Orientation.W = rosOrientation.w;

            // Update msg header.
            var header = poseStampedMsg as MessageWithHeader;
            SimulatorROS2Node.UpdateROSTimestamp(ref header);

            // Publish to ROS2.
            poseStampedPublisher.Publish(poseStampedMsg);
        }

        void OnDestroy()
        {
            SimulatorROS2Node.RemovePublisher<geometry_msgs.msg.PoseStamped>(poseStampedPublisher);
        }
    }
}
