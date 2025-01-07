using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using SDL2Engine.Core.Utils;
using static SDL2Engine.Core.Engine;
namespace SDL2Engine.Core.GuiRenderer.Helpers
{
    public static class ImGuiInputHelper
    {
        public delegate void ImGuiInputDelegate(string key, ref object value);

        public static readonly Dictionary<Type, ImGuiInputDelegate> InputActions = new()
    {
        { typeof(int), ImGuiInputInt },
        { typeof(float), ImGuiInputFloat },
        { typeof(double), ImGuiInputDouble },
        { typeof(bool), ImGuiCheckbox },
        { typeof(string), ImGuiInputText },
        { typeof(long), ImGuiInputLong },
        { typeof(short), ImGuiInputShort },
        { typeof(byte), ImGuiInputByte },
        { typeof(uint), ImGuiInputUInt },
        { typeof(ulong), ImGuiInputULong },
        { typeof(ushort), ImGuiInputUShort },
        { typeof(sbyte), ImGuiInputSByte },
        // { typeof(char), ImGuiInputChar },   no worky fucky wucky ):
        { typeof(Vector2), ImGuiInputVector2 },
        { typeof(Vector3), ImGuiInputVector3 },
        { typeof(Vector4), ImGuiInputVector4 },
        { typeof(Color), ImGuiInputColor },
        { typeof(ExampleEnum), ImGuiInputEnum },
        { typeof(string[]), ImGuiInputTab },
        { typeof(ImGuiTableData), ImGuiInputTable },
        { typeof(ImGuiCellTableData), ImGuiInputCell },
        { typeof(Action), ImGuiInputButton }
    };

        private static void ImGuiInputButton(string key, ref object value)
        {
            if (value is Action action)
            {
                if (ImGui.Button(key))
                {
                    action.Invoke();
                }
            }
            else
            {
                ImGui.Text("Invalid value for button. Expected Action.");
            }
        }

        private static void ImGuiInputInt(string key, ref object value)
        {
            int intValue = (int)value;
            ImGui.InputInt(key, ref intValue);
            value = intValue;
        }

        private static void ImGuiInputFloat(string key, ref object value)
        {
            float floatValue = (float)value;
            ImGui.InputFloat(key, ref floatValue);
            value = floatValue;
        }

        private static void ImGuiInputDouble(string key, ref object value)
        {
            double doubleValue = (double)value;
            ImGui.InputDouble(key, ref doubleValue);
            value = doubleValue;
        }

        private static void ImGuiCheckbox(string key, ref object value)
        {
            bool boolValue = (bool)value;
            ImGui.Checkbox(key, ref boolValue);
            value = boolValue;
        }

        private static void ImGuiInputText(string key, ref object value)
        {
            string stringValue = (string)value;
            ImGui.InputText(key, ref stringValue, 256);
            value = stringValue;
        }

