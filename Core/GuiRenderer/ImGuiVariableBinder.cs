using System.Runtime.CompilerServices;
using ImGuiNET;

namespace SDL2Engine.Core.GuiRenderer
{

    public class ImGuiVariableBinder : IVariableBinder
    {
        private Dictionary<string, object> _variables = new Dictionary<string, object>();
        private Dictionary<string, Action> _drawActions = new Dictionary<string, Action>();

        public void BindVariable<T>(string key, T variable)
        {
            _variables[key] = variable;
            _drawActions[key] = () => DrawAction(key, variable);
        }

        private void DrawAction<T>(string key, T variable)
        {
            switch (variable)
            {
                case int val:
                    DrawEditable(key, ref val);
                    break;
                case float val:
                    DrawEditable(key, ref val);
                    break;
                case string val:
                    DrawEditable(key, ref val, 100);
                    break;
                case bool val:
                    DrawEditable(key, ref val);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported type {typeof(T)} provided for ImGui binding.");
            }
            _variables[key] = variable;
        }

private void DrawEditable<T>(string key, ref T value, int maxLength = 0)
{
    // Handling integers
    if (value is int intValue)
    {
        ImGui.InputInt(key, ref intValue);
        value = (T)(object)intValue;
        return;
    }

    // Handling floats
    if (value is float floatValue)
    {
        ImGui.InputFloat(key, ref floatValue);
        value = (T)(object)floatValue;
        return;
    }

    // Handling booleans
    if (value is bool boolValue)
    {
        ImGui.Checkbox(key, ref boolValue);
        value = (T)(object)boolValue;
        return;
    }

    // Handling strings
    if (value is string stringValue)
    {
        ImGui.InputText(key, ref stringValue, (uint)maxLength);
        value = (T)(object)stringValue;
        return;
    }

    ImGui.Text($"Unsupported type {typeof(T)} for ImGui binding.");
}


        public void Draw(string key)
        {
            if (_drawActions.TryGetValue(key, out var drawAction))
            {
                drawAction();
            }
            else
            {
                ImGui.Text($"No binding found for {key}");
            }
        }
    }
}