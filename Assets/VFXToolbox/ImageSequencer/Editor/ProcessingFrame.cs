using UnityEngine;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class ProcessingFrame
    {
        public Texture texture
        {
            get {
                if (m_Texture == null)
                {
                    if(m_Processor == null) // For input frames, either our input asset has been deleted, or something went wrong with the meta's, let's replace by a dummy
                    {
                        m_Texture = Missing.texture;
                        m_Texture.name = @"/!\ MISSING /!\";
                    }
                    else // For processor's outputs, Should not happen, unless reset by changing Linear/Gamma or Graphics API
                        ResetTexture();
                }
                return m_Texture;
            }
        }

        public bool isInputFrame
        {
            get { return m_Processor == null; }
        }

        public int mipmapCount
        {
            get
            {
                if (isInputFrame)
                    return ((Texture2D)texture).mipmapCount;
                else
                {
                    return (int)Mathf.Max(1,(Mathf.Log(Mathf.Max(texture.width, texture.height), 2) - 1));
                }
            }
        }

        public bool dirty;

        private Texture m_Texture;
        private FrameProcessor m_Processor;

        public ProcessingFrame(Texture texture)
        {
            m_Texture = texture;
            dirty = false;
            m_Processor = null;
        }

        public ProcessingFrame(FrameProcessor processor)
        {
            dirty = true;
            m_Processor = processor;
            ResetTexture();
        }

        public void SyncSize()
        {
            if(texture.width != m_Processor.OutputWidth || texture.height != m_Processor.OutputHeight )
            {
                ResetTexture();
            }
        }

        private void ResetTexture()
        {
            if(m_Texture == null || (m_Processor != null && (m_Texture.width != m_Processor.OutputWidth || m_Texture.height != m_Processor.OutputHeight)))
            {
                UnityEngine.Profiling.Profiler.BeginSample("ImageSequencer.ProcessingFrame.ResetTexture : " + m_Processor.GetName());
                RenderTexture.DestroyImmediate(m_Texture);
                m_Texture = new RenderTexture(m_Processor.OutputWidth, m_Processor.OutputHeight, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                ((RenderTexture)m_Texture).autoGenerateMips = true;
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public bool Process()
        {
            UnityEngine.Profiling.Profiler.BeginSample("ImageSequencer.ProcessingFrame.Process : " + m_Processor.GetName());
            if(dirty && m_Processor != null)
            {
                SyncSize();
                if(m_Processor.Process(this))
                {
                    dirty = false;
                    UnityEngine.Profiling.Profiler.EndSample();
                    return true;
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
            return false;
        }

        public void Dispose()
        {
            UnityEngine.Profiling.Profiler.BeginSample("ImageSequencer.ProcessingFrame.Dispose");
            Texture2D.DestroyImmediate(m_Texture);
            dirty = true;
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public override string ToString()
        {
            return texture.name.ToString();
        }

        public static ProcessingFrame Missing
        {
            get {
                Texture2D t = new Texture2D(1, 1);
                Color[] color = new Color[1] { Color.magenta };
                t.SetPixels(color);
                t.Resize(256, 256);
                t.Apply(true);
                return new ProcessingFrame(t);
            }
        }
    }
}
