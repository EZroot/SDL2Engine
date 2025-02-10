using System;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

namespace SDL2Engine.Core.Physics.Bepu
{
    /// <summary>
    /// Updated narrow phase callback that configures contact material properties.
    /// </summary>
    public struct DefaultNarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public SpringSettings ContactSpringiness;
        public float MaximumRecoveryVelocity;
        public float FrictionCoefficient;

        /// <summary>
        /// Constructs the narrow phase callbacks with material parameters.
        /// </summary>
        /// <param name="contactSpringiness">Spring settings for contacts.</param>
        /// <param name="maximumRecoveryVelocity">Maximum recovery velocity.</param>
        /// <param name="frictionCoefficient">Friction coefficient.</param>
        public DefaultNarrowPhaseCallbacks(SpringSettings contactSpringiness, float maximumRecoveryVelocity = 2f, float frictionCoefficient = 1f)
        {
            ContactSpringiness = contactSpringiness;
            MaximumRecoveryVelocity = maximumRecoveryVelocity;
            FrictionCoefficient = frictionCoefficient;
        }

        public void Initialize(Simulation simulation)
        {
            if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
            {
                ContactSpringiness = new SpringSettings(30, 1.0f);
                MaximumRecoveryVelocity = 2f;
                FrictionCoefficient = 0.5f;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(
            int workerIndex,
            CollidableReference a,
            CollidableReference b,
            ref float speculativeMargin)
        {
            // Only allow contacts if at least one body is dynamic.
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(
            int workerIndex,
            CollidablePair pair,
            int childIndexA,
            int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold<TManifold>(
            int workerIndex,
            CollidablePair pair,
            ref TManifold manifold,
            out PairMaterialProperties pairMaterial)
            where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = FrictionCoefficient;
            pairMaterial.MaximumRecoveryVelocity = MaximumRecoveryVelocity;
            pairMaterial.SpringSettings = ContactSpringiness;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(
            int workerIndex,
            CollidablePair pair,
            int childIndexA,
            int childIndexB,
            ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {
            // No unmanaged resources to release.
        }
    }
}
