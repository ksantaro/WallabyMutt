namespace UnityEngine.VFXToolbox.ImageSequencer
{
    public class CropProcessorSettings : ProcessorSettingsBase
    {
        public uint Crop_Top;
        public uint Crop_Bottom;
        public uint Crop_Left;
        public uint Crop_Right;

        public override void Default()
        {
                Crop_Top = 0;
                Crop_Bottom = 0;
                Crop_Left = 0;
                Crop_Right = 0;
        }
    }
}

