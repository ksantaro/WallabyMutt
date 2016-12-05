namespace UnityEngine.VFXToolbox.ImageSequencer
{
    public class RotateProcessorSettings : ProcessorSettingsBase
    {
        public enum RotateMode
        {
            None = 0,
            Rotate90 = 1,
            Rotate180 = 2,
            Rotate270 = 3
        }

        public RotateMode FrameRotateMode;

        public override void Default()
        {
            FrameRotateMode = 0;
        }

    }
}