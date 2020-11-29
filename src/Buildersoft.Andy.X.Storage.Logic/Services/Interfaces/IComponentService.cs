using Buildersoft.Andy.X.Storage.Data.Model.Events.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services.Interfaces
{
    public interface IComponentService
    {
        void CreateComponent(ComponentCreatedArgs componentCreatedArgs);
        Component UpdateComponent(ComponentUpdatedArgs componentDeletedArgs);
        Component ReadComponent(ComponentReadArgs componentReadArgs);
        ConcurrentDictionary<string, Component> ReadComponents();
        bool DeleteComponent(ComponentDeletedArgs componentDeletedArgs);
    }
}
