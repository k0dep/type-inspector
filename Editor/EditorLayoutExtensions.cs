using System.Linq;
using UnityEngine;

namespace TypeInspector.Editor
{
    public static class EditorLayoutExtensions
    {
        public static Rect[] DivideHorizontal(this Rect source, float multiplier = .5f)
        {
            var wHalf = source.width * multiplier;
            var left = new Rect(source.x, source.y, wHalf, source.height);
            var right = new Rect(source.x + wHalf, source.y, source.width - wHalf, source.height);
            return new[] {left, right};
        }

        public static Rect[] DivideVertical(this Rect source, float multiplier = .5f)
        {
            var hHalf = source.height * multiplier;
            var upper = new Rect(source.x, source.y, source.width, hHalf);
            var lower = new Rect(source.x, source.y + hHalf, source.width, source.height - hHalf);
            return new[] {upper, lower};
        }

        public static Rect[,] DivideQuad(this Rect source, float hMul = 0.5f, float wMul = 0.5f)
        {
            var horizontal = source.DivideHorizontal(hMul);
            var vertLeft = horizontal[0].DivideVertical(wMul);
            var vertRight = horizontal[1].DivideVertical(wMul);

            return new[,]
            {
                {vertLeft[0], vertRight[0]},
                {vertLeft[1], vertRight[1]}
            };
        }

        public static Rect Padding(this Rect source, float top, float right, float bottom, float left)
        {
            return new Rect(source.x + left, source.y + top, source.width - right - left, source.height - bottom - top);
        }

        public static Rect[] Padding(this Rect[] source, float top, float right, float bottom, float left)
        {
            return source.Select(r => r.Padding(top, right, bottom, left)).ToArray();
        }

        public static Rect[,] Padding(this Rect[,] source, float top, float right, float bottom, float left)
        {
            var result = new Rect[source.GetLength(0), source.GetLength(1)];

            for (int i = 0; i < source.GetLength(0); i++)
            {
                for (int j = 0; j < source.GetLength(1); j++)
                {
                    result[i, j] = source[i, j].Padding(top, right, bottom, left);
                }
            }

            return result;
        }

        public static Rect Move(this Rect source, float x, float y)
        {
            return new Rect(source.position + new Vector2(x, y), source.size);
        }
    }
}