using Silk.NET.Maths;

namespace Szeminarium
{
    internal class CameraDescriptor
    {
        public enum CameraMode
        {
            BehindObject,
            FrontOfObject
        }

        public CameraMode Mode { get; private set; } = CameraMode.BehindObject;

        public double DistanceToOrigin { get; private set; } = 15;
        public double AngleToZYPlane { get; private set; } = 0;
        public double AngleToZXPlane { get; private set; } = Math.PI / 15;

        private const double DistanceScaleFactor = 1.1;
        private const double AngleChangeStepSize = Math.PI / 10;

        private Vector3D<float> targetPosition = Vector3D<float>.Zero;
        private Vector3D<float>? manualPosition = null;
        private Vector3D<float>? manualTarget = null;
        private double relativeAngleToTarget = 0;

        public bool IsFollowingTarget { get; private set; } = true;

        public Vector3D<float> Position => manualPosition ?? CalculateDefaultPosition();
        public Vector3D<float> Target => manualTarget ?? targetPosition;
        public Vector3D<float> UpVector => new Vector3D<float>(0f, 1f, 0f);

        public void ToggleCameraMode()
        {
            if (Mode == CameraMode.BehindObject)
            {
                Mode = CameraMode.FrontOfObject;
            }
            else
            {
                manualPosition = null;
                manualTarget = null;
                Mode = CameraMode.BehindObject;
            }
        }

        
        public void UpdateCamera(Vector3D<float> target, float rotation, float scale = 1f)
        {
            manualPosition = null;
            manualTarget = null;

            if (Mode == CameraMode.BehindObject)
            {
                UpdateFollowingBehind(target, rotation, distance: 3f * scale, height: 1.8f * scale);
            }
            else
            {
                var forward = new Vector3D<float>((float)Math.Sin(rotation), 0f, (float)Math.Cos(rotation));
                float eyeLevel = 1.5f * scale;

                var eyeOffset = new Vector3D<float>(0f, eyeLevel, 0f);

                float distanceFront = 4.5f * scale;
                float lookAtDistance = 5.0f * scale;

                var camPos = target + forward * distanceFront + eyeOffset;
                var lookAt = target + forward * lookAtDistance + eyeOffset;


                OverrideCamera(camPos, lookAt);
                Mode = CameraMode.FrontOfObject;
            }
        }

        public void OverrideCamera(Vector3D<float> position, Vector3D<float> target)
        {
            manualPosition = position;
            manualTarget = target;
        }

        public void UpdateFollowingBehind(Vector3D<float> target, float rotation, float distance = 10f, float height = 5f)
        {
            targetPosition = target;
            var offsetAngle = rotation + MathF.PI;

            AngleToZYPlane = offsetAngle;
            AngleToZXPlane = Math.Atan2(height, distance);

            DistanceToOrigin = Math.Sqrt(distance * distance + height * height);

            IsFollowingTarget = true;
            Mode = CameraMode.BehindObject;
        }


        public void IncreaseZXAngle() => AngleToZXPlane += AngleChangeStepSize;
        public void DecreaseZXAngle() => AngleToZXPlane -= AngleChangeStepSize;
        public void IncreaseZYAngle(float amount) => AngleToZYPlane -= amount;
        public void DecreaseZYAngle(float amount) => AngleToZYPlane += amount;
        public void IncreaseDistance() => DistanceToOrigin *= DistanceScaleFactor;
        public void DecreaseDistance() => DistanceToOrigin /= DistanceScaleFactor;

        private Vector3D<float> CalculateDefaultPosition()
        {
            if (IsFollowingTarget)
            {
                var relativePos = GetPointFromAngles(DistanceToOrigin, AngleToZYPlane + relativeAngleToTarget, AngleToZXPlane);
                return targetPosition + relativePos;
            }
            else
            {
                return GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane);
            }
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

