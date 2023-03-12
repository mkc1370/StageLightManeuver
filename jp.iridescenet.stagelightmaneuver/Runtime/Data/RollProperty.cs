using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageLightManeuver
{
    
   
    [Serializable]
    public class RollProperty:SlmAdditionalProperty
    {
        [DisplayName("Roll Transform")]public SlmToggleValue<MinMaxEasingValue> rollTransform;
       
        public RollProperty(RollProperty rollProperty)
        {
            propertyName = rollProperty.propertyName;

            clockOverride = new SlmToggleValue<ClockOverrideToggleValueBase>()
            {
                propertyOverride = rollProperty.clockOverride.propertyOverride,
                value = new ClockOverrideToggleValueBase(rollProperty.clockOverride.value)
            };
            this.rollTransform = new SlmToggleValue<MinMaxEasingValue>()
            {
                propertyOverride =  rollProperty.rollTransform.propertyOverride,
                value =     new MinMaxEasingValue(rollProperty.rollTransform.value),
            };
            propertyOverride = rollProperty.propertyOverride;

        }

        public RollProperty()
        {
            propertyOverride = false;
            clockOverride = new SlmToggleValue<ClockOverrideToggleValueBase>(){value = new ClockOverrideToggleValueBase()};
            rollTransform = new SlmToggleValue<MinMaxEasingValue>() {value = new MinMaxEasingValue()};
        }

        public override void ToggleOverride(bool toggle)
        {
            base.ToggleOverride(toggle);
            propertyOverride = toggle;
            rollTransform.propertyOverride = toggle;
            clockOverride.propertyOverride = toggle;
            
        }

        public override void OverwriteProperty(SlmProperty other)
        {
            base.OverwriteProperty(other);
            RollProperty rollProperty = other as RollProperty;
            if (rollProperty == null) return;
            if(rollProperty.rollTransform.propertyOverride) rollTransform.value = new MinMaxEasingValue(rollProperty.rollTransform.value);
            if(rollProperty.clockOverride.propertyOverride) clockOverride.value = new ClockOverrideToggleValueBase(rollProperty.clockOverride.value);
        }
    }
    
    [Serializable]
    public class PanProperty:RollProperty
    {
        public PanProperty(PanProperty panProperty):base(panProperty)
        {
            propertyName = "Pan";
        }

        public PanProperty():base()
        {
            propertyName = "Pan";
        }
    }
    [Serializable]
    public class TiltProperty:RollProperty
    {
        public TiltProperty(TiltProperty tiltProperty):base(tiltProperty)
        {
            
            propertyName = "Tilt";
        }

        public TiltProperty():base()
        {
            
            propertyName = "Tilt";
        }
    }

}