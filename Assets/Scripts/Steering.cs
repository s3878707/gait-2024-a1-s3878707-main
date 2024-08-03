using System.Collections.Generic;
using UnityEngine;

namespace SteeringCalcs
{
    [System.Serializable]
    public class AvoidanceParams
    {
        public bool Enable;
        public LayerMask ObstacleMask;
        public float CircleCastRadius;
        // Note: As mentioned in the spec, you're free to add extra parameters to AvoidanceParams.
    }

    public class Steering
    {
        // PLEASE NOTE:
        // You do not need to edit any of the methods in the HelperMethods region.
        // In Visual Studio, you can collapse the HelperMethods region by clicking
        // the "-" to the left.
        #region HelperMethods

        // Helper method for rotating a vector by an angle (in degrees).
        public static Vector2 rotate(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;

            return new Vector2(
                v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
                v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
            );
        }

        // Converts a desired velocity into a steering force, as will
        // be explained in class (Week 2).
        public static Vector2 DesiredVelToForce(Vector2 desiredVel, Rigidbody2D rb, float accelTime, float maxAccel)
        {
            Vector2 accel = (desiredVel - rb.velocity) / accelTime;

            if (accel.magnitude > maxAccel)
            {
                accel = accel.normalized * maxAccel;
            }

            // F = ma
            return rb.mass * accel;
        }

        // In addition to separation, cohesion and alignment, the flies also have
        // an "anchor" force applied to them while flocking, to keep them within
        // the game arena. This is already implemented for you.
        public static Vector2 GetAnchor(Vector2 currentPos, Vector2 anchorDims)
        {
            Vector2 desiredVel = Vector2.zero;

            if (Mathf.Abs(currentPos.x) > anchorDims.x)
            {
                desiredVel -= new Vector2(currentPos.x, 0.0f);
            }

            if (Mathf.Abs(currentPos.y) > anchorDims.y)
            {
                desiredVel -= new Vector2(0.0f, currentPos.y);
            }

            return desiredVel;
        }

        // This "parent" seek method toggles between SeekAndAvoid and BasicSeek
        // depending on whether obstacle avoidance is enabled. Do not edit this.
        public static Vector2 Seek(Vector2 currentPos, Vector2 targetPos, float maxSpeed, AvoidanceParams avoidParams)
        {
            if (avoidParams.Enable)
            {
                return SeekAndAvoid(currentPos, targetPos, maxSpeed, avoidParams);
            }
            else
            {
                return BasicSeek(currentPos, targetPos, maxSpeed);
            }
        }

        // Seek is already implemented for you. Do not edit this method.
        public static Vector2 BasicSeek(Vector2 currentPos, Vector2 targetPos, float maxSpeed)
        {
            Vector2 offset = targetPos - currentPos;
            Vector2 desiredVel = offset.normalized * maxSpeed;
            return desiredVel;
        }

        // Do not edit this method. To implement obstacle avoidance, the only method
        // you need to edit is GetAvoidanceTarget.
        public static Vector2 SeekAndAvoid(Vector2 currentPos, Vector2 targetPos, float maxSpeed, AvoidanceParams avoidParams)
        {
            targetPos = GetAvoidanceTarget(currentPos, targetPos, avoidParams);

            return BasicSeek(currentPos, targetPos, maxSpeed);
        }

        // This "parent" arrive method toggles between ArriveAndAvoid and BasicArrive
        // depending on whether obstacle avoidance is enabled. Do not edit this.
        public static Vector2 Arrive(Vector2 currentPos, Vector2 targetPos, float radius, float maxSpeed, AvoidanceParams avoidParams)
        {
            if (avoidParams.Enable)
            {
                return ArriveAndAvoid(currentPos, targetPos, radius, maxSpeed, avoidParams);
            }
            else
            {
                return BasicArrive(currentPos, targetPos, radius, maxSpeed);
            }   
        }

        // Do not edit this method. To implement obstacle avoidance, the only method
        // you need to edit is GetAvoidanceTarget.
        public static Vector2 ArriveAndAvoid(Vector2 currentPos, Vector2 targetPos, float radius, float maxSpeed, AvoidanceParams avoidParams)
        {
            targetPos = GetAvoidanceTarget(currentPos, targetPos, avoidParams);

            return BasicArrive(currentPos, targetPos, radius, maxSpeed);
        }

        #endregion

        // Below are all the methods that you *do* need to edit.
        #region MethodsToImplement

