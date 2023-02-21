﻿
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace StageLightManeuver
{
    [Serializable]
    public class RotationProperty : SlmAdditionalProperty
    {
        // public SlmToggleValue<Vector3> rotationAxis;
        [DisplayName("Rotation Speed")] public SlmToggleValue<MinMaxEasingValue> rotationSpeed;
        public RotationProperty()
        {
            propertyName = "Rotation";

            propertyOverride = false;
            bpmOverrideData = new SlmToggleValue<BpmOverrideToggleValueBase>()
                { value = new BpmOverrideToggleValueBase() };
            // rotationAxis = new SlmToggleValue<Vector3>(){value = new Vector3(0,0,1)};
            rotationSpeed = new SlmToggleValue<MinMaxEasingValue>()
            {
                value = new MinMaxEasingValue(AnimationMode.Constant, new Vector2(-30,30), new Vector2(-40,40), EaseType.Linear,30, new AnimationCurve(new []{new Keyframe(0,0),new Keyframe(1,40)}))
            };
        }
        
        public override void ToggleOverride(bool toggle)
        {
            propertyOverride = toggle;
            // rotationAxis.propertyOverride=(toggle);
            rotationSpeed.propertyOverride=(toggle);
            bpmOverrideData.propertyOverride=(toggle);
        }
        
        public RotationProperty(RotationProperty other)
        {
            propertyName = other.propertyName;
            propertyOverride = other.propertyOverride;
            bpmOverrideData = new SlmToggleValue<BpmOverrideToggleValueBase>()
            {
                propertyOverride =  other.bpmOverrideData.propertyOverride,
                value = new BpmOverrideToggleValueBase(other.bpmOverrideData.value)
            };
            // rotationAxis = new SlmToggleValue<Vector3>(other.rotationAxis){};
            rotationSpeed = new SlmToggleValue<MinMaxEasingValue>()
            {
                propertyOverride = other.rotationSpeed.propertyOverride,
                value = new MinMaxEasingValue(other.rotationSpeed.value)
            };
        }

    }
}