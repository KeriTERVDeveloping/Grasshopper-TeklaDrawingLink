﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using GTDrawingLink.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Tools
{
    public abstract class Param
    {
        public Type DataType { get; }
        public GH_InstanceDescription InstanceDescription { get; }
        public GH_ParamAccess ParamAccess { get; }

        public Param(Type type, GH_InstanceDescription instanceDescription, GH_ParamAccess paramAccess)
        {
            DataType = type;
            InstanceDescription = instanceDescription;
            ParamAccess = paramAccess;
        }
    }

    public abstract class InputParam : Param
    {
        protected bool _isOptional;
        protected InputParam(Type type, GH_InstanceDescription instanceDescription, GH_ParamAccess paramAccess)
            : base(type, instanceDescription, paramAccess)
        {
        }

        public bool IsOptional { get; protected set; }
        public abstract Result EvaluateInput(IGH_DataAccess DA);

        protected Result GetWrongInputMessage(string parameterName)
        {
            return Result.Fail($"Wrong input: {parameterName}");
        }
    }

    public abstract class OutputParam : Param
    {
        protected OutputParam(Type type, GH_InstanceDescription instanceDescription, GH_ParamAccess paramAccess)
            : base(type, instanceDescription, paramAccess)
        {
        }

        public abstract Result SetOutput(IGH_DataAccess DA);
    }

    public class OutputParam<T> : OutputParam
    {
        public T Value { get; set; }

        public OutputParam(GH_InstanceDescription instanceDescription)
            : base(typeof(T), instanceDescription, GH_ParamAccess.item)
        {
        }

        public override Result SetOutput(IGH_DataAccess DA)
        {
            if (DA.SetData(InstanceDescription.Name, Value))
                return Result.Ok();
            else
                return Result.Fail("SetData failed");
        }
    }

    public class OutputListParam<T> : OutputParam
    {
        public IEnumerable<T> Value { get; set; }

        public OutputListParam(GH_InstanceDescription instanceDescription)
            : base(typeof(T), instanceDescription, GH_ParamAccess.list)
        {
        }

        public override Result SetOutput(IGH_DataAccess DA)
        {
            if (DA.SetDataList(InstanceDescription.Name, Value))
                return Result.Ok();
            else
                return Result.Fail("SetDataList failed");
        }
    }

    public class InputParam<T> : InputParam where T : class
    {
        protected bool _properlySet;
        protected T _value;
        public T Value => _properlySet ? _value : throw new InvalidOperationException(InstanceDescription.Name);

        public InputParam(GH_InstanceDescription instanceDescription)
            : base(typeof(T), instanceDescription, GH_ParamAccess.item)
        {
        }

        public override Result EvaluateInput(IGH_DataAccess DA)
        {
            var typeOfInput = typeof(T);
            if (typeOfInput.InheritsFrom(typeof(DatabaseObject)))
            {
                GH_Goo<DatabaseObject> objectGoo = null;
                if (DA.GetData(InstanceDescription.Name, ref objectGoo))
                {
                    _value = objectGoo.Value as T;
                    if (_value == null)
                    {
                        return Result.Fail($"Provided input is not type of {typeOfInput.ToShortString()}");
                    }

                    _properlySet = true;
                    return Result.Ok();
                }
            }
            else
            {
                GH_Goo<T> objectGoo = null;
                if (DA.GetData(InstanceDescription.Name, ref objectGoo))
                {
                    _value = objectGoo.Value;
                    _properlySet = true;
                    return Result.Ok();
                }
            }

            return GetWrongInputMessage(InstanceDescription.Name);
        }
    }
    public class InputOptionalParam<T> : InputParam<T> where T : class
    {
        private T _defaultValue;

        public bool ValueProvidedByUser { get; private set; }

        public InputOptionalParam(GH_InstanceDescription instanceDescription, T defaultValue)
            : base(instanceDescription)
        {
            IsOptional = true;
            _defaultValue = defaultValue;
        }

        public InputOptionalParam(GH_InstanceDescription instanceDescription)
            : base(instanceDescription)
        {
            IsOptional = true;
            _defaultValue = default;
        }

        public override Result EvaluateInput(IGH_DataAccess DA)
        {
            var resultFromUserInput = base.EvaluateInput(DA);
            if (resultFromUserInput.Success)
            {
                ValueProvidedByUser = true;
            }
            if (resultFromUserInput.Failure)
            {
                _value = _defaultValue;
                _properlySet = true;
            }

            return Result.Ok();
        }
    }

    public class InputStructParam<T> : InputParam where T : struct, IConvertible
    {
        protected bool _properlySet;
        protected T _value;
        public T Value => _properlySet ? _value : throw new InvalidOperationException(InstanceDescription.Name);

        public InputStructParam(GH_InstanceDescription instanceDescription)
            : base(typeof(T), instanceDescription, GH_ParamAccess.item)
        {
        }

        public override Result EvaluateInput(IGH_DataAccess DA)
        {
            var typeOfInput = typeof(T);
            if (typeOfInput.IsEnum)
            {
                object inputObject = null;
                DA.GetData(InstanceDescription.Name, ref inputObject);
                var inputCastedToEnum = EnumHelpers.ObjectToEnumValue<T>(inputObject);
                if (inputCastedToEnum.HasValue)
                {
                    _value = inputCastedToEnum.Value;
                    _properlySet = true;
                    return Result.Ok();
                }
            }
            else
            {
                GH_Goo<T> objectGoo = null;
                if (DA.GetData(InstanceDescription.Name, ref objectGoo))
                {
                    _value = objectGoo.Value;
                    _properlySet = true;
                    return Result.Ok();
                }
            }

            return GetWrongInputMessage(InstanceDescription.Name);
        }
    }
    public class InputOptionalStructParam<T> : InputStructParam<T> where T : struct, IConvertible
    {
        private T _defaultValue;

        public bool ValueProvidedByUser { get; private set; }

        public InputOptionalStructParam(GH_InstanceDescription instanceDescription, T defaultValue)
            : base(instanceDescription)
        {
            IsOptional = true;
            _defaultValue = defaultValue;
        }

        public InputOptionalStructParam(GH_InstanceDescription instanceDescription)
            : base(instanceDescription)
        {
            IsOptional = true;
            _defaultValue = default;
        }

        public override Result EvaluateInput(IGH_DataAccess DA)
        {
            var resultFromUserInput = base.EvaluateInput(DA);
            if (resultFromUserInput.Success)
            {
                ValueProvidedByUser = true;
            }
            if (resultFromUserInput.Failure)
            {
                _value = _defaultValue;
                _properlySet = true;
            }

            return Result.Ok();
        }
    }

    public class InputListParam<T> : InputParam
    {
        private bool _properlySet;
        private List<T> _value;
        public List<T> Value => _properlySet ? _value : throw new InvalidOperationException(InstanceDescription.Name);

        public InputListParam(GH_InstanceDescription instanceDescription)
            : base(typeof(T), instanceDescription, GH_ParamAccess.list)
        {
        }

        public override Result EvaluateInput(IGH_DataAccess DA)
        {
            var value = new List<GH_Goo<T>>();
            if (DA.GetDataList(InstanceDescription.Name, value))
            {
                _value = value.Select(v => v.Value).ToList();
                _properlySet = true;
                return Result.Ok();
            }
            else
            {
                _properlySet = false;
                return GetWrongInputMessage(InstanceDescription.Name);
            }
        }
    }

    public class InputTreeParam<T> : InputParam
    {
        private bool _properlySet;
        private List<List<T>> _value;
        public List<List<T>> Value => _properlySet ? _value : throw new InvalidOperationException(InstanceDescription.Name);

        public InputTreeParam(GH_InstanceDescription instanceDescription)
            : base(typeof(T), instanceDescription, GH_ParamAccess.tree)
        {
        }

        public override Result EvaluateInput(IGH_DataAccess DA)
        {
            if (DA.GetDataTree(InstanceDescription.Name, out GH_Structure<GH_Goo<T>> tree))
            {
                _value = tree.Branches.Select(b => b.Select(i => i.Value).ToList()).ToList();
                _properlySet = true;
                return Result.Ok();
            }
            else
            {
                _properlySet = false;
                return GetWrongInputMessage(InstanceDescription.Name);
            }
        }
    }
    public abstract class CommandBase
    {
        private IEnumerable<InputParam> _inputParameters;
        private IEnumerable<OutputParam> _outputParameters;

        public IEnumerable<InputParam> GetInputParameters()
        {
            if (_inputParameters == null)
                _inputParameters = GetPrivateFieldsWithType<InputParam>();

            return _inputParameters;
        }

        public IEnumerable<OutputParam> GetOutputParameters()
        {
            if (_outputParameters == null)
                _outputParameters = GetPrivateFieldsWithType<OutputParam>();

            return _outputParameters;
        }

        public Result EvaluateInput(IGH_DataAccess DA)
        {
            var results = _inputParameters.Select(p => p.EvaluateInput(DA));
            return Result.Combine(results.ToArray());
        }

        protected Result SetOutput(IGH_DataAccess DA)
        {
            var results = _outputParameters.Select(p => p.SetOutput(DA));
            return Result.Combine(results.ToArray());
        }

        private IEnumerable<T> GetPrivateFieldsWithType<T>() where T : class
        {
            var type = typeof(T);
            var parameters = new List<T>();
            foreach (var property in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (property.FieldType.BaseType.InheritsFrom(type))
                {
                    parameters.Add(property.GetValue(this) as T);
                }
            }

            return parameters;
        }
    }
    public class Result
    {
        public bool Success { get; private set; }
        public string Error { get; private set; }

        public bool Failure
        {
            get { return !Success; }
        }

        protected Result(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(default, false, message);
        }

        public static Result Ok()
        {
            return new Result(true, String.Empty);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, String.Empty);
        }

        public static Result Combine(params Result[] results)
        {
            foreach (Result result in results)
            {
                if (result.Failure)
                    return result;
            }

            return Ok();
        }
    }

    public class Result<T> : Result
    {
        private T _value;

        public T Value
        {
            get { return _value; }
            private set { _value = value; }
        }

        protected internal Result(T value, bool success, string error)
            : base(success, error)
        {
            Value = value;
        }
    }
}