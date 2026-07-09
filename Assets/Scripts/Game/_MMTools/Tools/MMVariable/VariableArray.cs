using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [Serializable]
    public class VariableArray
    {
        [SerializeField]
        List<Variable> variables = new();

        public List<Variable> Variables => variables;
        public Variable this[int index] => variables[index];


        public Variable this[string name]
        {
            get
            {
                if (variables is not {Count: > 0})
                    return null;

                foreach (var v in variables)
                {
                    if (v.Name == name)
                        return v;
                }

                return null;
            }
        }

        Dictionary<string, object> _dict = new();

        public object Get(string name)
        {
            if (variables is not {Count: > 0})
                return null;

            if (_dict.TryGetValue(name, out var value))
                return value;

            foreach (var v in variables)
            {
                if (v.Name == name)
                {
                    var o = v.GetValue();
                    _dict[name] = o;
                    return o;
                }
            }

            return null;
        }

        public T Get<T>(string name)
        {
            if (variables is not {Count: > 0})
                return default;

            if (_dict.TryGetValue(name, out var value))
                return (T) value;

            foreach (var v in variables)
            {
                if (v.Name == name)
                {
                    var o = v.GetValue<T>();
                    _dict[name] = o;
                    return o;
                }
            }

            return default;
        }

        public bool Get<T>(string name, out T result)
        {
            result = default;
            if (variables is not {Count: > 0})
                return false;

            if (_dict.TryGetValue(name, out var value))
            {
                result = (T) value;
                return true;
            }

            foreach (var v in variables)
            {
                if (v.Name == name)
                {
                    result = v.GetValue<T>();
                    _dict[name] = result;
                    return true;
                }
            }

            return false;
        }

        public static implicit operator List<Variable>(VariableArray array)
        {
            return array.variables;
        }

        public static implicit operator VariableArray(List<Variable> variables)
        {
            return new VariableArray() {variables = variables};
        }

        public void AddVariable(string name, Component component)
        {
            var v = new Variable();
            v.Name = name;
            v.VariableType = VariableType.Component;
            v.SetValue(component);
            variables.Add(v);
        }
    }
}