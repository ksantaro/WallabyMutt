using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFXToolbox.ImageSequencer;
using UnityEditorInternal;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public partial class ImageSequencer : EditorWindow
    {
        [MenuItem("Window/Experimental/VFX Toolbox/Image Sequencer")]
        public static void OpenEditor()
        {
            GetWindow(typeof(ImageSequencer));
        }

        public enum SidePanelMode
        {
            InputFrames = 0,
            Processors = 1,
            Export = 2
        }

        public ImageSequencerCanvas previewCanvas
        {
            get
            {
                return m_PreviewCanvas;
            }
        }

        public SidePanelMode sidePanelViewMode
        {
            get
            {
                return m_SidePanelViewMode;
            }
            set
            {
                m_SidePanelViewMode = value;
            }
        }

        public FrameProcessor currentProcessor
        {
            get
            {
                return m_CurrentProcessor;
            }
        }

        private ImageSequence m_CurrentAsset;
        private SerializedObject m_CurrentAssetSerializedObject;

        private ImageSequencerCanvas m_PreviewCanvas;
        private FrameProcessorStack m_processorStack;
        private bool m_AutoCook = false;
        private FrameProcessor m_CurrentProcessor;
        private FrameProcessor m_LockedPreviewProcessor;

        private GraphicsDeviceType m_CurrentGraphicsAPI;
        private ColorSpace m_CurrentColorSpace;

        public ImageSequencer()
        {
            Selection.selectionChanged -= OnEditorSelectionChange;
            Selection.selectionChanged += OnEditorSelectionChange;
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void OnEnable()
        {
            if(Selection.activeObject != null && Selection.activeObject is ImageSequence)
            {
                LoadAsset((ImageSequence)Selection.activeObject);
                DefaultView();
            }
            else if(m_CurrentAsset != null)
            {
                LoadAsset(m_CurrentAsset);
                DefaultView();
            }

        }

        void OnDisable()
        {
            LoadAsset(null);
            Selection.selectionChanged -= OnEditorSelectionChange;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        public void OnEditorSelectionChange()
        {
            if(Selection.activeObject != null && Selection.activeObject.GetType() == typeof(ImageSequence) && m_CurrentAsset != Selection.activeObject)
            {
                LoadAsset((ImageSequence)Selection.activeObject);
                DefaultView();
            }
            Repaint();
        }

        public void OnUndoRedo()
        {
            if (m_CurrentAsset == null)
                return;

            SidePanelMode bkpSidePanelMode = m_SidePanelViewMode;

            int hash = GetInputTexturesHashCode();

            if (m_InputFramesHashCode != hash)
                LoadAsset(m_CurrentAsset);
            else
            {
                m_processorStack.LoadProcessorsFromAsset(m_CurrentAsset);
                RestoreProcessorView();
            }

            if (m_CurrentAsset.inputFrameGUIDs.Count > 0)
                m_SidePanelViewMode = bkpSidePanelMode;
            else
                m_SidePanelViewMode = SidePanelMode.InputFrames;

            foreach(FrameProcessor p in m_processorStack.processors)
            {
                p.Refresh();
                p.Invalidate();
            }

            Repaint();
        }

        public void LoadAsset(ImageSequence asset)
        {
            m_CurrentAsset = asset;

            m_InputFramesReorderableList = null;
            m_ProcessorsReorderableList = null;
            m_LockedPreviewProcessor = null;
            m_CurrentProcessor = null;

            // Free resources if any
            if(m_processorStack != null)
                m_processorStack.Dispose();

            InitializeGUI();

            if(m_CurrentAsset != null)
            {
                m_processorStack = new FrameProcessorStack(new ProcessingFrameSequence(null), this);

                m_CurrentAssetSerializedObject = new SerializedObject(m_CurrentAsset);

                VFXToolboxGUIUtility.DisplayProgressBar("Image Sequencer", "Loading asset....", 0.0f);

                m_LockedPreviewProcessor = null;

                VFXToolboxGUIUtility.DisplayProgressBar("Image Sequencer", "Loading Frames", 0.333333f);

                m_processorStack.LoadFramesFromAsset(m_CurrentAsset);
                UpdateInputTexturesHash();

                m_InputFramesReorderableList = new ReorderableList(m_processorStack.inputSequence.frames, typeof(Texture2D),true,false,true,true);
                m_InputFramesReorderableList.onAddCallback = AddInputFrame;
                m_InputFramesReorderableList.onRemoveCallback = RemoveInputFrame;
                m_InputFramesReorderableList.onReorderCallback = ReorderInputFrame;
                m_InputFramesReorderableList.drawElementCallback = DrawInputFrameRListElement;
                m_InputFramesReorderableList.onSelectCallback = SelectInputFrameRListElement;

                VFXToolboxGUIUtility.DisplayProgressBar("Image Sequencer", "Loading Processors", 0.66666f);
                m_processorStack.LoadProcessorsFromAsset(m_CurrentAsset);

                m_ProcessorsReorderableList = new ReorderableList(m_CurrentAssetSerializedObject, m_CurrentAssetSerializedObject.FindProperty("processorInfos"),true,false,true,true);
                m_ProcessorsReorderableList.onAddCallback = ShowAddProcessorMenu;
                m_ProcessorsReorderableList.onRemoveCallback = MenuRemoveProcessor;
                m_ProcessorsReorderableList.onReorderCallback = ReorderProcessor;
                m_ProcessorsReorderableList.onSelectCallback = MenuSelectProcessor;
                m_ProcessorsReorderableList.drawElementCallback = DrawRListProcessorElement;

                m_PreviewCanvas.sequence = m_processorStack.inputSequence;
                m_PreviewCanvas.currentFrameIndex = 0;

                VFXToolboxGUIUtility.DisplayProgressBar("Image Sequencer", "Finalizing...", 1.0f);

                m_processorStack.InvalidateAll();
                RestoreProcessorView();
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// Setups the view for post load asset
        /// </summary>
        public void DefaultView()
        {
            if (m_processorStack.processors.Count == 0 || m_processorStack.inputSequence.frames.Count == 0)
            {
                m_SidePanelViewMode = SidePanelMode.InputFrames;
                m_ProcessorsReorderableList.index = -1;
                SetCurrentFrameProcessor(null, false);
            }
            else
            {
                m_SidePanelViewMode = SidePanelMode.Processors;
                RestoreProcessorView();
            }

            m_PreviewCanvas.UpdateCanvasSequence();

            if(m_PreviewCanvas != null)
                m_PreviewCanvas.Recenter(false);
        }

        /// <summary>
        /// Restores the visibility and lock of processors (on load or after an undo)
        /// </summary>
        public void RestoreProcessorView()
        {
            // index Checks
            m_CurrentAsset.editSettings.lockedProcessor = Mathf.Clamp(m_CurrentAsset.editSettings.lockedProcessor, -1, m_processorStack.processors.Count - 1);
            m_CurrentAsset.editSettings.selectedProcessor = Mathf.Clamp(m_CurrentAsset.editSettings.selectedProcessor, -1, m_processorStack.processors.Count - 1);

            // Locked processor
            if (m_CurrentAsset.editSettings.lockedProcessor != -1)
            {
                m_ProcessorsReorderableList.index = m_CurrentAsset.editSettings.lockedProcessor;
                m_LockedPreviewProcessor = m_processorStack.processors[m_CurrentAsset.editSettings.lockedProcessor];
                m_CurrentProcessor = m_processorStack.processors[m_CurrentAsset.editSettings.lockedProcessor];
            }
            else
                m_LockedPreviewProcessor = null; 

            // Selected Processor
            if(m_CurrentAsset.editSettings.selectedProcessor != -1)
            {
                m_ProcessorsReorderableList.index = m_CurrentAsset.editSettings.selectedProcessor;

                if (m_CurrentAsset.editSettings.lockedProcessor != -1)
                    m_CurrentProcessor = m_processorStack.processors[m_CurrentAsset.editSettings.lockedProcessor];
                else
                    m_CurrentProcessor = m_processorStack.processors[m_CurrentAsset.editSettings.selectedProcessor];
            }

            m_processorStack.InvalidateAll();
            RefreshCanvas();
        }

        public void InitializeGUI()
        {
            minSize = m_MinimumSize;

            if(m_PreviewCanvas == null)
            {
                m_PreviewCanvas = new ImageSequencerCanvas(new Rect(0, 16, position.width - m_ResizePanelValue,position.height - 16),this);
            }

            CheckGraphicsSettings();
        }

        private void CheckGraphicsSettings()
        {
            GraphicsDeviceType device = SystemInfo.graphicsDeviceType;
            if(m_CurrentGraphicsAPI != device)
            {
                m_CurrentGraphicsAPI = device;
                if(m_processorStack != null)
                    m_processorStack.InvalidateAll();
                Repaint();
            }
            ColorSpace colorSpace = QualitySettings.activeColorSpace;
            if(m_CurrentColorSpace != colorSpace)
            {
                m_CurrentColorSpace = colorSpace;
                if(m_processorStack != null)
                    m_processorStack.InvalidateAll();
            }
        }
    }
}

