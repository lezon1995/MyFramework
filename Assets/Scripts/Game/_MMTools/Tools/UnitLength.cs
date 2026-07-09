using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.Tools
{
    [Serializable]
    [HideLabel]
    public struct UnitLength
    {
        [LabelText("@$property.Parent.NiceName"), SuffixLabel("$CentimeterText", Overlay = true)]
        [OnValueChanged(nameof(LimitPrecision))]
        [Unit(@base: Units.Meter, display: Units.Meter)]
        public float _meter;

        public float Meter
        {
            get => _meter;
            set => Centimeter = (int)(value * 100);
        }

        public int Centimeter
        {
            get => (int)(_meter * 100);
            set => _meter = value / 100F;
        }

        public string CentimeterText => $"{_meter * 100:F0} cm ";

        void LimitPrecision()
        {
            Meter = Mathf.Round(Meter * 100F) / 100F;
        }

        public static implicit operator float(UnitLength unit) => unit.Meter;
        public static implicit operator int(UnitLength unit) => unit.Centimeter;
        public static implicit operator UnitLength(float meter) => new() { Meter = meter };
        public static implicit operator UnitLength(int centimeter) => new() { Centimeter = centimeter };
    }
}