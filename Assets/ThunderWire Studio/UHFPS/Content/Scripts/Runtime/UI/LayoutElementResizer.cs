using UnityEngine;
using UnityEngine.UI;

namespace UHFPS.Runtime
{
    public class LayoutElementResizer : LayoutElement
    {
        public bool AutoResizeWidth;
        public bool AutoResizeHeight;

        public bool CustomWidthResize;
        public bool CustomHeightResize;

        public RectTransform WidthTarget;
        public RectTransform HeightTarget;

        public float WidthPadding;
        public float HeightPadding;

        public float MaxWidth = -1f;

        public override float preferredWidth
        {
            get
            {
                if (AutoResizeWidth)
                {
                    float width = 0f;
                    if (!CustomWidthResize)
                    {
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            Transform tr = transform.GetChild(i);
                            if (!tr.gameObject.activeSelf)
                                continue;

                            RectTransform rectTransform = tr as RectTransform;
                            width = LayoutUtility.GetPreferredWidth(rectTransform) + WidthPadding;
                            break;
                        }
                    }
                    else
                    {
                        width = LayoutUtility.GetPreferredWidth(WidthTarget) + WidthPadding;
                    }

                    if (MaxWidth > 0f && width > MaxWidth)
                    {
                        return MaxWidth;
                    }
                    return width;
                }

                return base.preferredWidth;
            }
            set => base.preferredWidth = value;
        }

        public override float preferredHeight
        {
            get
            {
                if (AutoResizeHeight)
                {
                    if (!CustomWidthResize)
                    {
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            Transform tr = transform.GetChild(i);
                            if (!tr.gameObject.activeSelf)
                                continue;

                            RectTransform rectTransform = tr as RectTransform;
                            return LayoutUtility.GetPreferredHeight(rectTransform) + HeightPadding;
                        }
                    }
                    else
                    {
                        return LayoutUtility.GetPreferredHeight(HeightTarget) + HeightPadding;
                    }
                }

                return base.preferredHeight;
            }
            set => base.preferredHeight = value;
        }
    }
}