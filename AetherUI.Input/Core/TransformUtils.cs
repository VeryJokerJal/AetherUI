using System;
using System.Numerics;

namespace AetherUI.Input.Core
{
    /// <summary>
    /// 变换工具类
    /// </summary>
    public static class TransformUtils
    {
        /// <summary>
        /// 创建平移变换
        /// </summary>
        /// <param name="x">X偏移</param>
        /// <param name="y">Y偏移</param>
        /// <returns>平移变换矩阵</returns>
        public static Matrix4x4 CreateTranslation(double x, double y)
        {
            return Matrix4x4.CreateTranslation((float)x, (float)y, 0);
        }

        /// <summary>
        /// 创建平移变换
        /// </summary>
        /// <param name="offset">偏移向量</param>
        /// <returns>平移变换矩阵</returns>
        public static Matrix4x4 CreateTranslation(Point offset)
        {
            return CreateTranslation(offset.X, offset.Y);
        }

        /// <summary>
        /// 创建缩放变换
        /// </summary>
        /// <param name="scaleX">X缩放</param>
        /// <param name="scaleY">Y缩放</param>
        /// <returns>缩放变换矩阵</returns>
        public static Matrix4x4 CreateScale(double scaleX, double scaleY)
        {
            return Matrix4x4.CreateScale((float)scaleX, (float)scaleY, 1);
        }

        /// <summary>
        /// 创建缩放变换
        /// </summary>
        /// <param name="scale">缩放因子</param>
        /// <returns>缩放变换矩阵</returns>
        public static Matrix4x4 CreateScale(double scale)
        {
            return CreateScale(scale, scale);
        }

        /// <summary>
        /// 创建旋转变换
        /// </summary>
        /// <param name="angle">旋转角度（弧度）</param>
        /// <returns>旋转变换矩阵</returns>
        public static Matrix4x4 CreateRotation(double angle)
        {
            return Matrix4x4.CreateRotationZ((float)angle);
        }

        /// <summary>
        /// 创建旋转变换
        /// </summary>
        /// <param name="angle">旋转角度（弧度）</param>
        /// <param name="center">旋转中心</param>
        /// <returns>旋转变换矩阵</returns>
        public static Matrix4x4 CreateRotation(double angle, Point center)
        {
            var translation1 = CreateTranslation(-center.X, -center.Y);
            var rotation = CreateRotation(angle);
            var translation2 = CreateTranslation(center.X, center.Y);

            return Matrix4x4.Multiply(Matrix4x4.Multiply(translation1, rotation), translation2);
        }

        /// <summary>
        /// 创建倾斜变换
        /// </summary>
        /// <param name="skewX">X倾斜角度（弧度）</param>
        /// <param name="skewY">Y倾斜角度（弧度）</param>
        /// <returns>倾斜变换矩阵</returns>
        public static Matrix4x4 CreateSkew(double skewX, double skewY)
        {
            return new Matrix4x4(
                1, (float)Math.Tan(skewY), 0, 0,
                (float)Math.Tan(skewX), 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
        }

        /// <summary>
        /// 变换点
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="transform">变换矩阵</param>
        /// <returns>变换后的点</returns>
        public static Point TransformPoint(Point point, Matrix4x4 transform)
        {
            var vector = new Vector4((float)point.X, (float)point.Y, 0, 1);
            var transformed = Vector4.Transform(vector, transform);
            return new Point(transformed.X, transformed.Y);
        }

        /// <summary>
        /// 变换向量
        /// </summary>
        /// <param name="vector">向量</param>
        /// <param name="transform">变换矩阵</param>
        /// <returns>变换后的向量</returns>
        public static Point TransformVector(Point vector, Matrix4x4 transform)
        {
            var v = new Vector4((float)vector.X, (float)vector.Y, 0, 0);
            var transformed = Vector4.Transform(v, transform);
            return new Point(transformed.X, transformed.Y);
        }

        /// <summary>
        /// 变换矩形
        /// </summary>
        /// <param name="rect">矩形</param>
        /// <param name="transform">变换矩阵</param>
        /// <returns>变换后的矩形</returns>
        public static Rect TransformRect(Rect rect, Matrix4x4 transform)
        {
            var corners = new[]
            {
                new Point(rect.Left, rect.Top),
                new Point(rect.Right, rect.Top),
                new Point(rect.Left, rect.Bottom),
                new Point(rect.Right, rect.Bottom)
            };

            var transformedCorners = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                transformedCorners[i] = TransformPoint(corners[i], transform);
            }

            var minX = Math.Min(Math.Min(transformedCorners[0].X, transformedCorners[1].X),
                               Math.Min(transformedCorners[2].X, transformedCorners[3].X));
            var maxX = Math.Max(Math.Max(transformedCorners[0].X, transformedCorners[1].X),
                               Math.Max(transformedCorners[2].X, transformedCorners[3].X));
            var minY = Math.Min(Math.Min(transformedCorners[0].Y, transformedCorners[1].Y),
                               Math.Min(transformedCorners[2].Y, transformedCorners[3].Y));
            var maxY = Math.Max(Math.Max(transformedCorners[0].Y, transformedCorners[1].Y),
                               Math.Max(transformedCorners[2].Y, transformedCorners[3].Y));

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// 获取变换的逆矩阵
        /// </summary>
        /// <param name="transform">变换矩阵</param>
        /// <returns>逆变换矩阵</returns>
        public static Matrix4x4? GetInverse(Matrix4x4 transform)
        {
            if (Matrix4x4.Invert(transform, out Matrix4x4 inverse))
            {
                return inverse;
            }
            return null;
        }

        /// <summary>
        /// 检查变换是否为单位矩阵
        /// </summary>
        /// <param name="transform">变换矩阵</param>
        /// <param name="tolerance">容差</param>
        /// <returns>是否为单位矩阵</returns>
        public static bool IsIdentity(Matrix4x4 transform, float tolerance = 1e-6f)
        {
            return Math.Abs(transform.M11 - 1) < tolerance &&
                   Math.Abs(transform.M12) < tolerance &&
                   Math.Abs(transform.M13) < tolerance &&
                   Math.Abs(transform.M14) < tolerance &&
                   Math.Abs(transform.M21) < tolerance &&
                   Math.Abs(transform.M22 - 1) < tolerance &&
                   Math.Abs(transform.M23) < tolerance &&
                   Math.Abs(transform.M24) < tolerance &&
                   Math.Abs(transform.M31) < tolerance &&
                   Math.Abs(transform.M32) < tolerance &&
                   Math.Abs(transform.M33 - 1) < tolerance &&
                   Math.Abs(transform.M34) < tolerance &&
                   Math.Abs(transform.M41) < tolerance &&
                   Math.Abs(transform.M42) < tolerance &&
                   Math.Abs(transform.M43) < tolerance &&
                   Math.Abs(transform.M44 - 1) < tolerance;
        }

        /// <summary>
        /// 分解变换矩阵
        /// </summary>
        /// <param name="transform">变换矩阵</param>
        /// <returns>分解结果</returns>
        public static TransformDecomposition Decompose(Matrix4x4 transform)
        {
            // 提取平移
            var translation = new Point(transform.M41, transform.M42);

            // 提取缩放和旋转
            var scaleX = Math.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12);
            var scaleY = Math.Sqrt(transform.M21 * transform.M21 + transform.M22 * transform.M22);

            // 检查是否有负缩放
            var determinant = transform.M11 * transform.M22 - transform.M12 * transform.M21;
            if (determinant < 0)
            {
                scaleX = -scaleX;
            }

            // 提取旋转
            var rotation = Math.Atan2(transform.M12 / scaleX, transform.M11 / scaleX);

            return new TransformDecomposition(translation, new Point(scaleX, scaleY), rotation);
        }

