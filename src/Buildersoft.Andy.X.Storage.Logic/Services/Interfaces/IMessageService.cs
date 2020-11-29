using Buildersoft.Andy.X.Storage.Data.Model.Events.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services.Interfaces
{
    public interface IMessageService
    {
        void StoreMessage(MessageStoredArgs messageStoredArgs);
    }
}
