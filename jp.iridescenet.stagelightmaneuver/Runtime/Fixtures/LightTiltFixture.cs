using UnityEngine;

namespace StageLightManeuver
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    public class LightTiltFixture: StageLightFixtureBase
    {
        private LightTransformType _lightTransformType = LightTransformType.Tilt;
        private float _angle =0f;
        public Vector3 rotationVector = Vector3.left;
        public Transform rotateTransform;
        public override void EvaluateQue(float currentTime)
        {
            base.EvaluateQue(currentTime);
            if(rotateTransform == null) return;
            _angle = 0f;
            while (stageLightDataQueue.Count>0)
            {
                var queueData = stageLightDataQueue.Dequeue();
                var qTiltProperty = queueData.TryGet<TiltProperty>() as TiltProperty;
                var timeProperty = queueData.TryGet<TimeProperty>() as TimeProperty;
                var weight = queueData.weight;
                if (qTiltProperty == null || timeProperty == null) continue;
                var normalizedTime = GetNormalizedTime(currentTime, queueData, typeof(TiltProperty));
                var manualPanTiltProperty = queueData.TryGet<ManualPanTiltProperty>();
                if(manualPanTiltProperty != null)
                {
                    var positions = manualPanTiltProperty.positions.value;
                    var mode = manualPanTiltProperty.mode.value;
                    if (Index < positions.Count)
                    {
                        switch (mode)
                        {
                            case ManualPanTiltMode.Overwrite:
                                _angle += positions[Index].tilt * weight;
                                break;
                            case ManualPanTiltMode.Add:
                                _angle += (positions[Index].tilt+qTiltProperty.rollTransform.value.Evaluate(normalizedTime)) * weight;
                                break;
                        }
                        // Debug.Log($"tilt({Index}): {positions[Index].tilt}, weight: {weight}");
                    }
                }
                else
                {
                    _angle += qTiltProperty.rollTransform.value.Evaluate(normalizedTime) * weight;
                }

            }
        }
        
        public override void UpdateFixture()
        {
            rotateTransform.localEulerAngles =  rotationVector * _angle;
        }
    }
}