using System;
using System.Diagnostics.CodeAnalysis;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;

namespace SDL2Engine.Core.Physics.Bepu
{
    public struct DefaultNarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public void Initialize(Simulation simulation)
        {
            // No initialization is required for the default callbacks.
        }

        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        {
            // Allow contact generation for all colliding pairs.
            return true;
        }

        public bool ConfigureContactManifold<TManifold>(
            int workerIndex,
            CollidablePair pair,
            ref TManifold manifold,
            [UnscopedRef] out PairMaterialProperties pairMaterial)
            where TManifold : unmanaged, IContactManifold<TManifold>
        {
            // Provide default pair material properties.
            pairMaterial = new PairMaterialProperties();
            // Allow contact generation.
            return true;
        }

        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            // Allow contacts for the specified child indices.
            return true;
        }

        public bool ConfigureContactManifold(
            int workerIndex,
            CollidablePair pair,
            int childIndexA,
            int childIndexB,
            ref ConvexContactManifold manifold)
        {
            // Default behavior: allow contact generation.
            return true;
        }

        public void Dispose()
        {
            // No unmanaged resources to dispose.
        }
    }
}