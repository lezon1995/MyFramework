using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreMountains.Tools
{
    [Serializable]
    public enum VariableType
    {
        Object,
        GameObject,
        Component,
        Boolean,
        Integer,
        Float,
        String,
        Color,
        Vector2,
        Vector3,
        Vector4
    }

    [Serializable]
    public class Variable
    {
        [SerializeField]
        protected string name = "";

        [SerializeField]
        protected Object objectValue;

        [SerializeField]
        protected string dataValue;

        [SerializeField]
        protected VariableType variableType;

        public virtual string Name
        {
            get => name;
            set => name = value;
        }

        public virtual VariableType VariableType
        {
            get => variableType;
            set => variableType = value;
        }

        public virtual Type ValueType
        {
            get
            {
                return variableType switch
                {
                    VariableType.Boolean => typeof(bool),
                    VariableType.Float => typeof(float),
                    VariableType.Integer => typeof(int),
                    VariableType.String => typeof(string),
                    VariableType.Color => typeof(Color),
                    VariableType.Vector2 => typeof(Vector2),
                    VariableType.Vector3 => typeof(Vector3),
                    VariableType.Vector4 => typeof(Vector4),
                    VariableType.Object => objectValue == null ? typeof(Object) : objectValue.GetType(),
                    VariableType.GameObject => objectValue == null ? typeof(GameObject) : objectValue.GetType(),
                    VariableType.Component => objectValue == null ? typeof(Component) : objectValue.GetType(),
                    _ => throw new NotSupportedException()
                };
            }
        }

        public virtual void SetValue<T>(T value)
        {
            SetValue((object)value);
        }

        public virtual T GetValue<T>()
        {
            return (T)GetValue();
        }

        public virtual void SetValue(object value)
        {
            (dataValue, objectValue) = variableType switch
            {
                VariableType.Boolean => (DataConverter.GetString((bool)value), null),
                VariableType.Float => (DataConverter.GetString((float)value), null),
                VariableType.Integer => (DataConverter.GetString((int)value), null),
                VariableType.String => (DataConverter.GetString((string)value), null),
                VariableType.Color => (DataConverter.GetString((Color)value), null),
                VariableType.Vector2 => (DataConverter.GetString((Vector2)value), null),
                VariableType.Vector3 => (DataConverter.GetString((Vector3)value), null),
                VariableType.Vector4 => (DataConverter.GetString((Vector4)value), null),
                VariableType.Object => (null, (Object)value),
                VariableType.GameObject => (null, (GameObject)value),
                VariableType.Component => (null, (Component)value),
                _ => (dataValue, objectValue)
            };
        }

        public virtual object GetValue()
        {
            return variableType switch
            {
                VariableType.Boolean => DataConverter.ToBoolean(dataValue),
                VariableType.Float => DataConverter.ToSingle(dataValue),
                VariableType.Integer => DataConverter.ToInt32(dataValue),
                VariableType.String => DataConverter.ToString(dataValue),
                VariableType.Color => DataConverter.ToColor(dataValue),
                VariableType.Vector2 => DataConverter.ToVector2(dataValue),
                VariableType.Vector3 => DataConverter.ToVector3(dataValue),
                VariableType.Vector4 => DataConverter.ToVector4(dataValue),
                VariableType.Object => objectValue,
                VariableType.GameObject => objectValue,
                VariableType.Component => objectValue,
                _ => throw new NotSupportedException()
            };
        }
    }
}