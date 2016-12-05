using UnityEngine;
using System.Linq;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class ImageSequencerCanvas
    {
        public Rect displayRect
        {
            get { return m_Rect; }
            set { m_Rect = value; }
        }

        public bool maskR
        {
            get { return m_Material.GetColor("_RGBAMask").r == 1.0f; }
            set
            {
                Color c = m_Material.GetColor("_RGBAMask");
                if (value) c.r = 1.0f; else c.r = 0.0f;
                m_Material.SetColor("_RGBAMask", c);
                m_ImageSequencerWindow.Invalidate();
            }
        }

        public bool maskG
        {
            get { return m_Material.GetColor("_RGBAMask").g == 1.0f; }
            set
            {
                Color c = m_Material.GetColor("_RGBAMask");
                if (value) c.g = 1.0f; else c.g = 0.0f;
                m_Material.SetColor("_RGBAMask", c);
                m_ImageSequencerWindow.Invalidate();
            }
        }

        public bool maskB
        {
            get { return m_Material.GetColor("_RGBAMask").b == 1.0f; }
            set
            {
                Color c = m_Material.GetColor("_RGBAMask");
                if (value) c.b = 1.0f; else c.b = 0.0f;
                m_Material.SetColor("_RGBAMask", c);
                m_ImageSequencerWindow.Invalidate();
            }

        }

        public bool maskA
        {
            get { return m_Material.GetColor("_RGBAMask").a == 1.0f; }
            set
            {
                Color c = m_Material.GetColor("_RGBAMask");
                if (value) c.a = 1.0f; else c.a = 0.0f;
                m_Material.SetColor("_RGBAMask", c);
                m_ImageSequencerWindow.Invalidate();
            }
        }

        public bool filter
        {
            get {   return m_bFilter; }
            set {
                    if(m_bFilter!=value)
                    {
                        m_bFilter = value;
                        InvalidateRenderTarget();
                    }
                    
                }
        }
    
        public int mipMap
        {
            get {
                    return m_MipMap;
            }
            set {
                    if(m_MipMap != value)
                    {
                        m_MipMap = value;
                        m_Material.SetFloat("_MipMap", (float)value);
                        InvalidateRenderTarget();
                    }
                    else
                    {
                        m_MipMap = value;
                    }
            }
        }

        public int mipMapCount
        {
            get {
                if (currentFrame != null)
                    return currentFrame.mipmapCount;
                else
                    return 0;
            }
        }

        public bool showGrid
        {
            get
            {
                return m_bShowGrid;
            }
            set
            {
                m_bShowGrid = value;
            }
        }

        public bool showExtraInfo
        {
            get
            {
                return m_bShowExtraInfo;
            }
            set
            {
                m_bShowExtraInfo = value;
            }
        }

        public int numFrames
        {
            get
            {
                return m_PreviewSequence.length;
            }
        }
       
        public ProcessingFrameSequence sequence
        {
            get
            {
                return m_PreviewSequence;
            }
            set
            {
                m_PreviewSequence = value;
            }
        }

        public ProcessingFrame currentFrame
        {
            get {
                return m_ProcessingFrame;
            }
            set {
                m_ProcessingFrame = value;
                if(value != null)
                    InvalidateRenderTarget();
            }
        }

        public int currentFrameIndex
        {
            get
            {
                return m_CurrentFrame;
            }
            set
            {
                if (m_CurrentFrame != value)
                {
                    m_CurrentFrame = value;
                    UpdateCanvasSequence();
                }
            } 
        }

        public bool isPlaying
        {
            get
            {
                return m_IsPlaying;
            }
        }

        public float BackgroundBrightness
        {
            get
            {
                return m_bgBrightness;
            }
            set
            {
                m_bgBrightness = value; ImageSequencer.styles.SetBGBrightness(value);
            }
        }

        public Styles styles
        {
            get
            {
                if (m_Styles == null)
                    m_Styles = new Styles(this);
                return m_Styles;
            }
        }

        private Vector2 m_CameraPosition = Vector2.zero;
        private float m_Zoom = 1.0f;
        private bool m_DragPreview = false;
        private bool m_ZoomPreview = false;
        private Vector2 m_ZoomPreviewCenter;
        private Vector2 m_PreviousMousePosition;

        private Styles m_Styles;

        private Vector2 m_ZoomMinMax = new Vector2(0.2f, 10.0f);
        private bool m_bFilter = true;
        private int m_MipMap = 0;
        private bool m_bShowGrid = true;
        private bool m_bShowExtraInfo = false;

        private int m_CurrentFrame = 0;

        private ProcessingFrameSequence m_PreviewSequence;
        private ProcessingFrame m_ProcessingFrame;
        private Rect m_Rect;
        private Rect m_PlayControlsRect;
        private ImageSequencer m_ImageSequencerWindow;

        private Shader m_Shader;
        private Material m_Material;
        private RenderTexture m_RenderTexture;

        private bool m_IsPlaying = false;
        private float m_PlayFramerate = 30.0f;
        private float m_PlayTime = 0.0f;
        private double m_EditorTime;

        private bool m_bNeedRedraw;
        private bool m_IsDirtyRenderTarget;
        private bool m_IsScrobbing;

        private float m_bgBrightness = -1.0f;

        public ImageSequencerCanvas(Rect displayRect, ImageSequencer editorWindow) 
        {
            m_Rect = displayRect;
            m_ImageSequencerWindow = editorWindow;

            m_IsScrobbing = false;
            m_IsDirtyRenderTarget = true;
            m_bNeedRedraw = true;
            m_PlayControlsRect = new Rect(16, 16, 420, 26);

            m_Shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/VFXToolbox/ImageSequencer/Editor/Shaders/ImageSequencerCanvas.shader");
            m_Material = new Material(m_Shader) { hideFlags = HideFlags.DontSave };
            m_RenderTexture = RenderTexture.GetTemporary(1,1,0);
        }

        public void Invalidate(bool needRedraw)
        {
            m_ImageSequencerWindow.Invalidate();
            m_bNeedRedraw = m_bNeedRedraw | needRedraw;
        }

        public void InvalidateRenderTarget()
        {
            m_IsDirtyRenderTarget = true;
        }

        private void UpdateRenderTarget()
        {
            Texture texture = m_ProcessingFrame.texture;
            int width = Mathf.Max(1, texture.width / (int)Mathf.Pow(2, (mipMap)));
            int height = Mathf.Max(1, texture.height / (int)Mathf.Pow(2, (mipMap)));

            if(m_RenderTexture.width != width || m_RenderTexture.height != height)
            {
                RenderTexture.ReleaseTemporary(m_RenderTexture);
                m_RenderTexture = RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGBHalf);
            }

            if (filter)
                m_RenderTexture.filterMode = FilterMode.Bilinear;
            else
                m_RenderTexture.filterMode = FilterMode.Point;

            m_IsDirtyRenderTarget = false;
            Invalidate(true);
        }

        public void Recenter(bool Refit)
        {
            m_CameraPosition = Vector2.zero;
            if(Refit)
            {
                float hZoom = (m_Rect.height - 70) / currentFrame.texture.height;
                float wZoom = (m_Rect.width - 70) / currentFrame.texture.width;

                m_Zoom = Mathf.Min(hZoom, wZoom);
            }
            else
            {
                m_Zoom = 1.0f;
            }
        }

        private void Zoom(float ZoomDelta, Vector2 zoomCenter)
        {
            Vector2 centerPos = - new Vector2(zoomCenter.x - m_Rect.width / 2, zoomCenter.y - m_Rect.height / 2) - m_CameraPosition;

            float prevZoom = m_Zoom;

            m_Zoom -= ZoomDelta;
            if(m_Zoom < m_ZoomMinMax.x)
                m_Zoom = m_ZoomMinMax.x;
            else if(m_Zoom > m_ZoomMinMax.y)
                m_Zoom = m_ZoomMinMax.y;
            else
            {
                m_CameraPosition += centerPos - ((m_Zoom / prevZoom) * centerPos);
            }
        }

        private void HandleKeyboardEvents()
        {
            if(Event.current.type == EventType.keyDown)
            {
                switch(Event.current.keyCode)
                {
                    // Viewport Toggles
                    case KeyCode.F:
                        Recenter(!Event.current.shift);
                        break;
                    case KeyCode.G:
                        showGrid = !showGrid;
                        break;
                    case KeyCode.H:
                        showExtraInfo = !showExtraInfo;
                        break;
                    case KeyCode.J:
                        filter = !filter;
                        Invalidate(true);
                        break;
                    // Brightness Control
                    case KeyCode.V:
                        BrightnessDown(0.1f);
                        break;
                    case KeyCode.B:
                        ResetBrightness();
                        break;
                    case KeyCode.N:
                        BrightnessUp(0.1f);
                        break;
                    // Play Controls
                    case KeyCode.Space:
                        TogglePlaySequence();
                        break;
                    case KeyCode.LeftArrow:
                        if (Event.current.shift)
                            FirstFrame();
                        else
                            PreviousFrame();
                        break;
                    case KeyCode.RightArrow:
                        if (Event.current.shift)
                            LastFrame();
                        else
                            NextFrame();
                        break;
                    default:
                        return; // Return without using event.
                }
                Invalidate(false);
                Event.current.Use();
            }
        }

        private void DrawCurrentTexture()
        {
            Rect rect = new Rect(0, 0, m_Rect.width, m_Rect.height);

            // Pan : use Middle Mouse button or Alt+Click
            if(Event.current.type == EventType.MouseDown && (Event.current.button == 2 || (Event.current.button == 0 && Event.current.alt)))
            {
                m_DragPreview = true;
            }

            if((Event.current.rawType == EventType.MouseUp || Event.current.rawType == EventType.DragExited) && (Event.current.button == 2 || Event.current.button == 0))
            {
                m_DragPreview = false;
                Invalidate(false);
            }

            if((!m_DragPreview && Event.current.alt) || m_DragPreview)
            {
               EditorGUIUtility.AddCursorRect(rect, MouseCursor.Pan);
               Invalidate(false);
            }

            if(m_DragPreview && Event.current.type == EventType.MouseDrag)
            {
                m_CameraPosition -= Event.current.delta;
                Invalidate(false);
            }

            // Zoom : using MouseWheel
            if (Event.current.type == EventType.ScrollWheel && rect.Contains(Event.current.mousePosition) )
            {
                // Delta negative when zooming In, Positive when zooming out
                Zoom(Event.current.delta.y * 0.05f, Event.current.mousePosition);
                m_ImageSequencerWindow.Invalidate();
            }


            // Zoom : using Alt + RightClick
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1  && Event.current.alt)
            {
                m_ZoomPreview = true;
                m_ZoomPreviewCenter = Event.current.mousePosition;
                m_PreviousMousePosition = m_ZoomPreviewCenter;
            }

            if (Event.current.rawType == EventType.MouseUp && Event.current.button == 1)
            {
                m_ZoomPreview = false;
            }

            if(m_ZoomPreview)
            {
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Zoom);
                Vector2 mouseDelta = Event.current.mousePosition - m_PreviousMousePosition;
                Zoom((mouseDelta.x + mouseDelta.y) * -0.002f, m_ZoomPreviewCenter);
                m_ImageSequencerWindow.Invalidate();
                m_PreviousMousePosition = Event.current.mousePosition;
            }

            // Draw Texture
            if(Event.current.type == EventType.Repaint)
            {

                Texture texture = currentFrame.texture;
                GUI.DrawTexture
                    (
                        new Rect(
                        (rect.width/2) - m_CameraPosition.x - (texture.width*m_Zoom*0.5f),
                        (rect.height/2) - m_CameraPosition.y - (texture.height*m_Zoom*0.5f),
                        texture.width*m_Zoom,
                        texture.height*m_Zoom
                        ),
                    m_RenderTexture,
                    ScaleMode.ScaleToFit
                    );
            }

        }

        public Vector2 CanvasToScreen(Vector2 Position)
        {
            return new Vector2((m_Rect.width / 2) - m_CameraPosition.x - (Position.x * m_Zoom), (m_Rect.height / 2) - m_CameraPosition.y - (Position.y * m_Zoom));
        }

        private void DrawGrid()
        {
            int GridNumU = m_PreviewSequence.numU;
            int GridNumV =  m_PreviewSequence.numV;

            Vector2 src, dst;
            float v;
            Texture texture = currentFrame.texture;
            if(BackgroundBrightness < 0.5f)
                Handles.color = new Color(1.0f,1.0f,1.0f,0.33333f);
            else
                Handles.color = new Color(0.0f,0.0f,0.0f,0.66666f);

            for(int i = 0; i <= GridNumV; i++)
            {
                v = -(texture.height * 0.5f) + (float)i / GridNumV * texture.height;
                src = CanvasToScreen(new Vector2(-texture.width * 0.5f, v));
                dst = CanvasToScreen(new Vector2(texture.width * 0.5f, v));
                Handles.DrawLine(src, dst);
            }
            for(int j = 0; j <= GridNumU; j++)
            {
                v = -(texture.width * 0.5f) + (float)j / GridNumU * texture.width;
                src = CanvasToScreen(new Vector2(v,-texture.height * 0.5f));
                dst = CanvasToScreen(new Vector2(v, texture.height * 0.5f));
                
                Handles.DrawLine(src, dst);
            }
            Handles.color = Color.white;
        }

        private void BlitIntoRenderTarget()
        {
            // Backup GUI RenderTarget
            var oldrendertarget = RenderTexture.active;

            Graphics.Blit(currentFrame.texture, m_RenderTexture, m_Material);

            // Restore GUI RenderTarget
            RenderTexture.active = oldrendertarget;
        }
       
        public void OnGUI(ImageSequencer editor)
        {

            if(m_bgBrightness < 0.0f)
            {
                ResetBrightness();
            }

            // Focus taken when clicked in viewport
            if(Event.current.type == EventType.mouseDown && m_Rect.Contains(Event.current.mousePosition))
            {
                GUI.FocusControl("");
            }

            if(currentFrame != null && Event.current.type == EventType.Repaint)
            {
                if (m_IsDirtyRenderTarget)
                    UpdateRenderTarget();

                if(m_bNeedRedraw)
                {
                    BlitIntoRenderTarget();
                    m_bNeedRedraw = false;
                }
            }

            GUI.BeginGroup(m_Rect);
            Rect LocalRect = new Rect(Vector2.zero, m_Rect.size);

            GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            GUI.DrawTextureWithTexCoords(LocalRect, ImageSequencer.styles.BackgroundTexture, new Rect(0, 0, m_Rect.width / 64, m_Rect.height / 64));
            GL.sRGBWrite = false;

            if (currentFrame != null)
            {
                HandleKeyboardEvents();

                GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear);
                DrawCurrentTexture();
                GL.sRGBWrite = false;

                if(showGrid) DrawGrid();
                if(editor.currentProcessor != null && editor.currentProcessor.Enabled && m_bShowExtraInfo)
                    editor.currentProcessor.OnCanvasGUI(this);
            }
            else
                GUI.Label(LocalRect, VFXToolboxGUIUtility.Get("No Texture"), EditorStyles.centeredGreyMiniLabel);

            GL.sRGBWrite = false;

            GUI.EndGroup();
            string procName = (editor.sidePanelViewMode == ImageSequencer.SidePanelMode.Export) ? "Export" : (sequence.processor == null ? "Input Frames" : sequence.processor.ToString());
            GUI.Label(new RectOffset(24,24,24,24).Remove(m_Rect), procName , styles.largeLabel);
            GUI.Label(new RectOffset(24,24,64,24).Remove(m_Rect), GetDebugInfoString() , styles.label);

            if(sequence != null && sequence.length > 1)
            {
                DrawSequenceControls(displayRect, editor);
            }

            if (m_IsPlaying)
                UpdatePlay();
        }

        private GUIContent GetDebugInfoString()
        {
            string output = "";
            if(m_ProcessingFrame != null)
            {
                output += "Frame Size : " + m_ProcessingFrame.texture.width + " x " + m_ProcessingFrame.texture.height;
                output += "\nMipMaps : " + m_ProcessingFrame.mipmapCount;
            }
            return new GUIContent(output);

        }

        public void DrawSequenceControls(Rect ViewportArea, ImageSequencer editor)
        {
            m_PlayControlsRect = new Rect(ViewportArea.x , (ViewportArea.y + ViewportArea.height), ViewportArea.width , 100);

            using (new GUILayout.AreaScope(m_PlayControlsRect, GUIContent.none, ImageSequencer.styles.playbackControlWindow))
            {
                Rect area = new Rect(16,16,m_PlayControlsRect.width-32,m_PlayControlsRect.height-32);
                GUILayout.BeginArea(area);
                using (new GUILayout.VerticalScope())
                {
                    // TRACKBAR
                    int count = sequence.length;

                    GUILayout.Space(16); // Reserve Layout for labels
                    Rect bar_rect = GUILayoutUtility.GetRect(area.width, 16);

                    EditorGUIUtility.AddCursorRect(bar_rect, MouseCursor.ResizeHorizontal);
                    if(Event.current.type == EventType.mouseDown && bar_rect.Contains(Event.current.mousePosition))
                    {
                        m_IsScrobbing = true;
                    }

                    if(Event.current.type == EventType.mouseUp || Event.current.rawType == EventType.MouseUp)
                    {
                        m_IsScrobbing = false;
                    }

                    if(m_IsScrobbing && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.mouseDown))
                    {
                        float pos = (Event.current.mousePosition.x - bar_rect.x) / bar_rect.width;
                        int frame = (int)Mathf.Round(pos * numFrames);
                        if (frame != currentFrameIndex)
                        {
                            currentFrameIndex = frame;
                            Invalidate(true);
                        }
                    }
                    
                    EditorGUI.DrawRect(bar_rect, ImageSequencer.styles.CookBarDirty);

                    float width = bar_rect.width / count;

                    Rect textpos;

                    for (int i = 0; i < count; i++)
                    {
                        if(!sequence.frames[i].dirty)
                        {
                            Rect cell = new Rect(bar_rect.x + i * width, bar_rect.y, width, bar_rect.height);
                            EditorGUI.DrawRect(cell, ImageSequencer.styles.CookBarCooked);
                        }

                        if(i == currentFrameIndex)
                        {
                            Rect cursor = new Rect(bar_rect.x + i * width, bar_rect.y, width, bar_rect.height);
                            EditorGUI.DrawRect(cursor, new Color(1.0f,1.0f,1.0f,0.5f));
                        }

                        // Labels : Every multiple of 10 based on homemade formula
                        int step = 10 * (int)Mathf.Max(1,Mathf.Floor(8*(float)count / bar_rect.width));

                        if( ((i+1) % step) == 0 )
                        {
                            textpos = new Rect(bar_rect.x + i * width, bar_rect.y - 16, 32, 16);
                            GUI.Label(textpos, (i+1).ToString(), EditorStyles.largeLabel);
                            Rect cursor = new Rect(bar_rect.x + i * width, bar_rect.y, 1, bar_rect.height);
                            EditorGUI.DrawRect(cursor, new Color(1.0f,1.0f,1.0f,0.2f));
                        }
                    }

                    // Labels : First 
                    textpos = new Rect(bar_rect.x, bar_rect.y - 16, 32, 16);
                    GUI.Label(textpos, VFXToolboxGUIUtility.Get("1"), EditorStyles.largeLabel);

                    GUILayout.Space(16);

                    // PLAY CONTROLS

                    bool lastplay;
                    using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        lastplay = m_IsPlaying;
                        if(GUILayout.Button(ImageSequencer.styles.iconFirst, ImageSequencer.styles.toolbarButton, GUILayout.Width(32)))
                        {
                            FirstFrame();
                        }

                        if(GUILayout.Button(ImageSequencer.styles.iconBack,ImageSequencer.styles.toolbarButton, GUILayout.Width(24)))
                        {
                            PreviousFrame();
                        }

                        bool playing = GUILayout.Toggle(m_IsPlaying,ImageSequencer.styles.iconPlay, ImageSequencer.styles.toolbarButton, GUILayout.Width(24));
                        if(m_IsPlaying != playing)
                        {
                            TogglePlaySequence();
                        }

                        if(GUILayout.Button(ImageSequencer.styles.iconForward, ImageSequencer.styles.toolbarButton, GUILayout.Width(24)))
                        {
                            NextFrame();
                        }

                        if(GUILayout.Button(ImageSequencer.styles.iconLast, ImageSequencer.styles.toolbarButton, GUILayout.Width(32)))
                        {
                            LastFrame();
                        }

                        if (lastplay != m_IsPlaying)
                        {
                            m_EditorTime = EditorApplication.timeSinceStartup;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(VFXToolboxGUIUtility.GetTextAndIcon("Frame : ","Profiler.Record"), ImageSequencer.styles.toolbarButton);
                        m_CurrentFrame = Mathf.Clamp(EditorGUILayout.IntField(m_CurrentFrame+1, ImageSequencer.styles.toolbarTextField, GUILayout.Width(42))-1,0,numFrames-1);
                        GUILayout.Label(" on " + numFrames + " ( TCR : " + GetTCR(m_CurrentFrame, (int)m_PlayFramerate) + " ) " , ImageSequencer.styles.toolbarButton);
                        GUILayout.FlexibleSpace();
                        ShowFrameratePopup();
                    }
                }
                GUILayout.EndArea();
            }
        }

        public void UpdateCanvasSequence()
        {
            int length;
            if (sequence.processor != null)
                length = sequence.processor.GetProcessorSequenceLength();
            else
                length = sequence.length;

            if (length > 0)
            {
                currentFrameIndex = Mathf.Clamp(currentFrameIndex, 0, length - 1);
                currentFrame = sequence.RequestFrame(currentFrameIndex);
            }
            else
                currentFrame = null;
        }

        #region PLAY CONTROLS

        private void TogglePlaySequence()
        {
            if(m_IsPlaying)
                StopSequence();
            else
                PlaySequence();
        }

        private void PlaySequence()
        {
            m_IsPlaying = true;
            m_PlayTime = currentFrameIndex / m_PlayFramerate;
            m_EditorTime = EditorApplication.timeSinceStartup;
        }

        private void StopSequence()
        {
            m_IsPlaying = false;
        }

        private void NextFrame()
        {
            int frame = currentFrameIndex + 1;
            if (frame >= numFrames) frame = 0;
            currentFrameIndex = frame;
        }

        private void PreviousFrame()
        {
            int frame = currentFrameIndex - 1;
            if (frame < 0) frame = numFrames - 1;
            currentFrameIndex = frame;
        }

        private void FirstFrame()
        {
            currentFrameIndex = 0;
            m_PlayTime = 0;
        }

        private void LastFrame()
        {
            currentFrameIndex = numFrames - 1;
            m_PlayTime =  (numFrames - 1) / m_PlayFramerate ;
        }

        private string GetTCR(int frame, int framerate)
        {
            int frames = frame % framerate;
            int seconds = frame / framerate;
            int minutes = seconds / 60;
            seconds %= 60;
            minutes %= 60;
            int numbering = (int)Mathf.Max(2,Mathf.Floor(Mathf.Log10(framerate))+1); // Minimum 2 digits
            return minutes.ToString("D2") + ":" + seconds.ToString("D2") + ":" + frames.ToString("D"+numbering);
        }

        private void ShowFrameratePopup()
        {
            if(GUILayout.Button(VFXToolboxGUIUtility.GetTextAndIcon("Speed","SpeedScale"),EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(VFXToolboxGUIUtility.Get("5 fps"),false, () =>{ m_PlayFramerate = 5; });
                menu.AddItem(VFXToolboxGUIUtility.Get("10 fps"),false, () =>{ m_PlayFramerate = 10; });
                menu.AddItem(VFXToolboxGUIUtility.Get("15 fps"),false, () =>{ m_PlayFramerate = 15; });
                menu.AddItem(VFXToolboxGUIUtility.Get("20 fps"),false, () =>{ m_PlayFramerate = 20; });
                menu.AddItem(VFXToolboxGUIUtility.Get("24 fps (Cine)"),false, () =>{ m_PlayFramerate = 24; });
                menu.AddItem(VFXToolboxGUIUtility.Get("25 fps (PAL)"),false, () =>{ m_PlayFramerate = 25; });
                menu.AddItem(VFXToolboxGUIUtility.Get("29.97 fps (NTSC)"),false, () =>{ m_PlayFramerate = 29.97f; });
                menu.AddItem(VFXToolboxGUIUtility.Get("30 fps"),false, () =>{ m_PlayFramerate = 30; });
                menu.AddItem(VFXToolboxGUIUtility.Get("50 fps"),false, () =>{ m_PlayFramerate = 50; });
                menu.AddItem(VFXToolboxGUIUtility.Get("60 fps"),false, () =>{ m_PlayFramerate = 60; });
                menu.ShowAsContext();
            }
            m_PlayFramerate = EditorGUILayout.FloatField(m_PlayFramerate, ImageSequencer.styles.toolbarTextField,GUILayout.Width(24));
            EditorGUILayout.LabelField(VFXToolboxGUIUtility.Get("fps"),GUILayout.Width(24));
        }

        private void UpdatePlay()
        {
            double deltaTime = EditorApplication.timeSinceStartup - m_EditorTime;
            m_PlayTime += (float)deltaTime;
            m_PlayTime = m_PlayTime % ((1.0f / m_PlayFramerate) * numFrames);
            currentFrameIndex = (int)Mathf.Floor(m_PlayTime*m_PlayFramerate);
            m_EditorTime = EditorApplication.timeSinceStartup;
        }

        #endregion

        #region BRIGHTNESS CONTROLS

        public void ResetBrightness()
        {
            if (EditorGUIUtility.isProSkin)
                BackgroundBrightness = 0.2f;
            else
                BackgroundBrightness = 0.4f;
        }

        public void BrightnessUp(float value)
        {
            BackgroundBrightness = Mathf.Min(BackgroundBrightness + value,1.0f);
        }

        public void BrightnessDown(float value)
        {
            BackgroundBrightness = Mathf.Max(0.0f, BackgroundBrightness - value);
        }

        #endregion

        #region STYLES



        public class Styles
        {
            public GUIStyle miniLabel
            {
                get { return m_Canvas.BackgroundBrightness < 0.5f ? m_ViewportMiniLabel : m_ViewportMiniLabelDark;  } 
            }

            public GUIStyle miniLabelRight
            {
                get { return m_Canvas.BackgroundBrightness < 0.5f ? m_ViewportMiniLabelRight : m_ViewportMiniLabelRightDark;  } 
            }

            public GUIStyle miniLabelCenter
            {
                get { return m_Canvas.BackgroundBrightness < 0.5f ? m_ViewportMiniLabelCenter : m_ViewportMiniLabelCenterDark;  } 
            }

            public GUIStyle label
            {
                get { return m_Canvas.BackgroundBrightness < 0.5f ? m_ViewportLabel : m_ViewportLabelDark;  } 
            }

            public GUIStyle largeLabel
            {
                get { return m_Canvas.BackgroundBrightness < 0.5f ? m_ViewportLargeLabel : m_ViewportLargeLabelDark;  } 
            }

            public Color backgroundPanelColor
            {
                get { return m_Canvas.BackgroundBrightness < 0.5f ? m_BackgroundPanelColor : m_BackgroundPanelColorDark; }
            }

            public Color red { get { return m_Canvas.BackgroundBrightness < 0.5f ? new Color(1, 0, 0, 1) : new Color(0.7f, 0, 0, 1); } }
            public Color green { get { return m_Canvas.BackgroundBrightness < 0.5f ? new Color(0, 1, 0, 1) : new Color(0, 0.5f, 0, 1); } }
            public Color blue { get { return m_Canvas.BackgroundBrightness < 0.5f ? new Color(0, 0, 1, 1) : new Color(0, 0, 0.5f, 1); } }
            public Color white { get { return m_Canvas.BackgroundBrightness < 0.5f ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 1); } }
            public Color black { get { return m_Canvas.BackgroundBrightness < 0.5f ? new Color(0, 0, 0, 1) : new Color(1, 1, 1, 1); } }
            public Color yellow { get { return m_Canvas.BackgroundBrightness < 0.5f ? new Color(1.0f, 0.8f, 0.25f) : new Color(0.5f, 0.4f, 0.1f); } }
            public Color cyan { get { return m_Canvas.BackgroundBrightness < 0.5f ? new Color(0.25f, 0.8f, 1.0f) : new Color(0.1f, 0.4f, 0.5f); } }
            public Color fadewhite  { get { return m_Canvas.BackgroundBrightness < 0.5f ? new Color(1, 1, 1, 0.25f) : new Color(0, 0, 0, 0.25f); } }

            private GUIStyle m_ViewportMiniLabel;
            private GUIStyle m_ViewportMiniLabelDark;
            private GUIStyle m_ViewportMiniLabelRight;
            private GUIStyle m_ViewportMiniLabelRightDark;
            private GUIStyle m_ViewportMiniLabelCenter;
            private GUIStyle m_ViewportMiniLabelCenterDark;
            private GUIStyle m_ViewportLabel;
            private GUIStyle m_ViewportLabelDark;
            private GUIStyle m_ViewportLargeLabel;
            private GUIStyle m_ViewportLargeLabelDark;

            private ImageSequencerCanvas m_Canvas;

            private Color m_BackgroundPanelColor;
            private Color m_BackgroundPanelColorDark;

            public Styles(ImageSequencerCanvas canvas)
            {
                m_Canvas = canvas;

                Color lightGray = new Color(0.8f, 0.8f, 0.8f, 1.0f);
                Color darkGray = new Color(0.2f, 0.2f, 0.2f, 1.0f);

                m_ViewportMiniLabel = new GUIStyle(EditorStyles.miniLabel);
                m_ViewportMiniLabel.normal.textColor = lightGray;
                m_ViewportMiniLabelDark = new GUIStyle(EditorStyles.miniLabel);
                m_ViewportMiniLabelDark.normal.textColor = darkGray;

                m_ViewportMiniLabelRight = new GUIStyle(m_ViewportMiniLabel);
                m_ViewportMiniLabelRight.alignment = TextAnchor.MiddleRight;
                m_ViewportMiniLabelRightDark = new GUIStyle(m_ViewportMiniLabelDark);
                m_ViewportMiniLabelRightDark.alignment = TextAnchor.MiddleRight;

                m_ViewportMiniLabelCenter = new GUIStyle(m_ViewportMiniLabel);
                m_ViewportMiniLabelCenter.alignment = TextAnchor.MiddleCenter;
                m_ViewportMiniLabelCenterDark = new GUIStyle(m_ViewportMiniLabelDark);
                m_ViewportMiniLabelCenterDark.alignment = TextAnchor.MiddleCenter;

                m_ViewportLabel = new GUIStyle(EditorStyles.largeLabel);
                m_ViewportLabel.normal.textColor = lightGray;

                m_ViewportLabelDark = new GUIStyle(EditorStyles.largeLabel);
                m_ViewportLabelDark.normal.textColor = darkGray;

                m_ViewportLargeLabel = new GUIStyle(EditorStyles.largeLabel);
                m_ViewportLargeLabel.fontSize = 24;
                m_ViewportLargeLabel.normal.textColor = lightGray;

                m_ViewportLargeLabelDark = new GUIStyle(EditorStyles.largeLabel);
                m_ViewportLargeLabelDark.fontSize = 24;
                m_ViewportLargeLabelDark.normal.textColor = darkGray;

                m_BackgroundPanelColor = new Color(0.02f, 0.02f, 0.02f, 0.85f);
                m_BackgroundPanelColorDark = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }
        #endregion

    }
}
