using System.Runtime.CompilerServices;
using ImGuiNET;
using SDL2Engine.Core.GuiRenderer.Helpers;

namespace SDL2Engine.Core.GuiRenderer
{

    public class ImGuiVariableBinder : IVariableBinder
    {
        private Dictionary<string, object> _variables = new Dictionary<string, object>();
        private Dictionary<string, Action> _drawActions = new Dictionary<string, Action>();

        public void BindVariable<T>(string key, T variable)
        {
            _variables[key] = variable;
            _drawActions[key] = () => DrawAction(key, ref variable);
        }

        private void DrawAction<T>(string key, ref T variable)
        {
            var type = typeof(T);

            if (ImGuiInputHelper.InputActions.TryGetValue(type, out var action))
            {
                object boxedVariable = variable;
                action(key, ref boxedVariable);
                variable = (T)boxedVariable;
            }
            else
            {
                ImGui.Text($"Unsupported type: {type}");
            }
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