        private static void ImGuiInputLong(string key, ref object value)
        {
            long longValue = (long)value;

            var handle = GCHandle.Alloc(longValue, GCHandleType.Pinned);
            try
            {
                ImGui.InputScalar(key, ImGuiDataType.S64, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }

            value = longValue;
        }

        private static void ImGuiInputShort(string key, ref object value)
        {
            short shortValue = (short)value;
            var handle = GCHandle.Alloc(shortValue, GCHandleType.Pinned);
            try
            {
                ImGui.InputScalar(key, ImGuiDataType.S16, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
            value = shortValue;
        }

        private static void ImGuiInputByte(string key, ref object value)
        {
            byte byteValue = (byte)value;
            var handle = GCHandle.Alloc(byteValue, GCHandleType.Pinned);
            try
            {
                ImGui.InputScalar(key, ImGuiDataType.U8, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
            value = byteValue;
        }

        private static void ImGuiInputUInt(string key, ref object value)
        {
            uint uintValue = (uint)value;
            var handle = GCHandle.Alloc(uintValue, GCHandleType.Pinned);
            try
            {
                ImGui.InputScalar(key, ImGuiDataType.U32, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
            value = uintValue;
        }

        private static void ImGuiInputULong(string key, ref object value)
        {
            ulong ulongValue = (ulong)value;
            var handle = GCHandle.Alloc(ulongValue, GCHandleType.Pinned);
            try
            {
                ImGui.InputScalar(key, ImGuiDataType.U64, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
            value = ulongValue;
        }

        private static void ImGuiInputUShort(string key, ref object value)
        {
            ushort ushortValue = (ushort)value;
            var handle = GCHandle.Alloc(ushortValue, GCHandleType.Pinned);
            try
            {
                ImGui.InputScalar(key, ImGuiDataType.U16, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
            value = ushortValue;
        }

        private static void ImGuiInputSByte(string key, ref object value)
        {
            sbyte sbyteValue = (sbyte)value;
            var handle = GCHandle.Alloc(sbyteValue, GCHandleType.Pinned);
            try
            {
                ImGui.InputScalar(key, ImGuiDataType.S8, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
            value = sbyteValue;
        }


        // private static void ImGuiInputChar(string key, ref object value)
        // {
        //     char charValue = (char)value;
        //     ImGui.InputText(key, ref charValue, 1);
        //     value = charValue;
        // }

        private static void ImGuiInputVector2(string key, ref object value)
        {
            Vector2 vectorValue = (Vector2)value;
            ImGui.InputFloat2(key, ref vectorValue);
            value = vectorValue;
        }

        private static void ImGuiInputVector3(string key, ref object value)
        {
            Vector3 vectorValue = (Vector3)value;
            ImGui.InputFloat3(key, ref vectorValue);
            value = vectorValue;
        }

        private static void ImGuiInputVector4(string key, ref object value)
        {
            Vector4 vectorValue = (Vector4)value;
            ImGui.InputFloat4(key, ref vectorValue);
            value = vectorValue;
        }

        private static void ImGuiInputColor(string key, ref object value)
        {
            Vector4 colorValue = (Vector4)value;
            ImGui.ColorEdit4(key, ref colorValue);
            value = colorValue;
        }

        private static void ImGuiInputEnum(string key, ref object value)
        {
            ExampleEnum enumValue = (ExampleEnum)value;
            string[] enumNames = ExampleEnum.GetNames(enumValue.GetType());
            int currentIndex = Array.IndexOf(enumNames, enumValue.ToString());

            if (ImGui.Combo(key, ref currentIndex, enumNames, enumNames.Length))
            {
                value = enumNames[currentIndex];//Enum.Parse(enumValue.GetType(), enumNames[currentIndex]);
            }
        }

        private static void ImGuiInputTab(string key, ref object value)
        {
            if (value is string[] tabLabels)
            {
                if (ImGui.BeginTabBar(key))
                {
                    for (int i = 0; i < tabLabels.Length; i++)
                    {
                        if (ImGui.BeginTabItem(tabLabels[i]))
                        {
                            value = tabLabels[i];
                            ImGui.EndTabItem();
                        }
                    }
                    ImGui.EndTabBar();
                }
            }
            else
            {
                ImGui.Text("Invalid value for tabs. Expected string[] for tab labels.");
            }
        }

        private static void ImGuiInputTable(string key, ref object value)
        {
            if (value is ImGuiTableData table)
            {
                if (ImGui.BeginTable(key + "table", table.Columns.Length, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.HighlightHoveredColumn))
                {
                    foreach (var column in table.Columns)
                    {
                        ImGui.TableSetupColumn(column.Header);
                    }
                    ImGui.TableHeadersRow();

                    int maxRows = table.Columns.Max(col => col.Values.Length);

                    for (int row = 0; row < maxRows; row++)
                    {
                        for (int col = 0; col < table.Columns.Length; col++)
                        {
                            ImGui.TableNextColumn();

                            var inputLabel = row < table.Columns[col].Values.Length ? table.Columns[col].Values[row].Label : string.Empty;
                            var cellValue = row < table.Columns[col].Values.Length ? table.Columns[col].Values[row].Value : string.Empty;
                            var isReadOnly = row < table.Columns[col].Values.Length ? table.Columns[col].Values[row].IsReadOnly : false;
                            var color = row < table.Columns[col].Values.Length ? table.Columns[col].Values[row].Color : Vector4.One;
                            var inputTextLabel = inputLabel;
                            var inputTextFlag = isReadOnly ? ImGuiInputTextFlags.ReadOnly : ImGuiInputTextFlags.None;

                            inputTextLabel = $"##{inputLabel}_{row}_{col}";

                            if (isReadOnly)
                            {
                                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f)); 
                                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.75f, 0.75f, 0.75f, 1.0f));   
                            }

                            if (!table.LableOnRight)
                            {
                                ImGui.TextColored(color, inputLabel);
                                ImGui.SameLine();
                            }

                            if (ImGui.InputText(inputTextLabel, ref cellValue, 256, inputTextFlag))
                            {
                                if (row < table.Columns[col].Values.Length)
                                {
                                    table.Columns[col].Values[row].Value = cellValue;
                                }
                                else
                                {
                                    var values = table.Columns[col].Values;
                                    Array.Resize(ref values, row + 1);
                                    table.Columns[col].Values = values;
                                    table.Columns[col].Values[row].Value = cellValue;
                                }
                            }

                            if (table.LableOnRight)
                            {
                                ImGui.SameLine();
                                ImGui.TextColored(color, inputLabel);
                            }
                            if (isReadOnly)
                            {
                                ImGui.PopStyleColor(2);  // Pop both FrameBg and Text colors
                            }
                        }
                    }
                    ImGui.EndTable();
                }
            }
            else
            {
                ImGui.Text("Invalid value for table. Expected ImGuiTableData.");
            }
        }


        private static void ImGuiInputCell(string key, ref object value)
        {
            if (value is ImGuiCellTableData table)
            {
                ImGui.BeginTable(key + "table", table.ImGuiCell.Length,
                    ImGuiTableFlags.Resizable
                    | ImGuiTableFlags.Borders
                    | ImGuiTableFlags.HighlightHoveredColumn);

                foreach (var cell in table.ImGuiCell)
                {
                    ImGui.TableSetupColumn(cell.Header);
                }

                ImGui.TableHeadersRow();
                for (int i = 0; i < table.ImGuiCell.Length; i++)
                {
                    var cell = table.ImGuiCell[i];

                    for (int j = 0; j < cell.Value.Length; j++)
                    {
                        byte val = (byte)((i + j) % 2 == 0 ? 20 : 25);
                        ImGui.TableNextColumn();
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ColorHelper.GetColor(val, val, val));

                        ImGui.Text(cell.Value[j]);
                    }
                }

                ImGui.EndTable();
            }
            else
            {
                ImGui.Text("Invalid value for cell. Expected ImGuiCell.");
            }
        }
    }
}