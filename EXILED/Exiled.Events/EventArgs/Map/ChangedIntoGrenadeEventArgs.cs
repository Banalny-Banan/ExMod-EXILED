// -----------------------------------------------------------------------
// <copyright file="ChangedIntoGrenadeEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Map
{
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Pickups.Projectiles;
    using Interfaces;
    using InventorySystem.Items.ThrowableProjectiles;

    /// <summary>
    /// Contains all information for when the server is turned a pickup into a live grenade.
    /// </summary>
    public class ChangedIntoGrenadeEventArgs : IExiledEvent, IPickupEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangedIntoGrenadeEventArgs"/> class.
        /// </summary>
        /// <param name="pickup">The <see cref="API.Features.Pickups.Pickup"/> being changed.</param>
        /// <param name="projectile">The <see cref="TimeGrenadeProjectile"/>.</param>
        public ChangedIntoGrenadeEventArgs(TimedGrenadePickup pickup, ThrownProjectile projectile)
        {
            GrenadePickup = Pickup.Get<GrenadePickup>(pickup);
            Projectile = Pickup.Get<Projectile>(projectile);
        }

        /// <summary>
        /// Gets a value indicating the pickup that changed into a grenade.
        /// </summary>
        public GrenadePickup GrenadePickup { get; }

        /// <inheritdoc cref="GrenadePickup"/>
        public Pickup Pickup => GrenadePickup;

        /// <summary>
        /// Gets a value indicating the projectile that spawned.
        /// </summary>
        public Projectile Projectile { get; }
    }
}
