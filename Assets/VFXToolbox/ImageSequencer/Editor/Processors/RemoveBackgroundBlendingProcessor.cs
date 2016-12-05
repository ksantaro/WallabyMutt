using UnityEngine;
using UnityEngine.VFXToolbox.ImageSequencer;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class RemoveBackgroundBlendingProcessor : GPUFrameProcessor<RemoveBackgroundSettings>
    {

        public RemoveBackgroundBlendingProcessor(FrameProcessorStack processorStack, ProcessorInfo info)
            : base("Assets/VFXToolbox/ImageSequencer/Editor/Shaders/Unblend.shader",processorStack, info)
        { }

        public override bool OnCanvasGUI(ImageSequencerCanvas canvas)
        {
            return false; 
        }

        protected override bool DrawSidePanelContent(bool hasChanged)
        {
            var bgColor = m_SerializedObject.FindProperty("BackgroundColor");

            EditorGUI.BeginChangeCheck();

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(bgColor, VFXToolboxGUIUtility.Get("Background Color"));
                if(GUILayout.Button(VFXToolboxGUIUtility.Get("Grab"),GUILayout.Width(40)))
                {
                    if(InputSequence.length > 0)
                    {
                        InputSequence.RequestFrame(0);

                        if (InputSequence.frames[0].texture is RenderTexture)
                        {
                            Color background = VFXToolboxUtility.ReadBack((RenderTexture)InputSequence.frames[0].texture)[0];
                            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                                background = background.gamma;

                            bgColor.colorValue = background;
                        }
                        else
                        {
                            Texture2D inputFrame = (Texture2D)InputSequence.frames[0].texture;
                            Texture2D readable = new Texture2D(inputFrame.width, inputFrame.height, inputFrame.format, inputFrame.mipmapCount > 1);
                            Graphics.CopyTexture(inputFrame, readable);
                            bgColor.colorValue = readable.GetPixel(0, 0);
                        }

                    }
                }
            }

            if(EditorGUI.EndChangeCheck())
            {
                UpdateOutputSize();
                Invalidate();
                hasChanged = true;
            }
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("Please select a color corresponding to the solid background of the flipbook to try to reconstruct the pixel's color. \n\nThis filter will only work if your flipbook was rendered on a solid color background. Try the Grab button to fetch the upper left pixel of the first frame, or use the color picker.", MessageType.Info);

            return hasChanged;

        }

        public override bool Process(int frame)
        {
            Texture tex = InputSequence.RequestFrame(frame).texture;
            SetOutputSize(tex.width, tex.height);
            m_Material.SetTexture("_MainTex", tex);
            m_Material.SetColor("_BackgroundColor", settings.BackgroundColor);
            ExecuteShaderAndDump(frame, tex);
            return true;
        }

        public override string GetName()
        {
            return "Remove Background";
        }
    }
}
