using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AWSIM
{
    /// <summary>
    /// Pose ground truth.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GroundTruthPose : MonoBehaviour
    {
        /// <summary>
        /// This data is output from GroundTruthPose at the OutputHz cycle.
        /// </summary>
        public class OutputData
        {
            /// <summary>
            /// Ground truth position (m)
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Ground truth orientation (quaternion)
            /// </summary>
            public Quaternion Orientation;

            public OutputData()
            {
                Position = new Vector3();
                Orientation = new Quaternion();
            }
        }

        /// <summary>
        /// Data output hz.
        /// Data processing and callbacks are called in this hz.
        /// </summary>
        [Range(0, 50)]
        public int OutputHz = 30;   // Autoware's sensors basically output at 30hz.

        /// <summary>
        /// Delegate used in callbacks.
        /// </summary>
        /// <param name="outputData">Data output for each hz</param>
        public delegate void OnOutputDataDelegate(OutputData outputData);

        /// <summary>
        /// Called each time data is output.
        /// </summary>
        public OnOutputDataDelegate OnOutputData;

        Rigidbody rigidbody;

        float timer = 0;
        OutputData outputData = new OutputData();

        void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            // Update timer.
            timer += Time.deltaTime;

            // Matching output to hz.
            var interval = 1.0f / OutputHz;
            interval -= 0.00001f;       // Allow for accuracy errors.
            if (timer < interval)
                return;
            timer = 0;

            // Update output data.
            outputData.Position.x = rigidbody.position.x;
            outputData.Position.y = rigidbody.position.y;
            outputData.Position.z = rigidbody.position.z;
            outputData.Orientation.x = rigidbody.rotation.x;
            outputData.Orientation.y = rigidbody.rotation.y;
            outputData.Orientation.z = rigidbody.rotation.z;
            outputData.Orientation.w = rigidbody.rotation.w;

            // Calls registered callbacks
            OnOutputData.Invoke(outputData);
        }
    }
}