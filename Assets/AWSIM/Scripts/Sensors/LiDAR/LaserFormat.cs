// Copyright 2022 Robotec.ai.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using RGLUnityPlugin;
using std_msgs.msg;
using sensor_msgs.msg;

/// <summary>
/// Following class describes the laser scan for ROS2 and RGL.
/// </summary>
namespace AWSIM.LaserFormat
{
    /// <summary>
    /// Laser scan format used by ROS 2
    /// </summary>
    public static class FormatLaserScan
    { 
        public static RGLField[] GetRGLFields()
        {
            return new[]
            {
                RGLField.DISTANCE_F32,
                RGLField.RAY_IDX_U32,
            };
        }
        public static LaserScan GetSensorMsg()
        {
            return new LaserScan()
            {
                Header = new std_msgs.msg.Header(), 
                Angle_min = 0.0f,
                Angle_max = 0.0f,
                Angle_increment = 0.0f,
                Time_increment = 0.0f,
                Scan_time = 0.0f,
                Range_min = 0.0f,
                Range_max = 0.0f,
                Ranges = null,
                Intensities = new float[0],
            };
        }
    }
}