        /// <summary>
        /// 组合变换矩阵
        /// </summary>
        /// <param name="translation">平移</param>
        /// <param name="scale">缩放</param>
        /// <param name="rotation">旋转（弧度）</param>
        /// <returns>组合的变换矩阵</returns>
        public static Matrix4x4 Compose(Point translation, Point scale, double rotation)
        {
            var t = CreateTranslation(translation);
            var s = CreateScale(scale.X, scale.Y);
            var r = CreateRotation(rotation);

            return Matrix4x4.Multiply(Matrix4x4.Multiply(s, r), t);
        }

        /// <summary>
        /// 插值变换矩阵
        /// </summary>
        /// <param name="from">起始变换</param>
        /// <param name="to">结束变换</param>
        /// <param name="t">插值参数（0-1）</param>
        /// <returns>插值后的变换矩阵</returns>
        public static Matrix4x4 Lerp(Matrix4x4 from, Matrix4x4 to, float t)
        {
            var fromDecomp = Decompose(from);
            var toDecomp = Decompose(to);

            var translation = new Point(
                fromDecomp.Translation.X + (toDecomp.Translation.X - fromDecomp.Translation.X) * t,
                fromDecomp.Translation.Y + (toDecomp.Translation.Y - fromDecomp.Translation.Y) * t);

            var scale = new Point(
                fromDecomp.Scale.X + (toDecomp.Scale.X - fromDecomp.Scale.X) * t,
                fromDecomp.Scale.Y + (toDecomp.Scale.Y - fromDecomp.Scale.Y) * t);

            var rotation = fromDecomp.Rotation + (toDecomp.Rotation - fromDecomp.Rotation) * t;

            return Compose(translation, scale, rotation);
        }
    }

    /// <summary>
    /// 变换分解结果
    /// </summary>
    public readonly struct TransformDecomposition
    {
        /// <summary>
        /// 平移
        /// </summary>
        public Point Translation { get; }

        /// <summary>
        /// 缩放
        /// </summary>
        public Point Scale { get; }

        /// <summary>
        /// 旋转（弧度）
        /// </summary>
        public double Rotation { get; }

        /// <summary>
        /// 初始化变换分解结果
        /// </summary>
        /// <param name="translation">平移</param>
        /// <param name="scale">缩放</param>
        /// <param name="rotation">旋转</param>
        public TransformDecomposition(Point translation, Point scale, double rotation)
        {
            Translation = translation;
            Scale = scale;
            Rotation = rotation;
        }

        public override string ToString() =>
            $"Translation: {Translation}, Scale: {Scale}, Rotation: {Rotation:F2}rad";
    }
}
