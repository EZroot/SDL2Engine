using System;
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

        // This field will cache Gravity * dt in a wide format for use in velocity integration.
        private Vector3Wide gravityWideDt;

        /// <summary>
        /// Constructs the callbacks with the specified gravity.
        /// </summary>
        /// <param name="gravity">Gravity to apply (e.g. new Vector3(0, -9.81f, 0)).</param>
        public DefaultPoseIntegratorCallbacks(Vector3 gravity)
        {
            Gravity = gravity;
            gravityWideDt = default;
        }

        /// <summary>
        /// Called once during simulation creation.
        /// </summary>
        /// <param name="simulation">The simulation instance.</param>
        public void Initialize(Simulation simulation)
        {
            // No initialization is required in this basic implementation.
        }

        /// <summary>
        /// Precomputes values that will be used during velocity integration.
        /// </summary>
        /// <param name="dt">The timestep duration.</param>
        public void PrepareForIntegration(float dt)
        {
            // Compute gravity * dt and broadcast it to all SIMD lanes.
            Vector3 gravityDt = Gravity * dt;
            Vector3Wide.Broadcast(gravityDt, out gravityWideDt);
        }

        /// <summary>
        /// Integrates the velocity for a batch of bodies.
        /// </summary>
        /// <param name="bodyIndices">Indices of the bodies being integrated in this bundle.</param>
        /// <param name="position">The current positions of the bodies (wide).</param>
        /// <param name="orientation">The current orientations of the bodies (wide).</param>
        /// <param name="localInertia">Local inertia information (wide).</param>
        /// <param name="integrationMask">Mask indicating which lanes are active.</param>
        /// <param name="workerIndex">Index of the worker thread processing this bundle.</param>
        /// <param name="dt">The timestep duration for each lane (as a vector).</param>
        /// <param name="velocity">The velocities of the bodies (wide) to be updated.</param>
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
            // Add the gravity contribution to the linear velocity for every active lane.
            Vector3Wide.Add(velocity.Linear, gravityWideDt, out velocity.Linear);
            // For this basic integrator, angular velocity is left unchanged.
        }

        /// <summary>
        /// Specifies the angular integration mode to use.
        /// In this case, we return Nonconserving, which is suitable for many simple scenarios.
        /// </summary>
        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

        /// <summary>
        /// Indicates whether unconstrained bodies should be substepped.
        /// </summary>
        public bool AllowSubstepsForUnconstrainedBodies => false;

        /// <summary>
        /// Indicates whether velocity integration is performed for kinematic bodies.
        /// </summary>
        public bool IntegrateVelocityForKinematics => false;
    }
}
