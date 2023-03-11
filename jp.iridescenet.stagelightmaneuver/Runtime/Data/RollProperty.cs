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

            bpmOverride = new SlmToggleValue<BpmOverrideToggleValueBase>()
            {
                propertyOverride = rollProperty.bpmOverride.propertyOverride,
                value = new BpmOverrideToggleValueBase(rollProperty.bpmOverride.value)
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
            bpmOverride = new SlmToggleValue<BpmOverrideToggleValueBase>(){value = new BpmOverrideToggleValueBase()};
            rollTransform = new SlmToggleValue<MinMaxEasingValue>() {value = new MinMaxEasingValue()};
        }

        public override void ToggleOverride(bool toggle)
        {
            base.ToggleOverride(toggle);
            propertyOverride = toggle;
            rollTransform.propertyOverride = toggle;
            
        }

        public override void OverwriteProperty(SlmProperty other)
        {
            base.OverwriteProperty(other);
            RollProperty rollProperty = other as RollProperty;
            if (rollProperty == null) return;
            if(rollProperty.rollTransform.propertyOverride) rollTransform.value = new MinMaxEasingValue(rollProperty.rollTransform.value);
            if(rollProperty.bpmOverride.propertyOverride) bpmOverride.value = new BpmOverrideToggleValueBase(rollProperty.bpmOverride.value);
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