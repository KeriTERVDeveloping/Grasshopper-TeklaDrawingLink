﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;

namespace DrawingLink.UI.GH
{
    public class GHParams
    {
        public TeklaParams TeklaParams { get; }
        public IReadOnlyList<ActiveObjectWrapper> AttributeParams { get; }

        public GHParams(TeklaParams teklaParams, List<ActiveObjectWrapper> attributeParams)
        {
            TeklaParams = teklaParams ?? throw new ArgumentNullException(nameof(teklaParams));
            AttributeParams = attributeParams ?? throw new ArgumentNullException(nameof(attributeParams));

            ApplyFieldNames();
        }

        private void ApplyFieldNames()
        {
            var stringCounter = 0;
            var doubleCounter = 0;
            var intCounter = 0;

            foreach (var activeObjectWrapper in AttributeParams)
            {
                var fieldName = activeObjectWrapper.ActiveObject switch
                {
                    Param_FilePath => GetNextStringFieldName(),
                    GH_PersistentParam<GH_String> => GetNextStringFieldName(),
                    GH_PersistentParam<GH_Integer> => GetNextIntFieldName(),
                    GH_PersistentParam<GH_Number> => GetNextDoubleFieldName(),
                    GH_Panel panel => activeObjectWrapper.Connectivity.IsStandalone() ? "" : GetNextStringFieldName(),
                    GH_ValueList => GetNextStringFieldName(),
                    GH_BooleanToggle => GetNextStringFieldName(),
                    GH_ButtonObject => GetNextStringFieldName(),
                    GH_NumberSlider => GetNextDoubleFieldName(),
                    _ => IsCatalogBaseComponent(activeObjectWrapper.ActiveObject) ? GetNextStringFieldName() : ""
                };

                activeObjectWrapper.FieldName = fieldName;
            }

            string GetNextStringFieldName()
                => $"string_{stringCounter++}";
            string GetNextDoubleFieldName()
                => $"double_{doubleCounter++}";
            string GetNextIntFieldName()
                => $"int_{intCounter++}";
            bool IsCatalogBaseComponent(IGH_ActiveObject activeObject)
                => activeObject.GetType().BaseType.Name == "CatalogBaseComponent";
        }
    }

    public class ActiveObjectWrapper
    {
        public IGH_ActiveObject ActiveObject { get; }
        public TabAndGroup TabAndGroup { get; }
        public ObjectConnectivity Connectivity { get; }
        public string FieldName { get; set; }

        public ActiveObjectWrapper(IGH_ActiveObject activeObject, TabAndGroup tabAndGroup, ObjectConnectivity connectivity)
        {
            ActiveObject = activeObject ?? throw new ArgumentNullException(nameof(activeObject));
            TabAndGroup = tabAndGroup ?? throw new ArgumentNullException(nameof(tabAndGroup));
            Connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            FieldName = string.Empty;
        }
    }

    public abstract class TeklaParamBase
    {
        public IGH_ActiveObject ActiveObject { get; }
        public bool IsMultiple { get; }

        protected TeklaParamBase(IGH_ActiveObject activeObject, bool isMultiple)
        {
            ActiveObject = activeObject ?? throw new ArgumentNullException(nameof(activeObject));
            IsMultiple = isMultiple;
        }
    }

    public class TeklaModelParam : TeklaParamBase
    {
        public ModelParamType ParamType { get; }

        public TeklaModelParam(IGH_ActiveObject activeObject, ModelParamType paramType, bool isMultiple) : base(activeObject, isMultiple)
        {
            ParamType = paramType;
        }
    }

    public class TeklaDrawingParam : TeklaParamBase
    {
        public DrawingParamType ParamType { get; }

        public TeklaDrawingParam(IGH_ActiveObject activeObject, DrawingParamType paramType, bool isMany) : base(activeObject, isMany)
        {
            ParamType = paramType;
        }
    }

    public enum ModelParamType
    {
        Point,
        Line,
        Polyline,
        Face,
        Object
    }

    public enum DrawingParamType
    {
        Point,
        Object
    }
}
