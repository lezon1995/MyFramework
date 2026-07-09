using System;
using System.Collections.Generic;
using System.Text;

namespace UniStats
{
    [Serializable]
    public class NumMod<T> : Mod<T> where T : struct
#if NET7_0_OR_GREATER
        where T : System.Numerics.INumber<T>
#endif
    {
        public IVar<T> Var { get; private set; }

        public Operator Op { get; private set; }

        NumMod(IVar<T> _var, Operator op, string name)
        {
            Construct(_var);
            Op = op;
            Name = name;
        }

        NumMod<T> Build(IVar<T> _var, Operator op, string name)
        {
            Construct(_var);
            Op = op;
            Name = name;
            return this;
        }

        void Construct(IVar<T> _var)
        {
            _var.Event.Add(OnChanged);
            Var = _var;
        }

        protected void Deconstruct()
        {
            Var.Release();
            Var.Event.Rem(OnChanged);
            Var = null;
        }

        public T Value
        {
            get => Var.Value;
            set => Var.Value = value;
        }

#if NET7_0_OR_GREATER
        public override T Modify(T given)
        {
            T v = Value.Value;
            return Op switch
            {
                Operator.Add => given + v,
                Operator.Sub => given - v,
                Operator.Mul => given * v,
                Operator.Div => given / v,
                Operator.Set => v,
                _ => given
            };
        }
#else
        public override T Modify(T given)
        {
            var t = Mod.GetOperator<T>();
            T v = Var.Value;
            return Op switch
            {
                Operator.Add => t.Add(given, v),
                Operator.Mul => t.Mul(given, v),
                Operator.Set => v,
                _ => given
            };
        }
#endif

        public override void Release()
        {
            Release(this);
        }

        public override void OnRelease()
        {
            Deconstruct();
            Op = default;
            base.OnRelease();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Num[");
            sb.Append(Name);
            sb.Append("]");

            switch (Op)
            {
                case Operator.Add:
                    sb.Append('+');
                    break;
                case Operator.Mul:
                    sb.Append('*');
                    break;
                case Operator.Set:
                    sb.Append('=');
                    break;
            }

            sb.Append(' ');
            sb.Append(Var.Value);

            return sb.ToString();
        }

        #region Pool

        static Queue<NumMod<T>> pool = new();

        public static NumMod<T> Get(IVar<T> var, Operator op, string name)
        {
            if (pool.TryDequeue(out var numMod))
                return numMod.Build(var, op, name);

            return new NumMod<T>(var, op, name);
        }

        static void Release(NumMod<T> mod)
        {
            mod.OnRelease();
            pool.Enqueue(mod);
        }

        #endregion
    }
}