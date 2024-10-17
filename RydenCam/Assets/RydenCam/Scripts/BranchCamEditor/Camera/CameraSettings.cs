using RydenCam.Common;
using System.Collections.Generic;

namespace RydenCam.BranchCamEditor.BranchCam
{
    //User will be able to apply different float values to specific different built-in shots
    public class CameraSettings
    {
        public float DefaultHeight;
        public float DefaultOrbitAngle;
        public float DefaultBiasX;

        public float PortaitDistance_Close;
        public float PortraitDistance_Mid;
        public float PortraitDistance_Far;

        public float FrameShareDistance_Close;
        public float FrameShareDistance_Mid;
        public float FrameShareDistance_Far;

        public float OverShoulderDistance_Close;
        public float OverShoulderDistance_Mid;
        public float OverShoulderDistance_Far;

        public float AngleEyeLevel;
        public float AngleLow;
        public float AngleHigh;


        public Dictionary<CameraGoal, Dictionary<CameraDistance, float>> DistanceSettings { get; private set; }
        public Dictionary<CameraAngle, float> AngleSettings
        {
            get; private set;
        }

        public CameraSettings()
        {
            SetDefaultCompositionValues();
            InitializeDistanceSettings();
            InitializeAngleSettings();
        }

        //TODO
        //Probably need to figure how to to persist variables changes when 
        //these are exposed in script and user changes them
        public void SetDefaultCompositionValues()
        {
            //Default Comp Values
            DefaultHeight = 0.0f;
            DefaultOrbitAngle = 30.0f;
            DefaultBiasX = 0.0f;

            PortaitDistance_Close = 0.5f;
            PortraitDistance_Mid = 1.0f;
            PortraitDistance_Far = 3.0f;

            FrameShareDistance_Close = 0.0f;
            FrameShareDistance_Mid = 1.5f;
            FrameShareDistance_Far = 5.0f;

            OverShoulderDistance_Close = 0.5f;
            OverShoulderDistance_Mid = 1.2f;
            OverShoulderDistance_Far = 2.0f;

            AngleEyeLevel = 0.0f;
            AngleLow = -1.0f;
            AngleHigh = 3.0f;
        }

        public float GetDistance(CamShotConfig shot)
        {
            return DistanceSettings[shot.GoalType][shot.GoalDistance];
        }

        public float GetAngle(CamShotConfig shot)
        {
            return AngleSettings[shot.GoalAngle];
        }

        private void InitializeDistanceSettings()
        {
            DistanceSettings = new Dictionary<CameraGoal, Dictionary<CameraDistance, float>>
        {
            { CameraGoal.Portrait, new Dictionary<CameraDistance, float>
                {
                    { CameraDistance.Close, PortaitDistance_Close },
                    { CameraDistance.Mid, PortraitDistance_Mid },
                    { CameraDistance.Far, PortraitDistance_Far }
                }
            },
            { CameraGoal.FrameShare, new Dictionary<CameraDistance, float>
                {
                    { CameraDistance.Close, FrameShareDistance_Close },
                    { CameraDistance.Mid, FrameShareDistance_Mid },
                    { CameraDistance.Far, FrameShareDistance_Far }
                }
            },
            { CameraGoal.OverShoulder, new Dictionary<CameraDistance, float>
                {
                    { CameraDistance.Close, OverShoulderDistance_Close },
                    { CameraDistance.Mid, OverShoulderDistance_Mid },
                    { CameraDistance.Far, OverShoulderDistance_Far }
                }
            }
        };
        }

        private void InitializeAngleSettings()
        {
            AngleSettings = new Dictionary<CameraAngle, float>
        {
            { CameraAngle.EyeLevel, AngleEyeLevel },
            { CameraAngle.Low, AngleLow },
            { CameraAngle.High, AngleHigh }
        };
        }
    }
}
