﻿using UnityEngine;

namespace StageLightManeuver
{
    public class MaterialTextureProperty:SlmAdditionalProperty
    {
        public SlmToggleValue<string> texturePropertyName;
        public SlmToggleValue<int> materialindex;
        public SlmToggleValue<Texture2D> texture;
        
        public  MaterialTextureProperty()
        {
            propertyName = "Material Texture";
            bpmOverrideData = new SlmToggleValue<BpmOverrideToggleValueBase>()
                { value = new BpmOverrideToggleValueBase() };
            texturePropertyName = new SlmToggleValue<string>(){value = "_Texture"};
            materialindex = new SlmToggleValue<int>() {value = 0};
            texture = new SlmToggleValue<Texture2D>(){value = null};
        }

        public MaterialTextureProperty(MaterialTextureProperty materialTextureProperty)
        {
            propertyName = materialTextureProperty.propertyName;
            bpmOverrideData = new SlmToggleValue<BpmOverrideToggleValueBase>()
            {
                propertyOverride =  materialTextureProperty.bpmOverrideData.propertyOverride,
                value = new BpmOverrideToggleValueBase(materialTextureProperty.bpmOverrideData.value)
            };
            texturePropertyName = new SlmToggleValue<string>(materialTextureProperty.texturePropertyName);
            materialindex = new SlmToggleValue<int>(materialTextureProperty.materialindex);
            texture = new SlmToggleValue<Texture2D>()
            {
                value = materialTextureProperty.texture.value
            };
        }
        
        public override void ToggleOverride(bool toggle)
        {
            base.ToggleOverride(toggle);
            texturePropertyName.propertyOverride = toggle;
            materialindex.propertyOverride = toggle;
            texture.propertyOverride = toggle;
        }

        public override void OverwriteProperty(SlmProperty other)
        {
            base.OverwriteProperty(other);
            MaterialTextureProperty materialTextureProperty = other as MaterialTextureProperty;
            if (materialTextureProperty == null) return;
            if(materialTextureProperty.texturePropertyName.propertyOverride) texturePropertyName.value = materialTextureProperty.texturePropertyName.value;
            if(materialTextureProperty.materialindex.propertyOverride) materialindex.value = materialTextureProperty.materialindex.value;
            if(materialTextureProperty.texture.propertyOverride) texture.value = materialTextureProperty.texture.value;
            if(materialTextureProperty.bpmOverrideData.propertyOverride) bpmOverrideData.value = new BpmOverrideToggleValueBase(materialTextureProperty.bpmOverrideData.value);
        }
    }
}