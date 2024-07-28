using UnityEditor;
using UnityEngine;

namespace GitostorySpace
{
    public static class GitostoryGUIExtensions
    {
        public static GUIStyle CreateButtonStyle(Color textColor, int fontSize = 14, FontStyle fontStyle = FontStyle.Bold, RectOffset padding = null)
        {
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                padding = padding ?? new RectOffset(15, 15, 5, 5),
                normal = { textColor = textColor },
                hover = { background = GUI.skin.button.normal.background, textColor = textColor },
                active = { textColor = textColor }
            };

            return buttonStyle;
        }

        public static GUIStyle CreateTextStyle(Color textColor, int fontSize = 12, FontStyle fontStyle = FontStyle.Normal)
        {
            var textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                normal = { textColor = textColor }
            };

            return textStyle;
        }

        public static GUIStyle CreateHeaderStyle(Color textColor, int fontSize = 18, FontStyle fontStyle = FontStyle.Bold)
        {
            return CreateLabelStyle(textColor, fontSize, fontStyle, EditorStyles.largeLabel);
        }

        public static GUIStyle CreateSubHeaderStyle(Color textColor, int fontSize = 15, FontStyle fontStyle = FontStyle.Bold)
        {
            return CreateLabelStyle(textColor, fontSize, fontStyle, EditorStyles.label);
        }

        public static GUIStyle CreateBoldTextStyle(int fontSize = 12, Color? textColor = null)
        {
            return CreateTextStyle(textColor ?? GUI.skin.label.normal.textColor, fontSize, FontStyle.Bold);
        }

        private static GUIStyle CreateLabelStyle(Color textColor, int fontSize, FontStyle fontStyle, GUIStyle baseStyle)
        {
            var style = new GUIStyle(baseStyle)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                normal = { textColor = textColor }
            };

            return style;
        }
        private static Color DefaultTextColor => EditorGUIUtility.isProSkin ? Color.white : Color.black;

        public static GUIStyle CreateDefaultHeaderStyle()
        {
            return CreateHeaderStyle(DefaultTextColor);
        }

    }
}
