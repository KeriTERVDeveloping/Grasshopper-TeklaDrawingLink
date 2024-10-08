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
            ApplyTeklaFieldNames();
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

        private void ApplyTeklaFieldNames()
        {
            var counter = 0;
            foreach (var param in TeklaParams.ModelParams)
                param.FieldName = $"tekla_{counter++}";

            foreach (var param in TeklaParams.DrawingParams)
                param.FieldName = $"tekla_{counter++}";
        }
    }
}
