using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace SDL2Engine.Core.Physics.Bepu
{
    /// <summary>
    /// A basic pose integrator callback that applies gravity using simple Euler integration.
    /// </summary>
    public struct DefaultPoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        /// <summary>
        /// Gets or sets the gravity vector to apply.
        /// </summary>
        public Vector3 Gravity { get; set; }

        // Cached gravity * dt in wide format.
        private Vector3Wide gravityWideDt;

        /// <summary>
        /// Constructs the callbacks with the specified gravity.
        /// </summary>
        public DefaultPoseIntegratorCallbacks(Vector3 gravity)
        {
            Gravity = gravity;
            gravityWideDt = default;
        }

        public void Initialize(Simulation simulation)
        {
            // No initialization needed.
        }

        public void PrepareForIntegration(float dt)
        {
            // Precompute gravity * dt for use in velocity integration.
            Vector3 gravityDt = Gravity * dt;
            Vector3Wide.Broadcast(gravityDt, out gravityWideDt);
        }

        public void IntegrateVelocity(
            System.Numerics.Vector<int> bodyIndices,
            Vector3Wide position,
            QuaternionWide orientation,
            BodyInertiaWide localInertia,
            System.Numerics.Vector<int> integrationMask,
            int workerIndex,
            System.Numerics.Vector<float> dt,
            ref BodyVelocityWide velocity)
        {
            // Add gravity to each body's linear velocity.
            Vector3Wide.Add(velocity.Linear, gravityWideDt, out velocity.Linear);
            // Angular velocity remains unchanged.
        }

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

        public bool AllowSubstepsForUnconstrainedBodies => false;

        public bool IntegrateVelocityForKinematics => false;
    }
}
