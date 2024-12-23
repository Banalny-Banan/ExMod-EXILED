// -----------------------------------------------------------------------
// <copyright file="ChangingIntoGrenadeEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Map
{
    using Exiled.API.Features;
    using Exiled.API.Features.Pickups;
    using Exiled.Events.EventArgs.Interfaces;
    using Footprinting;
    using InventorySystem.Items.ThrowableProjectiles;

    /// <summary>
    /// Contains all information for when the server is turning a pickup into a live grenade.
    /// </summary>
    public class ChangingIntoGrenadeEventArgs : IDeniableEvent, IPickupEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangingIntoGrenadeEventArgs" /> class.
        /// </summary>
        /// <param name="pickup">The <see cref="API.Features.Pickups.Pickup"/> being changed.</param>
        public ChangingIntoGrenadeEventArgs(TimedGrenadePickup pickup)
        {
            if (pickup is null)
                Log.Error($"{nameof(ChangingIntoGrenadeEventArgs)}: Pickup is null!");

            AttackerFootprint = pickup?._attacker;
            Pickup = Pickup.Get(pickup);
            Type = Pickup.Type;
        }

        /// <summary>
        /// Gets a value indicating the pickup being changed into a grenade.
        /// </summary>
        public Pickup Pickup { get; }

        /// <summary>
        /// Gets the footprint of the person responsible for activating the grenade. Can be null.
        /// </summary>
        public Footprint? AttackerFootprint { get; }

        /// <summary>
        /// Gets or sets a value indicating what type of grenade will be spawned.
        /// </summary>
        public ItemType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pickup will be changed.
        /// </summary>
        public bool IsAllowed { get; set; } = true;
    }
}