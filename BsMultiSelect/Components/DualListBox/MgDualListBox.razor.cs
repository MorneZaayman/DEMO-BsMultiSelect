using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BsMultiSelect.Components.DualListBox
{
    public partial class MgDualListBox<T> : ComponentBase where T : struct
    {
        #region Injected Services
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }
        #endregion

        #region Parameters
        private List<T> _value;
        [Parameter]
        public List<T> Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<List<T>>.Default.Equals(_value, value))
                {
                    _value = value;
                    ValueChanged.InvokeAsync(value);
                }
            }
        }

        [Parameter] public EventCallback<List<T>> ValueChanged { get; set; }

        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Parameter] public Dictionary<T, string> Options { get; set; }
        #endregion

        #region Component Lifecycle
        /// <summary>
        /// Performs the necessary setup after the component has been rendered.
        /// </summary>
        /// <param name="firstRender">Indicates whether this is the first render of the component.</param>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/DualListBox/MgDualListBox.razor.js");
            _selfReference = DotNetObjectReference.Create(this);

            if (firstRender)
            {
                await _module.InvokeVoidAsync("initialize", Id, _selfReference);
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_module is not null)
            {
                await _module.InvokeVoidAsync("refresh", Id);
            }
        }
        #endregion

        private DotNetObjectReference<MgDualListBox<T>>? _selfReference;
        private IJSObjectReference _module;

        #region JS Invokable
        /// <summary>
        /// Handles the change event of the dual list box.
        /// Parses the selected values and invokes the ValueChanged event.
        /// </summary>
        /// <param name="values">The array of selected values.</param>
        [JSInvokable]
        public async Task OnChange(string[] values)
        {
            List<T> parsedValues = new List<T>();

            foreach (string value in values)
            {
                if (TryParseValue(value, out T parsedValue))
                {
                    parsedValues.Add(parsedValue);
                }
                else
                {
                    // Do nothing - Throw away values that do not parse.
                }
            }

            await ValueChanged.InvokeAsync([.. parsedValues]);

            static bool TryParseValue(string value, out T parsedValue)
            {
                bool success = false;
                parsedValue = default;

                try
                {
                    // Use the TryParse method to attempt parsing the value  
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter.IsValid(value))
                    {
                        parsedValue = (T)converter.ConvertFromString(value);
                        success = true;
                    }
                }
                catch (NotSupportedException)
                {
                    // Do nothing - will return false. Caller should handle this case.
                }

                return success;
            }
        }
        #endregion
    }
}
