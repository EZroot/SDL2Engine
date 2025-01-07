using ImGuiNET;
using SDL2Engine.Core.GuiRenderer.Interfaces;
namespace SDL2Engine.Core.GuiRenderer
{
    public class ImGuiWindowBuilder : IServiceGuiWindowService
    {
        private Dictionary<string, object> _variables = new Dictionary<string, object>();
        private Dictionary<string, Action> _drawActions = new Dictionary<string, Action>();

        public void BindVariable<T>(string key, T variable)
        {
            _variables[key] = variable;
            if (typeof(T) == typeof(int))
            {
                _drawActions[key] = () =>
                {
                    int val = (int)_variables[key];
                    if (ImGui.InputInt(key, ref val))
                    {
                        _variables[key] = val;
                    }
                };
            }
            else if (typeof(T) == typeof(float))
            {
                _drawActions[key] = () =>
                {
                    float val = (float)_variables[key];
                    if (ImGui.InputFloat(key, ref val))
                    {
                        _variables[key] = val;
                    }
                };
            }
            else if (typeof(T) == typeof(string))
            {
                _drawActions[key] = () =>
                {
                    string val = (string)_variables[key];
                    if (ImGui.InputText(key, ref val, 100))
                    {
                        _variables[key] = val;
                    }
                };
            }
            else if (typeof(T) == typeof(bool))
            {
                _drawActions[key] = () =>
                {
                    bool val = (bool)_variables[key];
                    if (ImGui.Checkbox(key, ref val))
                    {
                        _variables[key] = val;
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Unsupported type provided for ImGui binding.");
            }
        }

        public void BeginWindow(string title)
        {
            ImGui.Begin(title);
        }

        public void EndWindow()
        {
            ImGui.End();
        }

        public void Draw(string key)
        {
            if (_drawActions.ContainsKey(key))
            {
                _drawActions[key].Invoke();
            }
            else
            {
                ImGui.Text($"No binding found for {key}");
            }
        }
    }
}