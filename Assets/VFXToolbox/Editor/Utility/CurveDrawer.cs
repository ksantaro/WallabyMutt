using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UnityEditor.VFXToolbox
{
    public class CurveDrawer
    {
        private enum EditMode
        {
            None,
            Moving,
            TangentEdit
        }

        private enum Tangent
        {
            In,
            Out
        }

        private List<string> m_CurveNames;
        private List<SerializedProperty> m_CurvesSerializedProperties;
        private List<Color> m_CurveColors;
        private List<bool> m_CurveVisibilities;

        public float minInput { get { return m_MinInput; } set { m_MinInput = value; } }
        public float maxInput { get { return m_MaxInput; } set { m_MaxInput = value; } }
        public float minOutput { get { return m_MinOutput; } set { m_MinOutput = value; } }
        public float maxOutput { get { return m_MaxOutput; } set { m_MaxOutput = value; } }

        private float m_MinInput;
        private float m_MaxInput;
        private float m_MinOutput;
        private float m_MaxOutput;

        private Rect m_CurveArea;
        private string m_CurveEditName;
        private int m_WidgetDefaultHeight;
        private bool m_WidgetShowToolbar;

        private bool m_Dirty;
        private EditMode m_EditMode;
        private Tangent m_TangentEditMode;
        private bool m_EditBothTangents;

        private int m_SelectedCurve;
        private int m_SelectedKeyFrame;

        public delegate void CurveDrawEventDelegate(Rect renderArea, Rect curveArea);
        public CurveDrawEventDelegate OnPostGUI;

        private float lineBrightnessValue { get { return EditorGUIUtility.isProSkin ? 1.0f : 0.0f; } }

        public CurveDrawer(string curveEditName, float minInput, float maxInput, float minOutput, float maxOutput, int height, bool showToolbar)
            :this(curveEditName, minInput, maxInput, minOutput, maxOutput)
        {
            m_WidgetDefaultHeight = height;
            m_WidgetShowToolbar = showToolbar;
        }

        public CurveDrawer(string curveEditName, float minInput, float maxInput, float minOutput, float maxOutput)
        {
            m_CurveNames = new List<string>();
            m_CurvesSerializedProperties = new List<SerializedProperty>();
            m_CurveColors = new List<Color>();
            m_CurveVisibilities = new List<bool>();

            m_MinInput = minInput;
            m_MaxInput = maxInput;
            m_MinOutput = minOutput;
            m_MaxOutput = maxOutput;
            m_CurveEditName = curveEditName;
            m_SelectedCurve = -1;
            m_SelectedKeyFrame = -1;
            m_EditMode = EditMode.None;
            m_WidgetDefaultHeight = 240;
            m_WidgetShowToolbar = true;
        }

        public void AddCurve(SerializedProperty curveProperty, Color curveColor, string name, bool visible = true)
        {
            m_CurveNames.Add(name);
            m_CurvesSerializedProperties.Add(curveProperty);
            m_CurveColors.Add(curveColor);
            m_CurveVisibilities.Add(visible);
        }

        public void RemoveCurve(string name)
        {
            if(m_CurveNames.Contains(name))
            {
                int index = m_CurveNames.IndexOf(name);
                m_CurvesSerializedProperties.RemoveAt(index);
                m_CurveNames.RemoveAt(index);
                m_CurveColors.RemoveAt(index);
                m_CurveVisibilities.RemoveAt(index);
            }
        }

        public void Clear()
        {
            m_CurveNames.Clear();
            m_CurvesSerializedProperties.Clear();
            m_CurveColors.Clear();
            m_CurveVisibilities.Clear();
        }

        public bool OnGUI(Rect drawRect)
        {
            
            GUILayout.BeginArea(drawRect);
            OnGUILayout();
            GUILayout.EndArea();
            return m_Dirty;
        }

        public bool OnGUILayout()
        {
            return OnGUILayout(m_WidgetDefaultHeight, m_WidgetShowToolbar);
        }

        public bool OnGUILayout(bool showToolbar)
        {
            return OnGUILayout(m_WidgetDefaultHeight, showToolbar);
        }

        private void Invalidate()
        {
            m_Dirty = true;
        }

        public bool OnGUILayout(float height, bool showToolbar)
        {
            m_Dirty = false;
            using (new GUILayout.VerticalScope())
            {
                // Prepare data for editing
                List<AnimationCurve> curves = new List<AnimationCurve>();
                List<Keyframe[]> keys = new List<Keyframe[]>();

                for(int i = 0; i < m_CurvesSerializedProperties.Count; i++)
                {
                    curves.Add(m_CurvesSerializedProperties[i].animationCurveValue);
                    keys.Add(curves[i].keys);
                }

                // Header
                if(m_CurveEditName != null || m_CurveEditName == "")
                    GUILayout.Label(m_CurveEditName);

                GUILayout.Space(4.0f);

                // Curve Area
                if(showToolbar)
                {
                    using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        for(int i = 0; i < m_CurvesSerializedProperties.Count; i++)
                        {
                            bool b = GUILayout.Toggle(m_CurveVisibilities[i], m_CurveNames[i], EditorStyles.toolbarButton);
                            if (b != m_CurveVisibilities[i])
                                m_CurveVisibilities[i] = b;
                        }
                        GUILayout.FlexibleSpace();
                    }
                }

                Rect lastRect = GUILayoutUtility.GetLastRect();
                Rect curveArea = GUILayoutUtility.GetRect(lastRect.width, height);

                // Draw Selection if any
                if(m_SelectedCurve >= 0  && m_SelectedKeyFrame >= 0)
                {
                    EditorGUI.indentLevel ++;
                    Keyframe key = keys[m_SelectedCurve][m_SelectedKeyFrame];
                    float t = EditorGUILayout.Slider("Time", key.time, m_MinInput, m_MaxInput);
                    float v = EditorGUILayout.Slider("Value", key.value, m_MinOutput, m_MaxOutput);
                    float inTgt = EditorGUILayout.FloatField("In Tangent", key.inTangent);
                    float outTgt = EditorGUILayout.FloatField("Out Tangent", key.outTangent);

                    if(t != key.time || v != key.value || inTgt != key.inTangent || outTgt != key.outTangent)
                    {
                        Keyframe k = new Keyframe(t, v, inTgt, outTgt);
                        SetKeyFrame(curves[m_SelectedCurve],m_SelectedCurve, m_SelectedKeyFrame, k);
                    }
                    EditorGUI.indentLevel--;
                }

                // Curve Area, after so Keystrokes are catched well
                DrawCurveArea(curveArea, curves, keys);
            }
            return m_Dirty;
        }

        private void DrawCurveArea(Rect rect, List<AnimationCurve> curves, List<Keyframe[]> keys)
        {
            if (Event.current.type == EventType.layout)
                return;

            EditorGUI.DrawRect(rect, new Color(0.0f, 0.0f, 0.0f, 0.25f));
            GUI.BeginClip(rect);

            Rect area = new Rect(Vector2.zero, rect.size);
            m_CurveArea = new RectOffset(40, 16, 16, 16).Remove(area);

            //////////////////////////////////////////////////////////////////////////////
            // Draw Origins
            //////////////////////////////////////////////////////////////////////////////

            float l = lineBrightnessValue;

            Handles.color = new Color(l, l, l, 0.2f);
            Handles.DrawLine(new Vector2(m_CurveArea.xMin, area.yMin), new Vector2(m_CurveArea.xMin, area.yMax));
            Handles.DrawLine(new Vector2(area.xMin, m_CurveArea.yMax), new Vector2(area.xMax, m_CurveArea.yMax));
            Handles.DrawLine(new Vector2(m_CurveArea.xMax, area.yMin), new Vector2(m_CurveArea.xMax, area.yMax));
            Handles.DrawLine(new Vector2(area.xMin, m_CurveArea.yMin), new Vector2(area.xMax, m_CurveArea.yMin));

            //////////////////////////////////////////////////////////////////////////////
            // Draw Zero Axis'es
            //////////////////////////////////////////////////////////////////////////////

            if(m_MinInput < 0 && m_MaxInput > 0)
            {
                Handles.color = new Color(l, l, l, 0.6f);
                Handles.DrawLine(CurveToCanvas(new Vector2(0,m_MinOutput)),CurveToCanvas(new Vector2(0,m_MaxOutput)));
            }

            if(m_MinOutput < 0 && m_MaxOutput > 0)
            {
                Handles.color = new Color(l, l, l, 0.6f);
                Handles.DrawLine(CurveToCanvas(new Vector2(m_MinInput,0)),CurveToCanvas(new Vector2(m_MaxInput, 0)));
            }

            //////////////////////////////////////////////////////////////////////////////
            // Draw Grid By Step
            //////////////////////////////////////////////////////////////////////////////

            Handles.color = new Color(l, l, l, 0.05f);

            for(int i = 1; i < 8; i++) //  Verticals
            {
                float step = Mathf.Lerp(m_CurveArea.xMin, m_CurveArea.xMax,(float)i / 8); 
                Handles.DrawLine(new Vector2(step, area.yMin), new Vector2(step, area.yMax));
            }

            for(int i = 1; i < 4; i++) //  Horizontals
            {
                float step = Mathf.Lerp(m_CurveArea.yMin, m_CurveArea.yMax,(float)i / 4); 
                Handles.DrawLine(new Vector2(area.xMin, step), new Vector2(area.xMax, step));
            }

            //////////////////////////////////////////////////////////////////////////////
            // Texts
            //////////////////////////////////////////////////////////////////////////////

            Rect minInRect = new Rect(m_CurveArea.xMin, m_CurveArea.yMax, 40, 12);
            Rect maxInRect = new Rect(m_CurveArea.xMax-40, m_CurveArea.yMax, 40, 12);
            Rect minOutRect = new Rect(m_CurveArea.xMin-40, m_CurveArea.yMax-12, 40, 12);
            Rect maxOutRect = new Rect(m_CurveArea.xMin-40, m_CurveArea.yMin, 40, 12);

            GUI.Label(minInRect, m_MinInput.ToString("F2"), styles.smallLabelLeftAlign);
            GUI.Label(maxInRect, m_MaxInput.ToString("F2"), styles.smallLabelRightAlign);
            GUI.Label(minOutRect, m_MinOutput.ToString("F2"), styles.smallLabelRightAlign);
            GUI.Label(maxOutRect, m_MaxOutput.ToString("F2"), styles.smallLabelRightAlign);

            //////////////////////////////////////////////////////////////////////////////
            // Text on Zero Axis'es
            //////////////////////////////////////////////////////////////////////////////

            if(m_MinInput < 0 && m_MaxInput > 0)
            {
                Handles.color = new Color(l, l, l, 0.6f);
                Handles.DrawLine(CurveToCanvas(new Vector2(0,m_MinOutput)), CurveToCanvas(new Vector2(0,m_MaxOutput)));
            }

            if(m_MinOutput < 0 && m_MaxOutput > 0)
            {
                Handles.color = new Color(l, l, l, 0.6f);
                Handles.DrawLine(CurveToCanvas(new Vector2(m_MinInput,0)),CurveToCanvas(new Vector2(m_MaxInput, 0)));
            }


            //////////////////////////////////////////////////////////////////////////////
            // Draw Curves
            //////////////////////////////////////////////////////////////////////////////

            for(int i = 0; i < m_CurvesSerializedProperties.Count; i++)
            {
                Color color = (m_SelectedCurve == i) ? m_CurveColors[i] : m_CurveColors[i]*new Color(1,1,1,0.5f);

                if (m_CurveVisibilities[i])
                {
                    if(keys[i].Length > 1)
                    { 
                        for (int k = 0; k < keys[i].Length - 1; k++)
                        {
                            if(k == 0 && keys[i][k].time > m_MinInput)
                            {
                                Vector3 source = CurveToCanvas(new Vector3(m_MinInput, keys[i][0].value));
                                Vector3 end = CurveToCanvas(new Vector3(keys[i][0].time, keys[i][0].value));
                                Handles.color = color;
                                Handles.DrawAAPolyLine(4.0f, new Vector3[] { source, end });
                            }

                            if(float.IsInfinity(keys[i][k].outTangent) || float.IsInfinity(keys[i][k+1].inTangent))
                            {
                                Vector3[] s = HardSegment(keys[i][k], keys[i][k + 1]);
                                Handles.color = color;
                                Handles.DrawAAPolyLine(3.0f, new Vector3[]{ s[0],s[1],s[2] });
                            }
                            else
                            {
                                Vector3[] s = BezierSegment(keys[i][k], keys[i][k + 1]);
                                Handles.DrawBezier(s[0], s[3], s[1], s[2], color, null, 4.0f);
                            }
                        }

                        int last = keys[i].Length - 1;
                        if(keys[i][last].time < m_MaxInput)
                        {
                            Vector3 source = CurveToCanvas(new Vector3(keys[i][last].time, keys[i][last].value));
                            Vector3 end = CurveToCanvas(new Vector3(m_MaxInput, keys[i][last].value));
                            Handles.color = color;
                            Handles.DrawAAPolyLine(4.0f, new Vector3[] { source, end });
                        }
                    }
                    else
                    {
                        Vector3 source = CurveToCanvas(new Vector3(m_MinInput, keys[i][0].value));
                        Vector3 end = CurveToCanvas(new Vector3(m_MaxInput, keys[i][0].value));
                        Handles.color = color;
                        Handles.DrawAAPolyLine(4.0f, new Vector3[] { source, end });
                    }
                }
                
            }

            //////////////////////////////////////////////////////////////////////////////
            // Handles
            //////////////////////////////////////////////////////////////////////////////

            for(int i = 0; i < m_CurvesSerializedProperties.Count; i++)
            {
                if(m_CurveVisibilities[i])
                {
                    for (int k = 0; k < keys[i].Length; k++)
                    {
                        Vector3 pos = CurveToCanvas(keys[i][k]);
                        Rect hitRect = new Rect(pos.x - 6, pos.y - 6, 12, 12);
                        RectOffset offset = new RectOffset(3, 3, 3, 3);

                        Vector3 outTgt = pos + CurveTangentToCanvas(keys[i][k].outTangent).normalized * 40.0f;
                        Vector3 inTgt = pos - CurveTangentToCanvas(keys[i][k].inTangent).normalized * 40.0f;
                        Rect inTgtHitRect = new Rect(inTgt.x - 5, inTgt.y - 5, 10, 10);
                        Rect outTgtHitrect = new Rect(outTgt.x-5, outTgt.y-5, 10, 10);

                        if (Event.current.type == EventType.repaint)
                        {
                            Color color = (i == m_SelectedCurve && k == m_SelectedKeyFrame) ? Color.yellow : m_CurveColors[i];
                            // Point Rect
                            EditorGUI.DrawRect(offset.Remove(hitRect), color);

                            // Tangent Handles
                            if(i == m_SelectedCurve)
                            {   
                                color = (i == m_SelectedCurve && k == m_SelectedKeyFrame) ? Color.yellow : Color.white;
                                Handles.color = color;

                                if(k > 0)
                                {
                                    EditorGUI.DrawRect(offset.Remove(inTgtHitRect), color);
                                    Handles.DrawAAPolyLine(2.0f,new Vector3[]{ pos, inTgt});
                                }

                                if(k < keys[i].Length - 1)
                                {
                                    EditorGUI.DrawRect(offset.Remove(outTgtHitrect), color);
                                    Handles.DrawAAPolyLine(2.0f,new Vector3[]{ pos, outTgt});
                                }
                            }
                        }

                        if(m_EditMode == EditMode.Moving && Event.current.type == EventType.MouseDrag && m_SelectedCurve == i && m_SelectedKeyFrame == k)
                        {
                            EditMoveKeyFrame(ref curves, ref keys, i, k);
                            Invalidate();
                        }

                        if(m_EditMode == EditMode.TangentEdit && Event.current.type == EventType.MouseDrag && m_SelectedCurve == i && m_SelectedKeyFrame == k)
                        {
                            bool alreadyBroken = (keys[i][k].inTangent != keys[i][k].outTangent);
                            EditMoveTangent(ref curves, ref keys, i, k, m_TangentEditMode, Event.current.shift || !(alreadyBroken || Event.current.control) );
                            Invalidate();
                        }

                        // Select Keyframe
                        if (Event.current.type == EventType.mouseDown && area.Contains(Event.current.mousePosition))
                        {
                            if(hitRect.Contains(Event.current.mousePosition))
                            {
                                Invalidate();
                                SelectKeyFrame(i, k);
                                if (Event.current.button == 1)
                                {
                                    ShowDeleteKeyframeContextMenu(ref curves, ref keys, m_SelectedCurve, m_SelectedKeyFrame);
                                    Invalidate();
                                }
                                else
                                    m_EditMode = EditMode.Moving;
                                Event.current.Use();
                            }
                        }
                        
                        // Edit Tangents in or out.
                        if (Event.current.type == EventType.mouseDown && area.Contains(Event.current.mousePosition))
                        {
                            if(inTgtHitRect.Contains(Event.current.mousePosition) && k > 0)
                            {
                                Invalidate();
                                SelectKeyFrame(i, k);
                                m_EditMode = EditMode.TangentEdit;
                                m_TangentEditMode = Tangent.In;
                                Event.current.Use();
                            }
                            else if(outTgtHitrect.Contains(Event.current.mousePosition) && k < keys[i].Length - 1)
                            {
                                Invalidate();
                                SelectKeyFrame(i, k);
                                m_EditMode = EditMode.TangentEdit;
                                m_TangentEditMode = Tangent.Out;
                                Event.current.Use();
                            }
                        }

                        if(Event.current.rawType == EventType.MouseUp && m_EditMode != EditMode.None)
                        {
                            m_EditMode = EditMode.None;
                            Invalidate();
                        }

                        EditorGUIUtility.AddCursorRect(hitRect, MouseCursor.MoveArrow);

                        if(k > 0)
                            EditorGUIUtility.AddCursorRect(inTgtHitRect, MouseCursor.RotateArrow);

                        if(k < keys[i].Length - 1)
                            EditorGUIUtility.AddCursorRect(outTgtHitrect, MouseCursor.RotateArrow);

                    }
                }
            }

            // if Hitting the background... search for curves or deselect
            if (Event.current.type == EventType.mouseDown)
            {
                Vector3 hit = CanvasToCurve(Event.current.mousePosition);

                if (Event.current.button == 1)
                {
                    ShowAddKeyframeContextMenu(ref curves, ref keys, m_SelectedCurve, hit);
                    Invalidate();
                }

                ClearSelection();

                float curveVPPickValue = CurveToCanvas(hit).y;

                for(int i =0; i < curves.Count; i++)
                {
                    Vector3 curveVPPos = CurveToCanvas(new Vector3(hit.x, curves[i].Evaluate(hit.x)));

                    if (Mathf.Abs(curveVPPos.y - curveVPPickValue) < 4.0f) // Picking at 4 pixels
                    {
                        m_SelectedCurve = i;
                        if(Event.current.clickCount == 2 && Event.current.button == 0) // Double Click
                        {
                            EditCreateKeyFrame(ref curves, ref keys, m_SelectedCurve, hit, true);
                            Invalidate();
                        }
                    }
                }

                if(Event.current.clickCount == 2 && m_SelectedCurve == -1) // Double Click
                {
                    for(int i = 0; i < curves.Count; i++)
                    {
                        EditCreateKeyFrame(ref curves, ref keys, i, hit, Event.current.alt);
                        Invalidate();
                    }
                } 

                Event.current.Use();
            }

            if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Delete)
            {

                // If selection is not null & Curve has more than one point
                if(m_SelectedKeyFrame != -1 && keys[m_SelectedCurve].Length >= 1) 
                {
                    EditDeleteKeyFrame(ref curves, ref keys, m_SelectedCurve, m_SelectedKeyFrame);
                    m_SelectedKeyFrame = -1;
                    Event.current.Use();
                }
            }

            Handles.color = Color.white;

            if (OnPostGUI != null)
                OnPostGUI(area, m_CurveArea);

            GUI.EndClip();
        }

        #region Selection & Keyframe Manipulation 

        private void SelectKeyFrame(int curve, int keyframe)
        {
            m_SelectedKeyFrame = keyframe;
            m_SelectedCurve = curve;
        }

        public void ClearSelection()
        {
            m_SelectedCurve = -1;
            m_SelectedKeyFrame = -1;
        }

        private void EditCreateKeyFrame(ref List<AnimationCurve> curves, ref List<Keyframe[]> keys, int curveIndex, Vector3 position, bool CreateOnCurve)
        {
            float tangent = EvaluateTangent(curves[curveIndex], position.x);

            if (CreateOnCurve)
                position.y = curves[curveIndex].Evaluate(position.x);

            AddKeyFrame(curves[curveIndex], curveIndex, new Keyframe(position.x, position.y, tangent, tangent));
        }

        private void EditDeleteKeyFrame(ref List<AnimationCurve> curves, ref List<Keyframe[]> keys, int curveIndex, int keyIndex)
        {
            RemoveKeyFrame(curves[curveIndex], curveIndex, keyIndex);
        }

        private void EditMoveKeyFrame(ref List<AnimationCurve> curves, ref List<Keyframe[]> keys, int curveIndex, int keyIndex)
        {
            Vector3 key = CanvasToCurve(Event.current.mousePosition);
            float inTgt = keys[curveIndex][keyIndex].inTangent;
            float outTgt = keys[curveIndex][keyIndex].outTangent;
            SetKeyFrame(curves[curveIndex], curveIndex, keyIndex, new Keyframe(key.x, key.y, inTgt, outTgt));
        }

        private void EditMoveTangent(ref List<AnimationCurve> curves, ref List<Keyframe[]> keys, int curveIndex, int keyIndex, Tangent targetTangent, bool linkTangents)
        {
            Vector3 pos = CanvasToCurve(Event.current.mousePosition);

            float time = keys[curveIndex][keyIndex].time;
            float value = keys[curveIndex][keyIndex].value;

            pos -= new Vector3(time, value);

            if(targetTangent == Tangent.In && pos.x > 0)
            {
                pos.x = 0;
            }
            if(targetTangent == Tangent.Out && pos.x < 0)
            {
                pos.x = 0;
            }

            float tangent;

            if(pos.x == 0)
                    tangent = pos.y < 0 ? float.PositiveInfinity : float.NegativeInfinity;
            else
                tangent = pos.y / pos.x;

            float inTgt = keys[curveIndex][keyIndex].inTangent;
            float outTgt = keys[curveIndex][keyIndex].outTangent; 

            if(targetTangent == Tangent.In || linkTangents)
                inTgt = tangent;
            if(targetTangent == Tangent.Out || linkTangents)
                outTgt = tangent;

            SetKeyFrame(curves[curveIndex], curveIndex, keyIndex, new Keyframe(time, value, inTgt, outTgt));
        }

        #endregion

        #region Editing API

        private void AddKeyFrame(AnimationCurve curve, int curveIndex, Keyframe newValue)
        {
            curve.AddKey(newValue);
            m_CurvesSerializedProperties[curveIndex].animationCurveValue = curve;
        }

        private void RemoveKeyFrame(AnimationCurve curve, int curveIndex, int keyframeIndex)
        {
            curve.RemoveKey(keyframeIndex);
            m_CurvesSerializedProperties[curveIndex].animationCurveValue = curve;
        }

        private void SetKeyFrame(AnimationCurve curve, int curveIndex, int keyIndex, Keyframe newValue)
        {
            if(keyIndex > 0)
            {
                newValue.time = Mathf.Max(curve.keys[keyIndex - 1].time + 0.00001f, newValue.time);
            }

            if(keyIndex < curve.keys.Length - 1)
            {
                newValue.time = Mathf.Min(curve.keys[keyIndex + 1].time - 0.00001f, newValue.time);
            }
            curve.MoveKey(keyIndex, newValue);
            m_CurvesSerializedProperties[curveIndex].animationCurveValue = curve;
        }

        #endregion

        #region Context Menus
        public void ShowAddKeyframeContextMenu(ref List<AnimationCurve> curves, ref List<Keyframe[]> keys, int curve, Vector3 position)
        {
            GenericMenu menu = new GenericMenu();
            MenuAddKeyFrameInfo add = new MenuAddKeyFrameInfo
            {
                curves = curves,
                keys = keys,
                curve = curve,
                position = position,
                createOnCurve = false
            };
            MenuAddKeyFrameInfo addOnCurve = new MenuAddKeyFrameInfo
            {
                curves = curves,
                keys = keys,
                curve = curve,
                position = position,
                createOnCurve = true
            };
            menu.AddItem(VFXToolboxGUIUtility.Get("Add Key : At Position"), false, MenuAddKeyframe, add);
            menu.AddItem(VFXToolboxGUIUtility.Get("Add Key : On Curve"), false, MenuAddKeyframe, addOnCurve);
            menu.ShowAsContext();
        }

        public void ShowDeleteKeyframeContextMenu(ref List<AnimationCurve> curves, ref List<Keyframe[]> keys, int curve, int key)
        {
            GenericMenu menu = new GenericMenu();
            MenuDeleteKeyFrameInfo del = new MenuDeleteKeyFrameInfo
            {
                curves = curves,
                keys = keys,
                curve = curve,
                key = key
            };
            menu.AddItem(VFXToolboxGUIUtility.Get("Delete Key"), true, MenuDeleteSelectedKeyframe, del);
            menu.ShowAsContext();
        }

        public void MenuAddKeyframe(object o)
        {
            MenuAddKeyFrameInfo info = (MenuAddKeyFrameInfo)o;

            List<SerializedObject> objects = new List<SerializedObject>();
            foreach(SerializedProperty curve in m_CurvesSerializedProperties)
            {
                if (!objects.Contains(curve.serializedObject))
                    objects.Add(curve.serializedObject);
            }

            foreach (SerializedObject obj in objects)
                obj.Update();

            if(info.curve != -1)
                EditCreateKeyFrame(ref info.curves, ref info.keys, info.curve, info.position, info.createOnCurve);
            else
                for(int i = 0; i < info.curves.Count; i++)
                {
                    EditCreateKeyFrame(ref info.curves, ref info.keys, i, info.position, info.createOnCurve);
                }

            foreach (SerializedObject obj in objects)
                obj.ApplyModifiedProperties();
        }

        public void MenuDeleteSelectedKeyframe(object o)
        {
            MenuDeleteKeyFrameInfo info = (MenuDeleteKeyFrameInfo)o;

            List<SerializedObject> objects = new List<SerializedObject>();
            foreach(SerializedProperty curve in m_CurvesSerializedProperties)
            {
                if (!objects.Contains(curve.serializedObject))
                    objects.Add(curve.serializedObject);
            }

            foreach (SerializedObject obj in objects)
                obj.Update();

            EditDeleteKeyFrame(ref info.curves, ref info.keys, info.curve, info.key);

            foreach (SerializedObject obj in objects)
                obj.ApplyModifiedProperties();

        }

        public struct MenuAddKeyFrameInfo
        {
            public List<AnimationCurve> curves;
            public List<Keyframe[]> keys;
            public int curve;
            public Vector3 position;
            public bool createOnCurve;
        }

        public struct MenuDeleteKeyFrameInfo
        {
            public List<AnimationCurve> curves;
            public List<Keyframe[]> keys;
            public int curve;
            public int key;
        }
        #endregion

        #region Drawing

        private Vector3[] BezierSegment(Keyframe start, Keyframe end)
        {
            Vector3[] segment = new Vector3[4];

            segment[0] = new Vector3(start.time , start.value);
            segment[3] = new Vector3(end.time , end.value);

            float middle = start.time + ((end.time - start.time) * 0.333333f);
            float middle2 = start.time + ((end.time - start.time) * 0.666666f);

            segment[1] = new Vector3(middle, ProjectTangent(start.time, start.value, start.outTangent, middle));
            segment[2] = new Vector3(middle2, ProjectTangent(end.time, end.value, end.inTangent, middle2));

            for (int i = 0; i < 4; i++)
                segment[i] = CurveToCanvas(segment[i]);

            return segment;
        }

        private Vector3[] HardSegment(Keyframe start, Keyframe end)
        {
            Vector3[] segment = new Vector3[3];

            segment[0] = CurveToCanvas(start);
            segment[1] = CurveToCanvas(new Vector3(end.time, start.value));
            segment[2] = CurveToCanvas(end);

            return segment;
        }

        private Vector3 CurveToCanvas(Keyframe keyframe)
        {
            return CurveToCanvas(new Vector3(keyframe.time, keyframe.value));
        }

        private Vector3 CurveToCanvas(Vector3 position)
        {
            Vector3 output = new Vector3((position.x - m_MinInput) / (m_MaxInput - m_MinInput), (position.y - m_MinOutput) / (m_MaxOutput - m_MinOutput));
            output.x = output.x * (m_CurveArea.xMax - m_CurveArea.xMin) + m_CurveArea.xMin;
            output.y = (1.0f - output.y) * (m_CurveArea.yMax - m_CurveArea.yMin) + m_CurveArea.yMin;
            return output;
        }

        private Vector3 CurveTangentToCanvas(float Tangent)
        {
            if(!float.IsInfinity(Tangent))
            {
                float Ratio = (m_CurveArea.width / m_CurveArea.height) / ((m_MaxInput - m_MinInput) / (m_MaxOutput - m_MinOutput));
                return new Vector3(1, -Tangent / Ratio).normalized;
            }
            else
            {
                return float.IsPositiveInfinity(Tangent) ? new Vector3(0, 1, 0) : new Vector3(0, -1, 0);
            }
        }

        private Vector3 CanvasToCurve(Vector3 position)
        {
            Vector3 output = position;
            output.x = (output.x - m_CurveArea.xMin) / (m_CurveArea.xMax - m_CurveArea.xMin);
            output.y = (output.y- m_CurveArea.yMin) / (m_CurveArea.yMax - m_CurveArea.yMin) ;
            output.x = Mathf.Lerp(m_MinInput, m_MaxInput, output.x);
            output.y = Mathf.Lerp(m_MaxOutput,m_MinOutput, output.y);
            return output;
        }

        private float ProjectTangent(float inPos, float inVal, float inTangent, float projPos)
        {
            return inVal + ((projPos - inPos) * inTangent);
        }

        private float EvaluateTangent(AnimationCurve curve, float time)
        {
            int prev = -1;
            int next = 0;
            for(int i = 0; i < curve.keys.Length; i++)
            {
                if(time > curve.keys[i].time)
                {
                    prev = i;
                    next = i + 1;
                }
                else break;
            }

            if (next == 0)
                return 0.0f;
            else if (prev == curve.keys.Length - 1)
                return 0.0f;
            else
            {
                float d = 0.001f;
                float tp = Mathf.Max(time - d, curve.keys[prev].time);
                float tn = Mathf.Min(time + d, curve.keys[next].time);

                float vp = curve.Evaluate(tp);
                float vn = curve.Evaluate(tn);

                if (tn == tp)
                    return (vn - vp > 0) ? float.PositiveInfinity : float.NegativeInfinity;

                return (vn - vp)/(tn- tp);
            }
        }

        #endregion

        #region Styles

        public Styles styles { get { if (m_Styles == null) m_Styles = new Styles(); return m_Styles; } }
        private Styles m_Styles;

        public class Styles
        {
            public GUIStyle smallLabelLeftAlign;
            public GUIStyle smallLabelRightAlign;

            public Styles()
            {
                smallLabelLeftAlign = new GUIStyle(EditorStyles.miniLabel);
                smallLabelLeftAlign.alignment = TextAnchor.MiddleLeft;
                smallLabelRightAlign = new GUIStyle(EditorStyles.miniLabel);
                smallLabelRightAlign.alignment = TextAnchor.MiddleRight;
            }
        }

        #endregion
    }
}

