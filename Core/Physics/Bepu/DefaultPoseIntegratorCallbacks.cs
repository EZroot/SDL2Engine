using System;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace SDL2Engine.Core.Physics.Bepu
{
    /// <summary>
    /// Updated pose integrator callback that applies gravity and damping.
    /// </summary>
    public struct DefaultPoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        /// <summary>
        /// Gravity to apply to dynamic bodies.
        /// </summary>
        public Vector3 Gravity { get; set; }
        /// <summary>
        /// Fraction of dynamic body linear velocity to remove per unit time (0 is undamped).
        /// </summary>
        public float LinearDamping;
        /// <summary>
        /// Fraction of dynamic body angular velocity to remove per unit time (0 is undamped).
        /// </summary>
        public float AngularDamping;

        // Cached values for use in SIMD velocity integration.
        private Vector3Wide gravityWideDt;
        private Vector<float> linearDampingDt;
        private Vector<float> angularDampingDt;

        /// <summary>
        /// Constructs the callbacks with the specified gravity and damping values.
        /// </summary>
        /// <param name="gravity">Gravity vector.</param>
        /// <param name="linearDamping">Linear damping coefficient (e.g. 0.03f).</param>
        /// <param name="angularDamping">Angular damping coefficient (e.g. 0.03f).</param>
        public DefaultPoseIntegratorCallbacks(Vector3 gravity, float linearDamping = 0.03f, float angularDamping = 0.03f)
        {
            Gravity = gravity;
            LinearDamping = linearDamping;
            AngularDamping = angularDamping;
            gravityWideDt = default;
            linearDampingDt = default;
            angularDampingDt = default;
        }

        public void Initialize(Simulation simulation)
        {
            // No additional initialization required.
        }

        public void PrepareForIntegration(float dt)
        {
            // Compute damping multipliers.
            // Clamp (1 - damping) between 0 and 1.
            float clampedLinear = MathHelper.Clamp(1 - LinearDamping, 0, 1);
            float clampedAngular = MathHelper.Clamp(1 - AngularDamping, 0, 1);
            // Exponentially decay the velocities based on dt.
            linearDampingDt = new Vector<float>(MathF.Pow(clampedLinear, dt));
            angularDampingDt = new Vector<float>(MathF.Pow(clampedAngular, dt));
            
            // Precompute gravity * dt.
            Vector3 gravityDt = Gravity * dt;
            Vector3Wide.Broadcast(gravityDt, out gravityWideDt);
        }

        public void IntegrateVelocity(
            Vector<int> bodyIndices,
            Vector3Wide position,
            QuaternionWide orientation,
            BodyInertiaWide localInertia,
            Vector<int> integrationMask,
            int workerIndex,
            Vector<float> dt,
            ref BodyVelocityWide velocity)
        {
            // Add gravity and apply damping.
            velocity.Linear = (velocity.Linear + gravityWideDt) * linearDampingDt;
            velocity.Angular = velocity.Angular * angularDampingDt;
        }

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

        public bool AllowSubstepsForUnconstrainedBodies => false;

        public bool IntegrateVelocityForKinematics => false;
    }
}
