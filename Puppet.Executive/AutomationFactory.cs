﻿using Puppet.Automation;
using Puppet.Common.Automation;
using Puppet.Common.Devices;
using Puppet.Common.Events;
using Puppet.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Puppet.Executive
{
    public class AutomationFactory
    {
        private const string _automationAssembly = "Puppet.Automation.dll";

        /// <summary>
        /// Figures out the appropriate implementation of IAutomation based on the data in the event and returns it.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="hub"></param>
        /// <returns>An IEnumberable<IAutomation> containing the automations to be run for this event.</returns>
        public static IEnumerable<IAutomation> GetAutomations(HubEvent evt, HomeAutomationPlatform hub)
        {
            /*
             *  Get the types from the assembly
             *      where the type implements IAutomation and
             *          the type has trigger attributes
             *              where the trigger attribute names a mapped device that matches the device that caused the event
             *                  and the attribute also names a Capability that matches the device that caused the event
             *          and the count of the matching trigger attributes is greater than 0
             */
            IEnumerable<Type> typeCollection = Assembly.LoadFrom(_automationAssembly).GetTypes() 
                .Where(t => typeof(IAutomation).IsAssignableFrom(t) && 
                    (t.GetCustomAttributes<TriggerDeviceAttribute>() 
                        .Where(a => hub.LookupDeviceId(a.DeviceMappedName) == evt.deviceId &&
                            a.Capability.ToString().ToLower() == evt.name))
                    .Count() > 0);
            foreach (Type automation in typeCollection)
            {
                yield return (IAutomation)Activator.CreateInstance(automation, new Object[] { hub, evt });
            }
        }
    }
}
