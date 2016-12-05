using UnityEngine;
using System.IO;
using UnityEngine.VFXToolbox.ImageSequencer;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public partial class ImageSequencer : EditorWindow
    {
        private string GetNumberedFileName(string pattern, int number, int maxFrames)
        {
            int numbering = (int)Mathf.Floor(Mathf.Log10(maxFrames))+1;
            return pattern.Replace("#", number.ToString("D" + numbering.ToString()));
        }

        public string ExportToFile(bool useCurrentFileName)
        {
            string path;
            if(useCurrentFileName)
            {
                path = m_CurrentAsset.exportSettings.fileName;
            }
            else
            {
                string title = "Save Texture, use # for frame numbering.";
                string defaultFileName, extension, message;
                switch(m_CurrentAsset.exportSettings.exportMode)
                {
                    case ImageSequence.ExportMode.EXR:
                        defaultFileName = "Frame_#.exr";
                        extension = "exr";
                        message = "Save as EXR Texture";
                        break;
                    case ImageSequence.ExportMode.Targa:
                        defaultFileName = "Frame_#.tga";
                        extension = "tga";
                        message = "Save as TGA Texture";
                        break;
                    case ImageSequence.ExportMode.PNG:
                        defaultFileName = "Frame_#.png";
                        extension = "png";
                        message = "Save as PNG Texture";
                        break;
                    default: return null;
                }

                 path = EditorUtility.SaveFilePanelInProject(title, defaultFileName, extension, message);
                if (path == null || path == "")
                    return "";
            }

            int frameCount = m_processorStack.outputSequence.length;

            if(frameCount > 1 && !Path.GetFileNameWithoutExtension(path).Contains("#"))
            {
                if (!EditorUtility.DisplayDialog("VFX Toolbox", "You are currently exporting a sequence of images with no # in filename for numbering, do you want to add _# as a postfix of the filename?", "Add Postfix", "Cancel Export"))
                    return "";

                string newpath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + "_#" + Path.GetExtension(path);
                path = newpath;
            }

            ImageSequence.ExportSettings settings = m_CurrentAsset.exportSettings;
            bool bCanceled = false;

            try
            {
                int i = 1;
                foreach (ProcessingFrame frame in m_processorStack.outputSequence.frames)
                {
                    if(VFXToolboxGUIUtility.DisplayProgressBar("Image Sequencer", "Exporting Frame #" + i + "/" + frameCount, (float)i / frameCount, 0, true))
                    {
                        bCanceled = true;
                        break;
                    }

                    Color[] inputs;

                    if (frame.texture is Texture2D)
                    {
                        RenderTexture temp = RenderTexture.GetTemporary(frame.texture.width, frame.texture.height, 0, RenderTextureFormat.ARGBHalf);
                        Graphics.Blit((Texture2D)frame.texture, temp); 
                        inputs = ReadBack(temp);
                    }
                    else // frame.texture is RenderTexture
                    {
                        frame.Process();
                        inputs = ReadBack((RenderTexture)frame.texture);
                    }

                    string fileName = GetNumberedFileName(path, i, frameCount);

                    switch(m_CurrentAsset.exportSettings.exportMode)
                    {
                        case ImageSequence.ExportMode.EXR:
                            MiniEXR.MiniEXR.MiniEXRWrite(fileName,(ushort)frame.texture.width, (ushort)frame.texture.height, settings.exportAlpha, inputs, true);
                            break;
                        case ImageSequence.ExportMode.Targa:
                            MiniTGA.MiniTGA.MiniTGAWrite(fileName,(ushort)frame.texture.width, (ushort)frame.texture.height, settings.exportAlpha, inputs);
                            break;
                        case ImageSequence.ExportMode.PNG:

                            byte[] bytes;
                            if(m_processorStack.outputSequence.processor != null)
                            {
                                RenderTexture backup = RenderTexture.active;
                                Texture2D texture = new Texture2D(frame.texture.width, frame.texture.height, TextureFormat.RGBA32, settings.generateMipMaps, !settings.sRGB);
                                RenderTexture.active = (RenderTexture)frame.texture;
                                RenderTexture ldrOutput = RenderTexture.GetTemporary(frame.texture.width, frame.texture.height, 0, RenderTextureFormat.ARGB32, settings.sRGB? RenderTextureReadWrite.sRGB: RenderTextureReadWrite.Linear);
                                GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear);
                                Graphics.Blit(frame.texture, ldrOutput);
                                RenderTexture.active = ldrOutput;
                                texture.ReadPixels(new Rect(0,0,frame.texture.width, frame.texture.height),0,0);
                                bytes = texture.EncodeToPNG();
                                RenderTexture.active = backup;
                            }
                            else
                            {
                                bytes = (frame.texture as Texture2D).EncodeToPNG();
                            }
                            File.WriteAllBytes(fileName, bytes);
                            break;
                        default: return null;
                    }

                    AssetDatabase.Refresh();

                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(fileName);
                    importer.wrapMode = m_CurrentAsset.exportSettings.wrapMode;
                    importer.filterMode = m_CurrentAsset.exportSettings.filterMode;
                    importer.textureType = TextureImporterType.Default;
                    importer.mipmapEnabled = m_CurrentAsset.exportSettings.generateMipMaps;

                    switch(m_CurrentAsset.exportSettings.exportMode)
                    {
                        case ImageSequence.ExportMode.Targa:
                            importer.sRGBTexture = m_CurrentAsset.exportSettings.sRGB;
                            importer.alphaSource = m_CurrentAsset.exportSettings.exportAlpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
                            importer.textureCompression = m_CurrentAsset.exportSettings.compress ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
                            break;
                        case ImageSequence.ExportMode.EXR:
                            importer.sRGBTexture = false;
                            importer.alphaSource = (m_CurrentAsset.exportSettings.exportAlpha && !m_CurrentAsset.exportSettings.compress) ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
                            importer.textureCompression = m_CurrentAsset.exportSettings.compress ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Uncompressed;
                            break;
                        case ImageSequence.ExportMode.PNG:
                            importer.sRGBTexture = m_CurrentAsset.exportSettings.sRGB;
                            importer.alphaSource = m_CurrentAsset.exportSettings.exportAlpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
                            importer.textureCompression = m_CurrentAsset.exportSettings.compress ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
                            break;
                    }

                    AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUpdate);
                    i++;
                }
            }
            catch(System.Exception e)
            {
                VFXToolboxGUIUtility.ClearProgressBar();
                Debug.LogError(e.Message);
            }

            VFXToolboxGUIUtility.ClearProgressBar();

            if(bCanceled)
                return "";
            else
                return path;
        }

        private Color[] ReadBack(RenderTexture renderTexture)
        {
            Color[] inputs = VFXToolboxUtility.ReadBack(renderTexture);
            Color[] outputs = new Color[inputs.Length];

            for (int j = 0; j < inputs.Length; j++)
            {

                if(QualitySettings.activeColorSpace == ColorSpace.Gamma)
                {
                    if ((!m_CurrentAsset.exportSettings.sRGB) || m_CurrentAsset.exportSettings.highDynamicRange)
                        outputs[j] = inputs[j].linear;
                    else
                        outputs[j] = inputs[j];
                }
                else
                {
                    if ((!m_CurrentAsset.exportSettings.sRGB) || m_CurrentAsset.exportSettings.highDynamicRange)
                        outputs[j] = inputs[j];
                    else
                        outputs[j] = inputs[j].gamma;
                }

                if (!m_CurrentAsset.exportSettings.exportAlpha)
                    outputs[j].a = 1.0f;
            }

            return outputs;
        }

        private static GUIContent[] GetExportModeFriendlyNames()
        {
            return new GUIContent[] { VFXToolboxGUIUtility.Get("Targa"), VFXToolboxGUIUtility.Get("OpenEXR (HDR)"), VFXToolboxGUIUtility.Get("PNG") };
        }
    }
}
