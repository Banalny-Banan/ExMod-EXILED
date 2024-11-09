// -----------------------------------------------------------------------
// <copyright file="Shooting.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using InventorySystem.Items.Firearms;
using Mirror;

namespace Exiled.Events.Patches.Events.Player
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using API.Features;
    using API.Features.Pools;
    using Attributes;
    using Exiled.Events.EventArgs.Player;
    using HarmonyLib;
    using InventorySystem.Items.Firearms.BasicMessages;
    using InventorySystem.Items.Firearms.Modules;
    using static HarmonyLib.AccessTools;

    /// <summary>
    ///     Patches <see cref="FirearmBasicMessagesHandler.ServerShotReceived" />.
    ///     Adds the <see cref="Handlers.Player.Shooting" /> events.
    /// </summary>
    [EventPatch(typeof(Handlers.Player), nameof(Handlers.Player.Shooting)), HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    internal static class Shooting
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label isAllowedLabel = generator.DefineLabel();
            Label returnLabel = generator.DefineLabel();

            LocalBuilder ev = generator.DeclareLocal(typeof(ShootingEventArgs));

            LocalBuilder refusedShotMessage = generator.DeclareLocal(typeof(AutomaticAction.RefusedShotMessage));

            int offset = -2;
            int index = newInstructions.FindIndex(instruction => instruction.Calls(Method(typeof(IActionModule), nameof(IActionModule.ServerAuthorizeShot)))) + offset;

            // Obtain the generic Send<T> method
            MethodInfo sendMethod = typeof(NetworkConnection)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(m => m.Name == nameof(NetworkConnection.Send) && m.IsGenericMethod
                    && m.GetParameters() is { Length: 2 } parameters && parameters[1].ParameterType == typeof(int))?
                .MakeGenericMethod(typeof(AutomaticAction.RefusedShotMessage));

            // Ensure the method was found
            if (sendMethod == null)
            {
                throw new Exception("Failed to find NetworkConnection.Send<T> method.");
            }

            newInstructions.InsertRange(
                index,
                new[]
                {
                    // Player.Get(referenceHub)
                    new CodeInstruction(OpCodes.Ldloc_0).MoveLabelsFrom(newInstructions[index]),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),

                    // firearm
                    new(OpCodes.Ldloc_1),

                    // msg
                    new(OpCodes.Ldarg_1),

                    // ShootingEventArgs ev = new(Player, firearm, ShotMessage)
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(ShootingEventArgs))[0]),

                    new(OpCodes.Dup),
                    new(OpCodes.Stloc, ev.LocalIndex),

                    // Handlers.Player.OnShooting(ev)
                    new(OpCodes.Dup),
                    new(OpCodes.Call, Method(typeof(Handlers.Player), nameof(Handlers.Player.OnShooting))),

                    // if (ev.IsAllowed)
                    //    goto isAllowedLabel;
                    new(OpCodes.Callvirt, PropertyGetter(typeof(ShootingEventArgs), nameof(ShootingEventArgs.IsAllowed))),
                    new(OpCodes.Brtrue_S, isAllowedLabel),

                    // RefusedShotMessage causes a crash with not automatic weapons
                    // if (firearm.ActionModule is not AutomaticAction)
                    //     return;
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Firearm), nameof(Firearm.ActionModule))),
                    new CodeInstruction(OpCodes.Isinst, typeof(AutomaticAction)),
                    new CodeInstruction(OpCodes.Brfalse, returnLabel),

                    // var refusedShotMessage = new AutomaticAction.RefusedShotMessage();
                    new CodeInstruction(OpCodes.Ldloca_S, refusedShotMessage.LocalIndex),
                    new CodeInstruction(OpCodes.Initobj, typeof(AutomaticAction.RefusedShotMessage)),

                    // ev.Player.Connection.Send(refusedShotMessage, 0)
                    new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex), // ev
                    new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(ShootingEventArgs), nameof(ShootingEventArgs.Player))), // ev.Player
                    new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.Connection))), // ev.Player.Connection

                    new CodeInstruction(OpCodes.Ldloc, refusedShotMessage.LocalIndex), // refusedShotMessage
                    new CodeInstruction(OpCodes.Ldc_I4_0), // 0

                    new CodeInstruction(OpCodes.Callvirt, sendMethod), // ev.Player.Connection.Send(refusedShotMessage, 0)

                    // return;
                    new CodeInstruction(OpCodes.Br, returnLabel),

                    // isAllowedLabel:
                    // msg = ev.ShotMessage
                    new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex).WithLabels(isAllowedLabel),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(ShootingEventArgs), nameof(ShootingEventArgs.ShotMessage))),
                    new(OpCodes.Starg_S, 1),
                });

            newInstructions[newInstructions.Count - 1].WithLabels(returnLabel);

            for (var z = 0; z < newInstructions.Count; z++)
            {
                yield return newInstructions[z];
            }

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}