        // See the spec for a detailed explanation of how GetAvoidanceTarget is expected to work.
        // You're expected to use Physics2D.CircleCast (https://docs.unity3d.com/ScriptReference/Physics2D.CircleCast.html)
        // You'll also probably want to use the rotate() method declared above.
        public static Vector2 GetAvoidanceTarget(Vector2 currentPos, Vector2 targetPos, AvoidanceParams avoidParams)
        {
            Vector2 newTarget = targetPos;
            Color color = Color.green;
            RaycastHit2D hit;
            Vector2 direction = targetPos - currentPos;
            if (avoidParams.Enable)
            {   
                hit = Physics2D.CircleCast(currentPos, avoidParams.CircleCastRadius, direction, direction.magnitude, avoidParams.ObstacleMask);
                if (hit)
                {
                    float angleIncrement = 5f; 
                    float maxAngle = 180f; 
                    float currentAngle = 0f; 
                    bool pathFound = false;

                    while (currentAngle < maxAngle && !pathFound)
                    {
                        color = Color.red;
                        Vector2 rotatedDirection = rotate(direction, angleIncrement);
                        hit = Physics2D.CircleCast(currentPos, avoidParams.CircleCastRadius, rotatedDirection, rotatedDirection.magnitude, avoidParams.ObstacleMask);
                
                        if (!hit)
                        {
                            newTarget = currentPos + rotatedDirection;
                            color = Color.green;
                            Debug.DrawLine(currentPos, currentPos + rotatedDirection, color);
                            pathFound = true;
                        }
                        else
                        {
                            currentAngle += angleIncrement;
                            direction = rotate(direction, angleIncrement);
                            Debug.DrawLine(currentPos, currentPos + direction, color);
                        }
                    }
                }
            }
            return newTarget;
        }

        public static Vector2 BasicFlee(Vector2 currentPos, Vector2 predatorPos, float maxSpeed)
        {
            // TODO: Implement proper flee logic.
            // The method should return the character's *desired velocity*, not a steering force.
            Vector2 offset = predatorPos - currentPos;
            Vector2 desiredVel = - offset.normalized * maxSpeed;
            return desiredVel;
        }

        public static Vector2 BasicArrive(Vector2 currentPos, Vector2 targetPos, float radius, float maxSpeed)
        {   
            Vector2 distance = targetPos - currentPos;
            if (distance.magnitude >= radius){
                return BasicSeek(currentPos, targetPos, maxSpeed);
            }
                return distance / radius * maxSpeed;
            }
            // TODO: Replace the BasicSeek() call with proper arrive logic.
            // The method should return the character's *desired velocity*, not a steering force.


        public static Vector2 GetSeparation(Vector2 currentPos, List<Transform> neighbours, float maxSpeed)
        {
            // TODO: Replace with proper separation calculation.
            // The method should return the character's *desired velocity*, not a steering force.
            // Note that there are various online guides/tutorials that calculate this in
            // different ways, but you are expected to follow the approach shown in class (Week 2).
            Vector2 rawVel = Vector2.zero;
            foreach (Transform neighbor in neighbours){
                Vector2 offset = currentPos - (Vector2)neighbor.position;
                rawVel += offset/ (offset.magnitude * offset.magnitude);
            }
            Vector2 desiredVel = rawVel.normalized * maxSpeed;
            return desiredVel;
        }

        public static Vector2 GetCohesion(Vector2 currentPos, List<Transform> neighbours, float maxSpeed)
        {
            // TODO: Replace with proper cohesion calculation.
            // The method should return the character's *desired velocity*, not a steering force.
            // Note that there are various online guides/tutorials that calculate this in
            // different ways, but you are expected to follow the approach shown in class (Week 2).
            Vector2 nAve = Vector2.zero;
            foreach (Transform neighbor in neighbours){
                nAve += (Vector2)neighbor.position;
            }
            if ( neighbours.Count > 0){
                nAve /= neighbours.Count;
                Vector2 rawVel = nAve - currentPos;
                Vector2 desiredVel = rawVel.normalized * maxSpeed;
                return desiredVel;
            }
            else {
                return Vector2.zero;
            }
        }

        public static Vector2 GetAlignment(List<Transform> neighbours, float maxSpeed)
        {
            // TODO: Replace with proper alignment calculation.
            // The method should return the character's *desired velocity*, not a steering force.
            // Note that there are various online guides/tutorials that calculate this in
            // different ways, but you are expected to follow the approach shown in class (Week 2).
            Vector2 vAve = Vector2.zero;
            foreach (Transform neighbor in neighbours){
                Rigidbody2D neighborRigidbody = neighbor.GetComponent<Rigidbody2D>();
                if(neighborRigidbody != null){
                    vAve += neighborRigidbody.velocity;
                }
            }
            if (neighbours.Count > 0)
            {
                vAve /= neighbours.Count;
                Vector2 desiredVel = vAve.normalized * maxSpeed;
                return desiredVel;
            }
            else {
                return Vector2.zero;
            }
        }

        #endregion
    }
}
