using Silk.NET.Maths;

namespace proba
{
    internal class CameraDescriptor
    {
        public double DistanceToOrigin { get; private set; } = 1;
        public double AngleToZYPlane { get; private set; } = 0;
        public double AngleToZXPlane { get; private set; } = 0;

        public Vector3D<float> Center { get; private set; } = Vector3D<float>.Zero;

        private const double DistanceScaleFactor = 1.1;
        private const double AngleChangeStepSize = Math.PI / 180 * 5;
        private const float MoveStep = 0.1f;

        public Vector3D<float> Position
        {
            get
            {
                return Center + GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane);
            }
        }

        public Vector3D<float> UpVector
        {
            get
            {
                var forward = Vector3D.Normalize(Target - Position);
                var worldUp = Vector3D<float>.UnitY;
                var right = Vector3D.Normalize(Vector3D.Cross(worldUp, forward));
                var up = Vector3D.Normalize(Vector3D.Cross(forward, right));
                return up;
            }
        }

        public Vector3D<float> Target => Center;

        // --- Szabad mozgás ---
        public void MoveForward()
        {
            var forward = Vector3D.Normalize(Target - Position);
            Center += forward * MoveStep;
        }

        public void MoveBackward()
        {
            var forward = Vector3D.Normalize(Target - Position);
            Center -= forward * MoveStep;
        }

        public void MoveRight()
        {
            var forward = Vector3D.Normalize(Target - Position);
            var right = Vector3D.Normalize(Vector3D.Cross(forward, UpVector));
            Center -= right * MoveStep;
        }

        public void MoveLeft()
        {
            var forward = Vector3D.Normalize(Target - Position);
            var right = Vector3D.Normalize(Vector3D.Cross(forward, UpVector));
            Center += right * MoveStep;
        }

        public void MoveUp()
        {
            Center += UpVector * MoveStep;
        }

        public void MoveDown()
        {
            Center -= UpVector * MoveStep;
        }

        public void IncreaseZXAngle()
        {
            AngleToZXPlane += AngleChangeStepSize;
        }

        public void DecreaseZXAngle()
        {
            AngleToZXPlane -= AngleChangeStepSize;
        }

        public void IncreaseZYAngle()
        {
            AngleToZYPlane += AngleChangeStepSize;
        }

        public void DecreaseZYAngle()
        {
            AngleToZYPlane -= AngleChangeStepSize;
        }

        public void IncreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin * DistanceScaleFactor;
        }

        public void DecreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin / DistanceScaleFactor;
        }

        private static Vector3D<float> GetPointFromAngles(double distanceToOrigin, double angleToMinZYPlane, double angleToMinZXPlane)
        {
            var x = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Sin(angleToMinZYPlane);
            var z = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Cos(angleToMinZYPlane);
            var y = distanceToOrigin * Math.Sin(angleToMinZXPlane);

            return new Vector3D<float>((float)x, (float)y, (float)z);
        }
    }
}
