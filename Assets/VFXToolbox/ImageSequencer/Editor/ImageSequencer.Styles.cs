using UnityEngine;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public partial class ImageSequencer : EditorWindow
    {
        static VFXToolboxStyles s_Styles = null;
        public static VFXToolboxStyles styles { get { if (s_Styles == null) s_Styles = new VFXToolboxStyles(); return s_Styles; } }

        public class VFXToolboxStyles
        {
            public GUIStyle TabButtonLeft;
            public GUIStyle TabButtonMid;
            public GUIStyle TabButtonRight;

            public GUIStyle toolbarButton;
            public GUIStyle toolbarTextField;
            public GUIStyle toolbarLabelLeft;

            public GUIStyle LargeLabel;
            public GUIStyle RListLabel;
            public GUIStyle Header;
            public GUIStyle HeaderCheckBox;
            public GUIStyle scrollView;

            public GUIStyle miniLabel;
            public GUIStyle miniLabelRight;
            public GUIStyle miniLabelCenter;


            public GUIStyle playbackControlWindow;

            public GUIContent title;

            public readonly GUIContent iconPlay = EditorGUIUtility.IconContent("Animation.Play", "Play the sequence"); 
            public readonly GUIContent iconBack = EditorGUIUtility.IconContent("Animation.PrevKey", "Go back one Frame");
            public readonly GUIContent iconForward = EditorGUIUtility.IconContent("Animation.NextKey", "Advance one Frame");
            public readonly GUIContent iconFirst = EditorGUIUtility.IconContent("Animation.FirstKey", "Go to first Frame");
            public readonly GUIContent iconLast = EditorGUIUtility.IconContent("Animation.LastKey", "Go to last Frame");

            public readonly GUIContent iconRGB = EditorGUIUtility.IconContent("PreTextureRGB", "Toggle RGB/Alpha only");
            public readonly GUIContent iconMipMapUp = EditorGUIUtility.IconContent("PreTextureMipMapLow", "Go one MipMap up (smaller size)");
            public readonly GUIContent iconMipMapDown = EditorGUIUtility.IconContent("PreTextureMipMapHigh", "Go one MipMap down (higher size)");
            


            public Texture2D BackgroundTexture { get { return m_BackgroundTexture; } }
            public Color CookBarDirty { get { if(EditorGUIUtility.isProSkin) return m_CookBarDirtyPro; else return m_CookBarDirty; } }
            public Color CookBarCooked { get { if(EditorGUIUtility.isProSkin) return m_CookBarCookedPro; else return m_CookBarCooked; } }

            private Texture2D m_BackgroundTexture;

            private Color m_CookBarDirty;
            private Color m_CookBarDirtyPro;
            private Color m_CookBarCooked;
            private Color m_CookBarCookedPro;

            public GUIStyle MaskRToggle { get { if (EditorGUIUtility.isProSkin) return m_MaskRTogglePro; else return m_MaskRToggle; } }
            public GUIStyle MaskGToggle { get { if (EditorGUIUtility.isProSkin) return m_MaskGTogglePro; else return m_MaskGToggle; } }
            public GUIStyle MaskBToggle { get { if (EditorGUIUtility.isProSkin) return m_MaskBTogglePro; else return m_MaskBToggle; } }
            public GUIStyle MaskAToggle { get { if (EditorGUIUtility.isProSkin) return m_MaskATogglePro; else return m_MaskAToggle; } }

            private GUIStyle m_MaskRToggle;
            private GUIStyle m_MaskRTogglePro;
            private GUIStyle m_MaskGToggle;
            private GUIStyle m_MaskGTogglePro;
            private GUIStyle m_MaskBToggle;
            private GUIStyle m_MaskBTogglePro;
            private GUIStyle m_MaskAToggle;
            private GUIStyle m_MaskATogglePro;


            public GUIStyle LockToggle { get { if (EditorGUIUtility.isProSkin) return m_LockTogglePro; else return m_LockToggle; } }
            private GUIStyle m_LockToggle;
            private GUIStyle m_LockTogglePro;

            public VFXToolboxStyles()
            {
                title = EditorGUIUtility.IconContent("SettingsIcon");
                title.text = "Image Seq.";

                TabButtonLeft = new GUIStyle(EditorStyles.miniButtonLeft);
                TabButtonMid = new GUIStyle(EditorStyles.miniButtonMid);
                TabButtonRight = new GUIStyle(EditorStyles.miniButtonRight);
                LargeLabel = new GUIStyle(EditorStyles.largeLabel);
                RListLabel = new GUIStyle(EditorStyles.label);

                toolbarButton = new GUIStyle(EditorStyles.toolbarButton);
                toolbarButton.padding = new RectOffset();
                toolbarButton.margin = new RectOffset();

                toolbarLabelLeft = new GUIStyle(EditorStyles.miniLabel);
                toolbarLabelLeft.alignment = TextAnchor.MiddleLeft;
                toolbarLabelLeft.contentOffset = new Vector2(-2, -4);

                toolbarTextField = new GUIStyle(EditorStyles.toolbarTextField);
                toolbarTextField.padding = new RectOffset(2,2,2,2);
                toolbarTextField.margin = new RectOffset(2,2,2,2);

                TabButtonLeft.fontSize = 12;
                TabButtonMid.fontSize = 12;
                TabButtonRight.fontSize = 12;
                LargeLabel.alignment = TextAnchor.UpperRight;

                Header = new GUIStyle("ShurikenModuleTitle");
                HeaderCheckBox = new GUIStyle("ShurikenCheckMark");

                Header.font = (new GUIStyle("Label")).font;
                Header.fontSize = 12;
                Header.fontStyle = FontStyle.Bold;
                Header.border = new RectOffset(15, 7, 4, 4);
                Header.fixedHeight = 28;
                Header.contentOffset = new Vector2(32f, -2f);

                scrollView = new GUIStyle();
                scrollView.padding = new RectOffset(8, 8, 0, 0);

                miniLabel = new GUIStyle(EditorStyles.miniLabel);
                miniLabelRight = new GUIStyle(EditorStyles.miniLabel);
                miniLabelRight.alignment = TextAnchor.MiddleRight;
                miniLabelCenter = new GUIStyle(EditorStyles.miniLabel);
                miniLabelCenter.alignment = TextAnchor.MiddleCenter;

                playbackControlWindow = new GUIStyle(EditorStyles.toolbar);
                playbackControlWindow.border = new RectOffset(4, 4, 4, 4);
                playbackControlWindow.stretchHeight = true;
                playbackControlWindow.fixedHeight = 0;
                playbackControlWindow.contentOffset = new Vector2();

                m_BackgroundTexture = EditorGUIUtility.isProSkin ? GetBGTexture(0.2f) : GetBGTexture(0.4f);

                m_CookBarCooked = new Color(0.25f,0.6f,1.0f,1.0f);
                m_CookBarCookedPro = new Color(0.25f,0.4f,0.65f,1.0f);
                m_CookBarDirty = new Color(1.0f,1.0f,1.0f,0.5f);
                m_CookBarDirtyPro = new Color(0.5f,0.5f,0.5f,0.5f);

                m_MaskRToggle = new GUIStyle(EditorStyles.toolbarButton);
                m_MaskGToggle = new GUIStyle(EditorStyles.toolbarButton);
                m_MaskBToggle= new GUIStyle(EditorStyles.toolbarButton);
                m_MaskAToggle= new GUIStyle(EditorStyles.toolbarButton);

                m_MaskRToggle.onNormal.textColor = new Color(1.0f, 0.0f, 0.0f);
                m_MaskGToggle.onNormal.textColor = new Color(0.0f, 0.6f, 0.2f);
                m_MaskBToggle.onNormal.textColor = new Color(0.0f, 0.2f, 1.0f);
                m_MaskAToggle.onNormal.textColor = new Color(0.5f, 0.5f, 0.5f);


                m_MaskRTogglePro = new GUIStyle(EditorStyles.toolbarButton);
                m_MaskGTogglePro= new GUIStyle(EditorStyles.toolbarButton);
                m_MaskBTogglePro= new GUIStyle(EditorStyles.toolbarButton);
                m_MaskATogglePro= new GUIStyle(EditorStyles.toolbarButton);

                m_MaskRTogglePro.onNormal.textColor = new Color(2.0f, 0.3f, 0.3f);
                m_MaskGTogglePro.onNormal.textColor = new Color(0.5f, 2.0f, 0.1f);
                m_MaskBTogglePro.onNormal.textColor = new Color(0.2f, 0.6f, 2.0f);
                m_MaskATogglePro.onNormal.textColor = new Color(2.0f, 2.0f, 2.0f);

                m_LockToggle = new GUIStyle("IN LockButton");
                m_LockTogglePro = new GUIStyle("IN LockButton");
            }

            public static Texture2D GetBGTexture(float brightness)
            {
                Texture2D out_tex = new Texture2D(2, 2) { hideFlags = HideFlags.DontSave };
                Color[] bgcolors = new Color[4];
                brightness *= 0.95f;
                bgcolors[0] = new Color(brightness+0.05f, brightness+0.05f, brightness+0.05f);
                bgcolors[1] = new Color(brightness, brightness, brightness);
                bgcolors[2] = new Color(brightness, brightness, brightness);
                bgcolors[3] = new Color(brightness+0.05f, brightness+0.05f, brightness+0.05f);
                out_tex.SetPixels(bgcolors);
                out_tex.wrapMode = TextureWrapMode.Repeat;
                out_tex.filterMode = FilterMode.Point;
                out_tex.Apply();
                return out_tex;
            }

            public void SetBGBrightness(float value)
            {
                m_BackgroundTexture = GetBGTexture(value);
            }

            public static bool ToggleableHeader(bool enabled, bool bToggleable, string title)
            {
                Rect rect = GUILayoutUtility.GetRect(16f, 32f, ImageSequencer.styles.Header);
                using (new EditorGUI.DisabledGroupScope(!enabled))
                {
                    GUI.Box(rect, title, ImageSequencer.styles.Header);
                }
                if(bToggleable)
                {
                    Rect toggleRect = new Rect(rect.x + 10f, rect.y + 6f, 13f, 13f);
                    if (Event.current.type == EventType.Repaint)
                        ImageSequencer.styles.HeaderCheckBox.Draw(toggleRect, false, false, enabled, false);

                    Event e = Event.current;
                    if (e.type == EventType.MouseDown)
                    {
                        if (toggleRect.Contains(e.mousePosition))
                        {
                            enabled = !enabled;
                            e.Use();
                        }
                    }
                }

                return enabled;
            }
        }
    }
}
