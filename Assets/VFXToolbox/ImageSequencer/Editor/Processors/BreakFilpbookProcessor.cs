using UnityEngine;
using UnityEngine.VFXToolbox.ImageSequencer;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class BreakFlipbookProcessor : GPUFrameProcessor<BreakFilpbookProcessorSettings>
    {

        public BreakFlipbookProcessor(FrameProcessorStack stack, ProcessorInfo info)
            : base("Assets/VFXToolbox/ImageSequencer/Editor/Shaders/GetSubUV.shader", stack, info)
        { }

        protected override void UpdateOutputSize()
        {
            int width = (int) Mathf.Ceil((float)InputSequence.RequestFrame(0).texture.width / settings.FlipbookNumU);
            int height = (int) Mathf.Ceil((float)InputSequence.RequestFrame(0).texture.height / settings.FlipbookNumV);
            SetOutputSize(width, height);
        }

        public override string GetName()
        {
            return "Break Flipbook";
        }

        public override int GetProcessorSequenceLength()
        {
            return settings.FlipbookNumU * settings.FlipbookNumV;
        }

        public override bool OnCanvasGUI(ImageSequencerCanvas canvas)
        {
            return false;
        }

        public override bool Process(int frame)
        {
            Texture texture = InputSequence.RequestFrame(0).texture;
            m_Material.SetTexture("_MainTex", texture);
            Vector4 rect = new Vector4();
            int x = frame % settings.FlipbookNumU;
            int y = (int)Mathf.Floor((float)frame / settings.FlipbookNumU);
            rect.x = (float)x;
            rect.y = (float)(settings.FlipbookNumV-1) - y;
            rect.z = 1.0f / settings.FlipbookNumU;
            rect.w = 1.0f / settings.FlipbookNumV;

            m_Material.SetVector("_Rect", rect);
            ExecuteShaderAndDump(frame, texture);
            return true;
        }

        protected override bool DrawSidePanelContent(bool hasChanged)
        {
            var flipbookNumU = m_SerializedObject.FindProperty("FlipbookNumU");
            var flipbookNumV = m_SerializedObject.FindProperty("FlipbookNumV");

            EditorGUI.BeginChangeCheck();

            int newU = Mathf.Max(1,EditorGUILayout.IntField(VFXToolboxGUIUtility.Get("Columns (U) : "),flipbookNumU.intValue));
            int newV = Mathf.Max(1,EditorGUILayout.IntField(VFXToolboxGUIUtility.Get("Rows (V) : "), flipbookNumV.intValue));

            if(newU != flipbookNumU.intValue || newV != flipbookNumV.intValue)
            {
                flipbookNumU.intValue = newU;
                flipbookNumV.intValue = newV;
            }

            if(EditorGUI.EndChangeCheck())
            {
                Invalidate();
                hasChanged = true;
            }

            return hasChanged;
        }

        protected override int GetNumU()
        {
            return 1;
        }

        protected override int GetNumV()
        {
            return 1;
        }
    }
}
