using UnityEngine;
using UnityEngine.VFXToolbox.ImageSequencer;
using UnityEditorInternal;
using System.Collections.Generic;
using System;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public partial class ImageSequencer : EditorWindow
    {
        private ReorderableList m_InputFramesReorderableList;
        private ReorderableList m_ProcessorsReorderableList;
        private Vector2 m_OptionsViewScroll = Vector2.zero;
        private Vector2 m_ResizePanelMinMax = new Vector2(320.0f, 480.0f);
        private Vector2 m_MinimumSize;
        private SidePanelMode m_SidePanelViewMode = 0;
        private float m_ResizePanelValue = 360.0f;
        private bool m_Dirty = true;
        private bool m_NeedRedraw = false;
        private bool m_Resize = false;

        public void OnGUI()
        {
            titleContent = styles.title;
            m_MinimumSize = new Vector2(840, 320);

            InitializeGUI();

            if(m_CurrentAsset == null)
            {
                OnNoAssetGUI();
                return;
            }

            m_Dirty = false;

            m_CurrentAssetSerializedObject.Update();

            UpdateCanvasRect();

            if(HandleDropData()) return;

            using (new EditorGUILayout.VerticalScope())
            {
                DrawToolbar();
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Edit Panel
                    DrawEditPanelGUI();

                    // Catches input for resize the left panel
                    HandlePanelResize();

                    // Draws the preview canvas
                    if (previewCanvas.sequence.length > 1)
                        previewCanvas.displayRect = new Rect(m_ResizePanelValue, 16, position.width - m_ResizePanelValue, position.height - 116);
                    else
                        previewCanvas.displayRect = new Rect(m_ResizePanelValue, 16, position.width - m_ResizePanelValue, position.height - 16);

                    previewCanvas.OnGUI(this);

                }
            }

            // Processing Play Mode, Cooking & Autocooking
            if (previewCanvas.isPlaying && previewCanvas.sequence.length > 1)
                Invalidate();
            else
            {
                if (Event.current.type == EventType.Repaint)
                {
                    if(m_NeedRedraw)
                    {
                        previewCanvas.UpdateCanvasSequence();
                        Invalidate();
                        m_NeedRedraw = false;
                    }
                    else if((m_AutoCook && m_CurrentProcessor != null))
                    {
                        m_CurrentProcessor.RequestProcessOneFrame(previewCanvas.currentFrameIndex);
                        previewCanvas.UpdateCanvasSequence();
                        Invalidate();
                    }



                }
            }

            // if Invalidated this frame, repaint.
            if (m_Dirty) Repaint();
        }

        public void OnNoAssetGUI()
        {
            UpdateCanvasRect();

            if(HandleDropData()) return;

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.HelpBox("No Frame Sequence is currently selected.\nPlease create one within your Assets then select It in the project view.", MessageType.Info);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }

        }

        private void HandlePanelResize()
        {
            Rect resizeArea = new Rect(m_ResizePanelValue - 8, 16,16,position.height-16);

            EditorGUIUtility.AddCursorRect(resizeArea, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && resizeArea.Contains(Event.current.mousePosition))
                m_Resize = true;

            if (m_Resize)
            {
                m_ResizePanelValue = Mathf.Clamp(Event.current.mousePosition.x, m_ResizePanelMinMax.x, m_ResizePanelMinMax.y);
                UpdateCanvasRect();
                Invalidate();
            }
            RectOffset o = new RectOffset(7, 8, 0, 0);
            EditorGUI.DrawRect(o.Remove(resizeArea), new Color(0,0,0,1.0f));
            if (Event.current.type == EventType.MouseUp)
                m_Resize = false;
        }

        private bool HandleDropData()
        {
            if(sidePanelViewMode == SidePanelMode.InputFrames && DragAndDrop.paths.Length > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                if( Event.current.type == EventType.DragExited)
                {
                    List<string> texturePaths = new List<string>();
                    foreach(string path in DragAndDrop.paths)
                    {
                        if (VFXToolboxUtility.IsDirectory(path))
                            texturePaths.AddRange(VFXToolboxUtility.GetAllTexturesInPath(path));
                        else
                        {
                            VFXToolboxGUIUtility.DisplayProgressBar("Image Sequencer", "Discovering Assets...", 0.5f);
                            Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                            if(t != null)
                                texturePaths.Add(path);
                        }
                    }
                    AddInputFrame(m_InputFramesReorderableList, texturePaths);
                    VFXToolboxGUIUtility.ClearProgressBar();
                    return true;
                }
            }
            return false;
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                bool prev;

                bool bMaskR = m_PreviewCanvas.maskR;
                bool bMaskG = m_PreviewCanvas.maskG;
                bool bMaskB = m_PreviewCanvas.maskB;
                bool bMaskA = m_PreviewCanvas.maskA;
                bool bMaskRGB = bMaskR && bMaskG && bMaskB;

                GUILayout.Space(m_ResizePanelValue);

                Rect r = GUILayoutUtility.GetRect(VFXToolboxGUIUtility.GetTextAndIcon(" ", "SceneviewFx"), EditorStyles.toolbarPopup);
                if (GUI.Button(r, VFXToolboxGUIUtility.GetTextAndIcon(" ", "SceneviewFx"), EditorStyles.toolbarPopup))
                {
                    PopupWindow.Show(r, (PopupWindowContent) new CanvasConfigPopupWindowContent(this));
                }

                GUILayout.Space(20);

                bMaskRGB = GUILayout.Toggle(bMaskRGB, styles.iconRGB, EditorStyles.toolbarButton);

                if(bMaskRGB != (bMaskR && bMaskG && bMaskB))
                {
                    bMaskR = bMaskG = bMaskB = bMaskRGB;

                    m_PreviewCanvas.maskR = bMaskR;
                    m_PreviewCanvas.maskG = bMaskG;
                    m_PreviewCanvas.maskB = bMaskB;

                }

                prev = bMaskR;
                bMaskR = GUILayout.Toggle(bMaskR, VFXToolboxGUIUtility.Get("R"),styles.MaskRToggle);

                if (bMaskR != prev)
                    m_PreviewCanvas.maskR = bMaskR;

                prev = bMaskG;
                bMaskG = GUILayout.Toggle(bMaskG, VFXToolboxGUIUtility.Get("G"),styles.MaskGToggle);
                if (bMaskG != prev)
                    m_PreviewCanvas.maskG = bMaskG;

                prev = bMaskB;
                bMaskB = GUILayout.Toggle(bMaskB, VFXToolboxGUIUtility.Get("B"),styles.MaskBToggle);
                if (bMaskB != prev)
                    m_PreviewCanvas.maskB = bMaskB;

                prev = bMaskA;
                bMaskA = GUILayout.Toggle(bMaskA, VFXToolboxGUIUtility.Get("A"),styles.MaskAToggle);
                if (bMaskA != prev)
                    m_PreviewCanvas.maskA = bMaskA;

                if(m_PreviewCanvas.sequence != null && m_PreviewCanvas.numFrames > 0 && m_PreviewCanvas.currentFrame != null)
                {
                    GUILayout.Space(20.0f);

                    if(m_PreviewCanvas.mipMapCount > 0)
                    {
                        int currentMip = m_PreviewCanvas.mipMap;
                        int newMip = currentMip;

                        {
                            Rect mipRect = GUILayoutUtility.GetRect(164, 24);
                            GUI.Box(mipRect, GUIContent.none, styles.toolbarButton);

                            GUI.Label(new RectOffset(0, 0, 0, 0).Remove(mipRect), styles.iconMipMapDown);
                            newMip = (int)Mathf.Round(GUI.HorizontalSlider(new RectOffset(24,64,0,0).Remove(mipRect), (float)newMip, 0.0f, (float)m_PreviewCanvas.mipMapCount-1));
                            GUI.Label(new RectOffset(100, 0, 0, 0).Remove(mipRect), styles.iconMipMapUp);
                            if (newMip != currentMip)
                            {
                                m_PreviewCanvas.mipMap = newMip;
                            }
                            GUI.Label(new RectOffset(124, 0, 0, 0).Remove(mipRect), (m_PreviewCanvas.mipMap+1)+"/"+m_PreviewCanvas.mipMapCount, styles.toolbarLabelLeft);
                        }
                    }
                }

                if(EditorGUI.EndChangeCheck())
                {
                    m_PreviewCanvas.UpdateCanvasSequence(); // Reblit if changed the flags.
                }

                GUILayout.Space(20);

                {
                    Rect brightnessRect = GUILayoutUtility.GetRect(160, 24);
                    GUI.Box(brightnessRect, GUIContent.none, styles.toolbarButton);
                    GUI.Label(new RectOffset(4, 0, 0, 0).Remove(brightnessRect), VFXToolboxGUIUtility.GetTextAndIcon("Background|Sets the Background Brightness", "CheckerFloor"), styles.toolbarLabelLeft);

                    float newBrightness = GUI.HorizontalSlider(new RectOffset(82, 4, 0, 0).Remove(brightnessRect), previewCanvas.BackgroundBrightness, 0.0f, 1.0f);
                    if (previewCanvas.BackgroundBrightness != newBrightness)
                        previewCanvas.BackgroundBrightness = newBrightness;
                }

                GUILayout.FlexibleSpace();
            }
            
        }

        private void DrawEditPanelGUI()
        {
            m_OptionsViewScroll = EditorGUILayout.BeginScrollView(m_OptionsViewScroll, styles.scrollView,  GUILayout.Width(m_ResizePanelValue));

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(16);
                // Three Button Tabs : Mode Selection
                DrawTabbedPanelSelector();
                GUILayout.Space(16);

                switch (m_SidePanelViewMode)
                {
                    case SidePanelMode.InputFrames:
                        // Draw Input Frames Panel
                        DrawInputFramesPanelContent();
                        break;
                    case SidePanelMode.Processors:
                        // Draw Processors Edit Panel
                        DrawProcessorsPanelContent();
                        break;
                    case SidePanelMode.Export:
                        // Draw Export Panel
                        DrawExportPanelContent();
                        break;
                    default:
                        break;
                }
                GUILayout.Space(32);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTabbedPanelSelector()
        {
            int buttonHeight = 24;
            SidePanelMode prevMode = m_SidePanelViewMode;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Toggle(m_SidePanelViewMode == SidePanelMode.InputFrames, VFXToolboxGUIUtility.Get("Input Frames"), styles.TabButtonLeft, GUILayout.Height(buttonHeight)))
                {
                    if (m_SidePanelViewMode != SidePanelMode.InputFrames || (m_PreviewCanvas.sequence != m_processorStack.inputSequence))
                    {
                        m_SidePanelViewMode = SidePanelMode.InputFrames;
                        m_PreviewCanvas.sequence = m_processorStack.inputSequence;
                    }
                }

                // Disable Processors/Export if no input frames
                using (new EditorGUI.DisabledGroupScope(m_processorStack.inputSequence.frames.Count == 0))
                {
                    if (GUILayout.Toggle(m_SidePanelViewMode == SidePanelMode.Processors, VFXToolboxGUIUtility.Get("Processors"), styles.TabButtonMid, GUILayout.Height(buttonHeight)))
                    {
                        if (m_SidePanelViewMode != SidePanelMode.Processors)
                        {
                            m_SidePanelViewMode = SidePanelMode.Processors;
                            if (m_LockedPreviewProcessor != null)
                                m_PreviewCanvas.sequence = m_LockedPreviewProcessor.OutputSequence;
                            else
                            {
                                if(m_CurrentProcessor != null)
                                    m_PreviewCanvas.sequence = m_CurrentProcessor.OutputSequence;
                                else
                                {
                                    if (m_processorStack.processors.Count > 0)
                                        m_PreviewCanvas.sequence = m_processorStack.processors[m_processorStack.processors.Count - 1].OutputSequence;
                                    else
                                        m_PreviewCanvas.sequence = m_processorStack.inputSequence;
                                }
                            }
                        }
                    }

                    if (GUILayout.Toggle(m_SidePanelViewMode == SidePanelMode.Export, VFXToolboxGUIUtility.Get("Export"), styles.TabButtonRight, GUILayout.Height(buttonHeight)))
                    {
                        if (m_SidePanelViewMode != SidePanelMode.Export || (m_PreviewCanvas.sequence != m_processorStack.outputSequence))
                        {
                            m_SidePanelViewMode = SidePanelMode.Export;
                            m_PreviewCanvas.sequence = m_processorStack.outputSequence;
                        }
                    }
                }
            }

            if (prevMode != m_SidePanelViewMode)
            {
                m_PreviewCanvas.InvalidateRenderTarget();
                m_PreviewCanvas.UpdateCanvasSequence();
                m_PreviewCanvas.Invalidate(true);
            }

        }

        private void DrawInputFramesPanelContent()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(VFXToolboxGUIUtility.Get("Input Frames"),EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if(GUILayout.Button(VFXToolboxGUIUtility.Get("Clear"), GUILayout.Width(80)))
                {
                    Undo.RecordObject(m_CurrentAsset, "Clear All Input Frames");
                    // Remove frames and update hash
                    m_processorStack.RemoveAllInputFrames(m_CurrentAsset);
                    m_processorStack.SyncFramesToAsset(m_CurrentAsset);
                    m_InputFramesHashCode = GetInputTexturesHashCode();
                    // Update view
                    sidePanelViewMode = SidePanelMode.InputFrames;
                    m_CurrentProcessor = null;
                    m_LockedPreviewProcessor = null;
                    m_CurrentAsset.editSettings.lockedProcessor = -1;
                    m_CurrentAsset.editSettings.selectedProcessor = -1;
                    m_PreviewCanvas.sequence = m_processorStack.inputSequence;
                    // Request an update
                    Invalidate();
                    RefreshCanvas();
                    return;
                }
            }

            m_InputFramesReorderableList.DoLayoutList();

            if(Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Delete && m_processorStack.inputSequence.length > 0)
            {
                RemoveInputFrame(m_InputFramesReorderableList);
                Event.current.Use();
            }
        }

        private void DrawProcessorsPanelContent()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(VFXToolboxGUIUtility.Get("Frame Processor Stack"),EditorStyles.boldLabel,GUILayout.Width(180));
                GUILayout.FlexibleSpace();
                if(GUILayout.Button(VFXToolboxGUIUtility.Get("Clear"), GUILayout.Width(80)))
                {
                    // Delete everything
                    Undo.RecordObject(m_CurrentAsset, "Clear All Processors");
                    m_processorStack.RemoveAllProcessors(m_CurrentAsset);
                    // Update UI
                    m_ProcessorsReorderableList.index = -1;
                    m_CurrentProcessor = null;
                    m_LockedPreviewProcessor = null;
                    m_CurrentAsset.editSettings.lockedProcessor = -1;
                    m_CurrentAsset.editSettings.selectedProcessor = -1;
                    m_PreviewCanvas.sequence = m_processorStack.inputSequence;
                    EditorUtility.SetDirty(m_CurrentAsset);
                    // Request Repaint
                    Invalidate();
                    RefreshCanvas();
                    return;
                }
            }
            m_ProcessorsReorderableList.DoLayoutList();

            GUILayout.Space(10);

            // Draw inspector and Invalidates whatever needs to.
            for(int i = 0; i < m_processorStack.processors.Count; i++)
            {
                if(m_ProcessorsReorderableList.index == i)
                {
                    bool changed = m_processorStack.processors[i].OnSidePanelGUI(m_CurrentAsset,i);
                    if (changed)
                    {
                        m_processorStack.processors[i].Invalidate();
                        UpdateViewport();
                    }
                    m_Dirty = m_Dirty || changed;
                }
            }

            // Handle final keyboard events (delete)
            if(Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Delete && m_processorStack.processors.Count > 0)
            {
                MenuRemoveProcessor(m_ProcessorsReorderableList);
                Event.current.Use();
            }
        }

        private void DrawExportPanelContent()
        {
            VFXToolboxStyles.ToggleableHeader(true, false, "Export Frames");
            EditorGUI.indentLevel++;
            int length = m_processorStack.outputSequence.length;

            if(length > 0)
            {
                m_CurrentAssetSerializedObject.Update();
                EditorGUI.BeginChangeCheck();

                ImageSequence.ExportSettings prevState = m_CurrentAsset.exportSettings;

                ImageSequence.ExportMode prevMode = m_CurrentAsset.exportSettings.exportMode;

                m_CurrentAsset.exportSettings.exportMode = (ImageSequence.ExportMode)EditorGUILayout.Popup(VFXToolboxGUIUtility.Get("Export Format"), (int)m_CurrentAsset.exportSettings.exportMode, GetExportModeFriendlyNames());

                if (prevMode != m_CurrentAsset.exportSettings.exportMode)
                {
                    m_CurrentAsset.exportSettings.fileName = "";
                }

                switch(m_CurrentAsset.exportSettings.exportMode)
                {

                    case ImageSequence.ExportMode.EXR:
                        m_CurrentAsset.exportSettings.highDynamicRange = true;
                        m_CurrentAsset.exportSettings.sRGB = false;
                        break;
                    case ImageSequence.ExportMode.PNG:
                    case ImageSequence.ExportMode.Targa:
                        m_CurrentAsset.exportSettings.highDynamicRange = false;
                        break;

                }

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(VFXToolboxGUIUtility.Get("File Name|File name or pattern of the export sequence, using # characters will add frame number to the file name, use multiple ### to ensure leading zeroes."), m_CurrentAsset.exportSettings.fileName);
                EditorGUI.EndDisabledGroup();

                if(!m_CurrentAsset.exportSettings.highDynamicRange)
                    m_CurrentAsset.exportSettings.sRGB = EditorGUILayout.Toggle(VFXToolboxGUIUtility.Get("sRGB (Color Data)|Whether the texture contains color (or not), HDR Data is always non sRGB."), m_CurrentAsset.exportSettings.sRGB);

                EditorGUI.BeginDisabledGroup(m_CurrentAsset.exportSettings.compress && m_CurrentAsset.exportSettings.highDynamicRange);
                m_CurrentAsset.exportSettings.exportAlpha = EditorGUILayout.Toggle(VFXToolboxGUIUtility.Get("Export Alpha|Whether to export the alpha channel"), m_CurrentAsset.exportSettings.exportAlpha);
                EditorGUI.EndDisabledGroup();

                m_CurrentAsset.exportSettings.compress = EditorGUILayout.Toggle(VFXToolboxGUIUtility.Get("Compress|Whether to apply texture compression (HDR Compressed Data does not support alpha channel)"), m_CurrentAsset.exportSettings.compress);
                m_CurrentAsset.exportSettings.generateMipMaps = EditorGUILayout.Toggle(VFXToolboxGUIUtility.Get("Generate MipMaps|Whether generate mipmaps."), m_CurrentAsset.exportSettings.generateMipMaps);
                m_CurrentAsset.exportSettings.wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup(VFXToolboxGUIUtility.Get("Wrap Mode|Texture Wrap mode"), m_CurrentAsset.exportSettings.wrapMode);
                m_CurrentAsset.exportSettings.filterMode = (FilterMode)EditorGUILayout.EnumPopup(VFXToolboxGUIUtility.Get("Filter Mode|Texture Filter mode"), m_CurrentAsset.exportSettings.filterMode);

                if(m_CurrentAsset.exportSettings.compress && m_CurrentAsset.exportSettings.highDynamicRange)
                {
                    m_CurrentAsset.exportSettings.exportAlpha = false;
                }

                if(GUILayout.Button("Export as New...", GUILayout.Height(24)))
                {
                    string fileName = "";

                    fileName = ExportToFile(false);

                    if (fileName != "")
                    {
                        m_CurrentAsset.exportSettings.fileName = fileName;
                        m_CurrentAsset.exportSettings.frameCount = (ushort)m_processorStack.outputSequence.frames.Count;
                    }
                }
                // Export Again
                if( m_CurrentAsset.exportSettings.fileName != null &&
                    ((m_CurrentAsset.exportSettings.fileName.EndsWith(".tga") && m_CurrentAsset.exportSettings.exportMode == ImageSequence.ExportMode.Targa)
                    ||   (m_CurrentAsset.exportSettings.fileName.EndsWith(".exr") && m_CurrentAsset.exportSettings.exportMode == ImageSequence.ExportMode.EXR)
                    ||   (m_CurrentAsset.exportSettings.fileName.EndsWith(".png") && m_CurrentAsset.exportSettings.exportMode == ImageSequence.ExportMode.PNG)
                    ))
                {
                    if(GUILayout.Button("Update Exported Assets", GUILayout.Height(24)))
                    {
                        if (ExportToFile(true) != "")
                            m_CurrentAsset.exportSettings.frameCount = (ushort)m_processorStack.outputSequence.frames.Count;
                        else
                            m_CurrentAsset.exportSettings.frameCount = 0;
                    }
                }

                if(EditorGUI.EndChangeCheck())
                {
                    ImageSequence.ExportSettings curState = m_CurrentAsset.exportSettings;
                    m_CurrentAsset.exportSettings = prevState;
                    Undo.RecordObject(m_CurrentAsset, "Update Export Settings");
                    m_CurrentAsset.exportSettings = curState;
                    m_CurrentAssetSerializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(m_CurrentAsset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("You do not have any frames to export.", MessageType.Warning);
            }
            EditorGUI.indentLevel--;
        }

        private void UpdateCanvasRect()
        {
            previewCanvas.displayRect = new Rect(m_ResizePanelValue, 16, position.width - m_ResizePanelValue, position.height - 16 );
        }

        public void Invalidate()
        {
            m_Dirty = true;
        }

        public void UpdateViewport()
        {
            m_NeedRedraw = true;
        }

        private class CanvasConfigPopupWindowContent : PopupWindowContent
        {
            private static Styles s_Styles;
            private ImageSequencer m_Window;
            
            public CanvasConfigPopupWindowContent(ImageSequencer window)
            {
                m_Window = window;
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(200, 232);
            }

            public override void OnGUI(Rect rect)
            {
                if (s_Styles == null)
                    s_Styles = new Styles();

                using (new GUILayout.AreaScope(rect))
                {
                    bool needRepaint = false;

                    using (new GUILayout.VerticalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        DoHeaderLayout("Viewport Options");
                        m_Window.previewCanvas.showGrid = GUILayout.Toggle(m_Window.previewCanvas.showGrid, VFXToolboxGUIUtility.Get("Grid Outline"), s_Styles.menuItem );
                        m_Window.previewCanvas.showExtraInfo = GUILayout.Toggle(m_Window.previewCanvas.showExtraInfo, VFXToolboxGUIUtility.Get("Frame Processor Overlays"), s_Styles.menuItem );
                        m_Window.previewCanvas.filter = GUILayout.Toggle(m_Window.previewCanvas.filter, VFXToolboxGUIUtility.Get("Texture Filtering"), s_Styles.menuItem );

                        DoHeaderLayout("Center View");
                        if(GUILayout.Button(VFXToolboxGUIUtility.Get("Fit to Window"),s_Styles.menuItem))
                        {
                            m_Window.previewCanvas.Recenter(true);
                            needRepaint = true;
                        }

                        if(GUILayout.Button(VFXToolboxGUIUtility.Get("Reset Zoom"),s_Styles.menuItem))
                        {
                            m_Window.previewCanvas.Recenter(false);
                            needRepaint = true;
                        }

                        DoHeaderLayout("Background Options");

                        if(GUILayout.Button(VFXToolboxGUIUtility.Get("Reset Brightness"),s_Styles.menuItem))
                        {
                            m_Window.previewCanvas.ResetBrightness();
                            needRepaint = true;
                        }

                        DoHeaderLayout("Processing Options");
                        m_Window.m_AutoCook = GUILayout.Toggle(m_Window.m_AutoCook, VFXToolboxGUIUtility.Get("AutoCook"), s_Styles.menuItem );

                        if(EditorGUI.EndChangeCheck())
                        {
                            needRepaint = true;
                        }
                    }

                    if (needRepaint)
                        m_Window.Repaint();
                }

                if (Event.current.type == EventType.MouseMove)
                    Event.current.Use();

                if (Event.current.type != EventType.KeyDown || Event.current.keyCode != KeyCode.Escape)
                    return;
                this.editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            private void DoHeaderLayout(string headerText)
            {
                GUILayout.Label(GUIContent.none, s_Styles.separator);
                GUILayout.Label(VFXToolboxGUIUtility.Get(headerText), EditorStyles.boldLabel);
            }

            private class Styles
            {
                public readonly GUIStyle menuItem = (GUIStyle)"MenuItem";
                public readonly GUIStyle separator = (GUIStyle)"sv_iconselector_sep";
            }
        }

    }
}
